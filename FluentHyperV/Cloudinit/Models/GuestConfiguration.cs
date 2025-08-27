namespace FluentHyperV.Cloudinit.Models;

/// <summary>
/// Guest OS configuration settings
/// </summary>
public class GuestConfiguration
{
    public string DomainName { get; set; } = "domain.local";

    public string KeyboardLayout { get; set; } = "us";

    public string? KeyboardModel { get; set; }

    public string? KeyboardOptions { get; set; }

    public string Locale { get; set; } = "en_US";

    public string TimeZone { get; set; } = "UTC";

    public string CloudInitPowerState { get; set; } = "reboot";

    public string GuestAdminUsername { get; set; } = "admin";

    public string GuestAdminPassword { get; set; } = "Passw0rd";

    public string? GuestAdminSshPubKey { get; set; }

    public string? GuestAdminSshPubKeyFile { get; set; }

    public string LOGO { get; set; } = "";

    public bool PreInstallDocker { get; set; } = false;

    public bool PreInstallGnomeDesktop { get; set; } = false;
}
