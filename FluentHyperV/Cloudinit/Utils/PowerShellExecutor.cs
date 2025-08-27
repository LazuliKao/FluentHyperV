using System.Management.Automation.Runspaces;
using Microsoft.Extensions.Logging;

namespace FluentHyperV.Cloudinit.Utils;

/// <summary>
/// PowerShell execution utility class
/// </summary>
public class PowerShellExecutor : IDisposable
{
    private readonly ILogger<PowerShellExecutor> _logger;
    private readonly Runspace _runspace;
    private bool _disposed = false;

    public PowerShellExecutor(ILogger<PowerShellExecutor> logger)
    {
        _logger = logger;

        // Create initial session state with Hyper-V module
        var sessionState = InitialSessionState.CreateDefault();
        sessionState.ImportPSModule(new[] { "Hyper-V" });

        _runspace = RunspaceFactory.CreateRunspace(sessionState);
        _runspace.Open();
    }

    /// <summary>
    /// Execute PowerShell command and return result
    /// </summary>
    public async Task<PowerShellResult> ExecuteAsync(
        string command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Executing PowerShell command: {Command}", command);

        using var powerShell = System.Management.Automation.PowerShell.Create();
        powerShell.Runspace = _runspace;

        try
        {
            powerShell.AddScript(command);

            var results = await Task.Run(() => powerShell.Invoke(), cancellationToken);
            var errors = powerShell.Streams.Error.ToList();
            var warnings = powerShell.Streams.Warning.ToList();

            var success = !powerShell.HadErrors;

            if (!success)
            {
                _logger.LogError("PowerShell command failed: {Command}", command);
                foreach (var error in errors)
                {
                    _logger.LogError("PowerShell Error: {Error}", error.ToString());
                }
            }

            foreach (var warning in warnings)
            {
                _logger.LogWarning("PowerShell Warning: {Warning}", warning.ToString());
            }

            return new PowerShellResult
            {
                Success = success,
                Output =
                    results?.Select(r => r?.ToString() ?? string.Empty).ToList()
                    ?? new List<string>(),
                Errors = errors.Select(e => e.ToString()).ToList(),
                Warnings = warnings.Select(w => w.Message).ToList(),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception executing PowerShell command: {Command}", command);
            return new PowerShellResult
            {
                Success = false,
                Output = new List<string>(),
                Errors = new List<string> { ex.Message },
                Warnings = new List<string>(),
            };
        }
    }

    /// <summary>
    /// Execute PowerShell script file
    /// </summary>
    public async Task<PowerShellResult> ExecuteScriptAsync(
        string scriptPath,
        Dictionary<string, object>? parameters = null,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Executing PowerShell script: {ScriptPath}", scriptPath);

        if (!File.Exists(scriptPath))
        {
            _logger.LogError("Script file not found: {ScriptPath}", scriptPath);
            return new PowerShellResult
            {
                Success = false,
                Output = new List<string>(),
                Errors = new List<string> { $"Script file not found: {scriptPath}" },
                Warnings = new List<string>(),
            };
        }

        using var powerShell = System.Management.Automation.PowerShell.Create();
        powerShell.Runspace = _runspace;

        try
        {
            powerShell.AddCommand(scriptPath);

            if (parameters != null)
            {
                foreach (var param in parameters)
                {
                    powerShell.AddParameter(param.Key, param.Value);
                }
            }

            var results = await Task.Run(() => powerShell.Invoke(), cancellationToken);
            var errors = powerShell.Streams.Error.ToList();
            var warnings = powerShell.Streams.Warning.ToList();

            var success = !powerShell.HadErrors;

            return new PowerShellResult
            {
                Success = success,
                Output =
                    results?.Select(r => r?.ToString() ?? string.Empty).ToList()
                    ?? new List<string>(),
                Errors = errors.Select(e => e.ToString()).ToList(),
                Warnings = warnings.Select(w => w.Message).ToList(),
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception executing PowerShell script: {ScriptPath}", scriptPath);
            return new PowerShellResult
            {
                Success = false,
                Output = new List<string>(),
                Errors = new List<string> { ex.Message },
                Warnings = new List<string>(),
            };
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _runspace?.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// PowerShell execution result
/// </summary>
public class PowerShellResult
{
    public bool Success { get; set; }
    public List<string> Output { get; set; } = new();
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}
