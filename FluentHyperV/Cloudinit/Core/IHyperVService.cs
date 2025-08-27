using FluentHyperV.Cloudinit.Models;

namespace FluentHyperV.Cloudinit.Core;

/// <summary>
/// Interface for Hyper-V operations
/// </summary>
public interface IHyperVService
{
    /// <summary>
    /// Create a new virtual machine with the specified configuration
    /// </summary>
    Task<bool> CreateVirtualMachineAsync(
        VirtualMachineConfiguration vmConfig,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Start a virtual machine
    /// </summary>
    Task<bool> StartVirtualMachineAsync(
        string vmName,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Stop a virtual machine
    /// </summary>
    Task<bool> StopVirtualMachineAsync(
        string vmName,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Remove a virtual machine
    /// </summary>
    Task<bool> RemoveVirtualMachineAsync(
        string vmName,
        bool force = false,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Check if a virtual machine exists
    /// </summary>
    Task<bool> VirtualMachineExistsAsync(
        string vmName,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Set advanced VM settings
    /// </summary>
    Task<bool> SetAdvancedSettingsAsync(
        string vmName,
        VirtualMachineConfiguration config,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Compact a VHD file
    /// </summary>
    Task<bool> CompactVHDAsync(string vhdPath, CancellationToken cancellationToken = default);
}
