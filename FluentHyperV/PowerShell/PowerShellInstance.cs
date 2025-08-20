using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text.Json;
using FluentHyperV.HyperV;
using FluentHyperV.SourceGenerator;

namespace FluentHyperV.PowerShell;

public class PowerShellInstance : IDisposable
{
    private readonly Func<System.Management.Automation.PowerShell> _powerShell;

    private readonly RunspacePool? _pool;

    public PowerShellInstance(string? setupScript = null)
    {
        _pool = RunspaceFactory.CreateRunspacePool(1, 5);
        _pool.Open();
        _powerShell = () =>
        {
            var ps = System.Management.Automation.PowerShell.Create();
            ps.RunspacePool = _pool;
            if (setupScript is not null)
                ps.AddStatement().AddScript(setupScript).Invoke();
            return ps;
        };
    }

    #region PowerShell Execution

    public void ExecuteScript(string script)
    {
        using var ps = _powerShell();
        ps.AddStatement().AddScript(script).Invoke();
    }

    public T[] InvokeFunction<T>(string name, Dictionary<string, object>? parameters = null)
    {
        using var ps = _powerShell();
        ps.AddStatement().AddCommand(name);
        if (parameters is not null)
            foreach (var (k, v) in parameters)
            {
                ps.AddParameter(k, v);
            }

        // ps.AddParameters(parameters);
        var results = ps.Invoke<T>();
        if (ps.HadErrors)
        {
            var error = ps.Streams.Error[0];
            throw new InvalidOperationException(
                $"Error invoking PowerShell command '{name}': {error}"
            );
        }

        if (results.Count > 0)
            return results.ToArray();
        return [];
    }

    public T[] InvokeFunctionLoad<T>(
        string name,
        Dictionary<string, object>? parameters = null,
        Action<Exception>? onError = null
    )
        where T : IPSObjectMapper, new()
    {
        var result = InvokeFunction(name, parameters);
        return result
            .Select(x =>
            {
                var model = new T();
                model.LoadFrom(x, onError);
                return model;
            })
            .ToArray();
    }

    public PSObject[] InvokeFunction(string name, Dictionary<string, object>? parameters = null)
    {
        using var ps = _powerShell();
        var cmd = ps.AddStatement().AddCommand(name);
        if (parameters is not null)
            cmd.AddParameters(parameters);
        var results = cmd.Invoke();
        if (cmd.HadErrors)
        {
            var error = cmd.Streams.Error[0];
            throw new InvalidOperationException(
                $"Error invoking PowerShell command '{name}': {error}"
            );
        }

        if (results.Count > 0)
            return results.ToArray();
        return [];
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
        Dictionary<string, object>? parameters = null,
        int depth = 2
    )
    {
        using var ps = _powerShell();
        var cmd = ps.AddStatement().AddCommand(name);
        if (parameters is not null)
            cmd.AddParameters(parameters);
        cmd.AddCommand("ConvertTo-Json");
        cmd.AddParameter("Depth", depth);
        cmd.AddParameter("Compress", true);
        var results = cmd.Invoke();
        if (cmd.HadErrors)
        {
            var error = cmd.Streams.Error[0];
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

    public void Dispose()
    {
        _pool?.Dispose();
    }
}
