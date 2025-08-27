using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace FluentHyperV.Cloudinit.Utils;

/// <summary>
/// Utility for executing external processes
/// </summary>
public class ProcessExecutor
{
    private readonly ILogger<ProcessExecutor> _logger;

    public ProcessExecutor(ILogger<ProcessExecutor> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Execute external process and return result
    /// </summary>
    public async Task<ProcessResult> ExecuteAsync(
        string fileName,
        string arguments,
        string? workingDirectory = null,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Executing process: {FileName} {Arguments}", fileName, arguments);

        var processStartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };

        if (!string.IsNullOrEmpty(workingDirectory))
        {
            processStartInfo.WorkingDirectory = workingDirectory;
        }

        try
        {
            using var process = new Process { StartInfo = processStartInfo };

            var outputBuilder = new List<string>();
            var errorBuilder = new List<string>();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.Add(e.Data);
                    _logger.LogTrace("Process Output: {Data}", e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.Add(e.Data);
                    _logger.LogTrace("Process Error: {Data}", e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            var success = process.ExitCode == 0;

            if (!success)
            {
                _logger.LogError(
                    "Process failed with exit code {ExitCode}: {FileName} {Arguments}",
                    process.ExitCode,
                    fileName,
                    arguments
                );
            }

            return new ProcessResult
            {
                Success = success,
                ExitCode = process.ExitCode,
                Output = outputBuilder,
                Errors = errorBuilder,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Exception executing process: {FileName} {Arguments}",
                fileName,
                arguments
            );
            return new ProcessResult
            {
                Success = false,
                ExitCode = -1,
                Output = new List<string>(),
                Errors = new List<string> { ex.Message },
            };
        }
    }
}

/// <summary>
/// Process execution result
/// </summary>
public class ProcessResult
{
    public bool Success { get; set; }
    public int ExitCode { get; set; }
    public List<string> Output { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
