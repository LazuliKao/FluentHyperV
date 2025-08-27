using System.ComponentModel.DataAnnotations;

namespace FluentHyperV.Cloudinit.Models;

/// <summary>
/// Virtual Machine configuration settings
/// </summary>
public class VirtualMachineConfiguration
{
    [Required]
    public string VMName { get; set; } = "CloudVm";

    public int VMGeneration { get; set; } = 2;

    public int VMProcessorCount { get; set; } = 2;

    public bool VMDynamicMemoryEnabled { get; set; } = false;

    public ulong VMMemoryStartupBytes { get; set; } = 1024 * 1024 * 1024; // 1GB

    public ulong VMMinimumBytes { get; set; }

    public ulong VMMaximumBytes { get; set; }

    public ulong VHDSizeBytes { get; set; } = 40L * 1024 * 1024 * 1024; // 40GB

    public string? VirtualSwitchName { get; set; }

    public string? VMVlanID { get; set; }

    public string? VMNativeVlanID { get; set; }

    public string? VMAllowedVlanIDList { get; set; }

    public bool VMVMQ { get; set; } = false;

    public bool VMDhcpGuard { get; set; } = false;

    public bool VMRouterGuard { get; set; } = false;

    public bool VMPassthru { get; set; } = false;

    public bool VMMacAddressSpoofing { get; set; } = false;

    public bool VMExposeVirtualizationExtensions { get; set; } = false;

    public string? VMVersion { get; set; }

    public string VMHostname { get; set; } = string.Empty;

    public string? VMMachine_StoragePath { get; set; }

    public string? VMMachinePath { get; set; }

    public string? VMStoragePath { get; set; }

    public string? VMStaticMacAddress { get; set; }

    public bool ConvertImageToNoCloud { get; set; } = false;

    public bool ImageTypeAzure { get; set; } = false;

    public bool ShowSerialConsoleWindow { get; set; } = false;

    public bool ShowVmConnectWindow { get; set; } = false;

    public bool Force { get; set; } = false;

    public VirtualMachineConfiguration()
    {
        VMHostname = VMName;
        VMMinimumBytes = VMMemoryStartupBytes;
        VMMaximumBytes = VMMemoryStartupBytes;
    }
}
