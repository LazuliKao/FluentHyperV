using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text.Json;

namespace FluentHyperV.Powershell;

public class PowerShellInstance : IDisposable
{
    private readonly System.Management.Automation.PowerShell _powerShell;

    public PowerShellInstance(string? setupScript = null)
    {
        var runspace = RunspaceFactory.CreateRunspace();
        runspace.Open();
        Runspace.DefaultRunspace = runspace;
        _powerShell = System.Management.Automation.PowerShell.Create();
        if (setupScript is not null)
        {
            _powerShell.AddScript(setupScript);
            _powerShell.Invoke();
        }
    }

    public void Dispose() => _powerShell?.Dispose();

    #region PowerShell Execution

    public void ExecuteScript(string script)
    {
        _powerShell.AddScript(script);
        _powerShell.Invoke();
    }

    public T? InvokeFunction<T>(string name, Dictionary<string, object>? parameters = null)
    {
        var result = InvokeFunctionToJson(name, parameters);
        return result == null ? default : result.Deserialize<T>();
    }

    public JsonDocument? InvokeFunctionToJson(
        string name,
        Dictionary<string, object>? parameters = null
    )
    {
        var result = InvokeFunction(name, parameters);
        if (result == null)
            return null;

        throw new NotImplementedException();
    }

    public PSObject? InvokeFunction(string name, Dictionary<string, object>? parameters = null)
    {
        _powerShell.AddCommand(name);
        if (parameters is not null)
            _powerShell.AddParameters(parameters);
        var results = _powerShell.Invoke();
        if (results.Count > 0)
            return results[0];
        return null;
    }

    #endregion
}
