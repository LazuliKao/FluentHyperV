using System.ComponentModel.DataAnnotations;

namespace FluentHyperV.Cloudinit.Models;

/// <summary>
/// Command line arguments configuration
/// </summary>
public class CommandLineOptions
{
    [Required]
    public string VMName { get; set; } = "CloudVm";

    public int VMGeneration { get; set; } = 2;

    public int VMProcessorCount { get; set; } = 2;

    public string VMMemory { get; set; } = "1GB";

    public string VHDSize { get; set; } = "40GB";

    public string ImageVersion { get; set; } = "22.04";

    public string? VirtualSwitchName { get; set; }

    public string? NetworkAddress { get; set; }

    public string? NetworkGateway { get; set; }

    public string DnsServers { get; set; } = "1.1.1.1,1.0.0.1";

    public string Username { get; set; } = "admin";

    public string Password { get; set; } = "Passw0rd";

    public string? Hostname { get; set; }

    public string Timezone { get; set; } = "UTC";

    public string Locale { get; set; } = "en_US";

    public string? StoragePath { get; set; }

    public string? CachePath { get; set; }

    public string? CustomUserData { get; set; }

    public bool ShowConsole { get; set; } = false;

    public bool ShowVmConnect { get; set; } = false;

    public bool Force { get; set; } = false;

    public bool Verbose { get; set; } = false;

    public bool PreInstallDocker { get; set; } = false;

    public bool PreInstallGnomeDesktop { get; set; } = false;
}
