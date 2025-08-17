using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text.Json;
using FluentHyperV.SourceGenerator;

namespace FluentHyperV.PowerShell;

public class PowerShellInstance
{
    private readonly Func<System.Management.Automation.PowerShell> _powerShell;

    public PowerShellInstance(string? setupScript = null)
    {
        var runspace = RunspaceFactory.CreateRunspace();
        runspace.Open();
        Runspace.DefaultRunspace = runspace;
        _powerShell = () =>
        {
            var ps = System.Management.Automation.PowerShell.Create();
            if (setupScript is not null)
                ps.AddScript(setupScript);
            return ps;
        };
    }

    #region PowerShell Execution

    public void ExecuteScript(string script)
    {
        using var ps = _powerShell();
        ps.AddScript(script);
        ps.Invoke();
    }

    public T? InvokeFunction<T>(
        string name,
        Dictionary<string, object>? parameters = null,
        Action<Exception>? onError = null
    )
        where T : IPSObjectMapper, new()
    {
        var result = InvokeFunction(name, parameters);
        if (result is null)
            return default;
        var model = new T();
        model.LoadFrom(result, onError);
        return model;
    }

    public PSObject? InvokeFunction(string name, Dictionary<string, object>? parameters = null)
    {
        using var ps = _powerShell();
        ps.AddCommand(name);
        if (parameters is not null)
            ps.AddParameters(parameters);
        var results = ps.Invoke();
        if (ps.HadErrors)
        {
            var error = ps.Streams.Error[0];
            throw new InvalidOperationException(
                $"Error invoking PowerShell command '{name}': {error}"
            );
        }

        if (results.Count > 0)
            return results[0];
        return null;
    }

    public T? InvokeFunctionJson<T>(string name, Dictionary<string, object>? parameters = null)
        where T : new()
    {
        var jsonDoc = InvokeFunctionJson(name, parameters);
        if (jsonDoc is null)
            return default;
        return jsonDoc.Deserialize<T>();
    }

    public JsonDocument? InvokeFunctionJson(
        string name,
        Dictionary<string, object>? parameters = null
    )
    {
        using var ps = _powerShell();
        ps.AddCommand(name);
        if (parameters is not null)
            ps.AddParameters(parameters);
        ps.AddCommand("ConvertTo-Json");
        ps.AddParameter("Depth", 10);
        ps.AddParameter("Compress", true);
        var results = ps.Invoke();
        if (ps.HadErrors)
        {
            var error = ps.Streams.Error[0];
            throw new InvalidOperationException(
                $"Error invoking PowerShell command '{name}': {error}"
            );
        }

        if (results.Count == 0)
            return null;
        var jsonString = results[0].ToString();
        if (string.IsNullOrEmpty(jsonString))
            return null;
        try
        {
            return JsonDocument.Parse(jsonString);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"Error parsing JSON from PowerShell command '{name}': {ex.Message}",
                ex
            );
        }
    }

    #endregion
}
