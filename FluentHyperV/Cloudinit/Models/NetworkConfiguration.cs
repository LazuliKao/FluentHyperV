namespace FluentHyperV.Cloudinit.Models;

/// <summary>
/// Network configuration for the VM
/// </summary>
public class NetworkConfiguration
{
    public string NetInterface { get; set; } = "eth0";

    public string? NetAddress { get; set; }

    public string? NetNetmask { get; set; }

    public string? NetNetwork { get; set; }

    public string? NetGateway { get; set; }

    public string NameServers { get; set; } = "1.1.1.1,1.0.0.1";

    public string? NetConfigType { get; set; } // ENI, v1, v2, ENI-file, dhclient

    public bool NetAutoconfig =>
        string.IsNullOrEmpty(NetAddress)
        && string.IsNullOrEmpty(NetNetmask)
        && string.IsNullOrEmpty(NetNetwork)
        && string.IsNullOrEmpty(NetGateway);
}
