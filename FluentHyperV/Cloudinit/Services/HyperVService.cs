using FluentHyperV.Cloudinit.Core;
using FluentHyperV.Cloudinit.Models;
using FluentHyperV.Cloudinit.Utils;
using Microsoft.Extensions.Logging;

namespace FluentHyperV.Cloudinit.Services;

/// <summary>
/// Hyper-V service implementation using PowerShell
/// </summary>
public class HyperVService : IHyperVService
{
    private readonly PowerShellExecutor _powerShellExecutor;
    private readonly ILogger<HyperVService> _logger;

    public HyperVService(PowerShellExecutor powerShellExecutor, ILogger<HyperVService> logger)
    {
        _powerShellExecutor = powerShellExecutor;
        _logger = logger;
    }

    public async Task<bool> CreateVirtualMachineAsync(
        VirtualMachineConfiguration vmConfig,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Creating virtual machine: {VMName}", vmConfig.VMName);

        try
        {
            // Check if VM already exists
            if (await VirtualMachineExistsAsync(vmConfig.VMName, cancellationToken))
            {
                if (!vmConfig.Force)
                {
                    _logger.LogError(
                        "Virtual machine {VMName} already exists. Use Force to overwrite.",
                        vmConfig.VMName
                    );
                    return false;
                }

                _logger.LogInformation(
                    "Removing existing virtual machine: {VMName}",
                    vmConfig.VMName
                );
                await RemoveVirtualMachineAsync(vmConfig.VMName, true, cancellationToken);
            }

            // TODO: Build PowerShell command for VM creation
            var command = BuildCreateVMCommand(vmConfig);
            var result = await _powerShellExecutor.ExecuteAsync(command, cancellationToken);

            if (!result.Success)
            {
                _logger.LogError(
                    "Failed to create virtual machine: {VMName}. Errors: {Errors}",
                    vmConfig.VMName,
                    string.Join(", ", result.Errors)
                );
                return false;
            }

            _logger.LogInformation(
                "Successfully created virtual machine: {VMName}",
                vmConfig.VMName
            );
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception creating virtual machine: {VMName}", vmConfig.VMName);
            return false;
        }
    }

    public async Task<bool> StartVirtualMachineAsync(
        string vmName,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Starting virtual machine: {VMName}", vmName);

        var command = $"Start-VM -Name '{vmName}'";
        var result = await _powerShellExecutor.ExecuteAsync(command, cancellationToken);

        if (result.Success)
        {
            _logger.LogInformation("Successfully started virtual machine: {VMName}", vmName);
        }
        else
        {
            _logger.LogError(
                "Failed to start virtual machine: {VMName}. Errors: {Errors}",
                vmName,
                string.Join(", ", result.Errors)
            );
        }

        return result.Success;
    }

    public async Task<bool> StopVirtualMachineAsync(
        string vmName,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Stopping virtual machine: {VMName}", vmName);

        var command = $"Stop-VM -Name '{vmName}' -Force";
        var result = await _powerShellExecutor.ExecuteAsync(command, cancellationToken);

        if (result.Success)
        {
            _logger.LogInformation("Successfully stopped virtual machine: {VMName}", vmName);
        }
        else
        {
            _logger.LogError(
                "Failed to stop virtual machine: {VMName}. Errors: {Errors}",
                vmName,
                string.Join(", ", result.Errors)
            );
        }

        return result.Success;
    }

    public async Task<bool> RemoveVirtualMachineAsync(
        string vmName,
        bool force = false,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Removing virtual machine: {VMName}", vmName);

        try
        {
            // Stop VM first if running
            await StopVirtualMachineAsync(vmName, cancellationToken);

            var command = $"Remove-VM -Name '{vmName}' -Force";
            var result = await _powerShellExecutor.ExecuteAsync(command, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("Successfully removed virtual machine: {VMName}", vmName);
            }
            else
            {
                _logger.LogError(
                    "Failed to remove virtual machine: {VMName}. Errors: {Errors}",
                    vmName,
                    string.Join(", ", result.Errors)
                );
            }

            return result.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception removing virtual machine: {VMName}", vmName);
            return false;
        }
    }

    public async Task<bool> VirtualMachineExistsAsync(
        string vmName,
        CancellationToken cancellationToken = default
    )
    {
        var command = $"Get-VM -Name '{vmName}' -ErrorAction SilentlyContinue";
        var result = await _powerShellExecutor.ExecuteAsync(command, cancellationToken);

        return result.Success && result.Output.Any();
    }

    public async Task<bool> SetAdvancedSettingsAsync(
        string vmName,
        VirtualMachineConfiguration config,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Setting advanced settings for VM: {VMName}", vmName);

        try
        {
            // TODO: Build PowerShell commands for advanced settings
            var commands = BuildAdvancedSettingsCommands(vmName, config);

            foreach (var command in commands)
            {
                var result = await _powerShellExecutor.ExecuteAsync(command, cancellationToken);
                if (!result.Success)
                {
                    _logger.LogWarning(
                        "Failed to execute setting command: {Command}. Errors: {Errors}",
                        command,
                        string.Join(", ", result.Errors)
                    );
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception setting advanced settings for VM: {VMName}", vmName);
            return false;
        }
    }

    public async Task<bool> CompactVHDAsync(
        string vhdPath,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Compacting VHD: {VHDPath}", vhdPath);

        var command = $"Optimize-VHD -Path '{vhdPath}' -Mode Full";
        var result = await _powerShellExecutor.ExecuteAsync(command, cancellationToken);

        if (result.Success)
        {
            _logger.LogInformation("Successfully compacted VHD: {VHDPath}", vhdPath);
        }
        else
        {
            _logger.LogError(
                "Failed to compact VHD: {VHDPath}. Errors: {Errors}",
                vhdPath,
                string.Join(", ", result.Errors)
            );
        }

        return result.Success;
    }

    private string BuildCreateVMCommand(VirtualMachineConfiguration config)
    {
        // TODO: Build complete PowerShell command for VM creation
        // This is a simplified version - the full implementation would include all parameters
        var command =
            $@"
$vmParams = @{{
    Name = '{config.VMName}'
    Generation = {config.VMGeneration}
    MemoryStartupBytes = {config.VMMemoryStartupBytes}
    ProcessorCount = {config.VMProcessorCount}
}}

if ('{config.VMMachinePath}' -ne '') {{
    $vmParams.Path = '{config.VMMachinePath}'
}}

New-VM @vmParams
";

        return command;
    }

    private List<string> BuildAdvancedSettingsCommands(
        string vmName,
        VirtualMachineConfiguration config
    )
    {
        var commands = new List<string>();

        // TODO: Add all advanced settings commands based on the original PowerShell script

        if (config.VMExposeVirtualizationExtensions)
        {
            commands.Add(
                $"Set-VMProcessor -VMName '{vmName}' -ExposeVirtualizationExtensions $true"
            );
        }

        if (!string.IsNullOrEmpty(config.VMStaticMacAddress))
        {
            commands.Add(
                $"Set-VMNetworkAdapter -VMName '{vmName}' -StaticMacAddress '{config.VMStaticMacAddress}'"
            );
        }

        // Add more settings as needed...

        return commands;
    }
}
