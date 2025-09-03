using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace FluentHyperV.Cloudinit.Models;

/// <summary>
/// Represents a complete cloud-init configuration that can be serialized to YAML
/// </summary>
public class CloudInitConfiguration
{
    /// <summary>
    /// Hostname for the system
    /// </summary>
    [YamlMember(Alias = "hostname")]
    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }

    /// <summary>
    /// Fully qualified domain name
    /// </summary>
    [YamlMember(Alias = "fqdn")]
    [JsonPropertyName("fqdn")]
    public string? Fqdn { get; set; }

    /// <summary>
    /// System timezone
    /// </summary>
    [YamlMember(Alias = "timezone")]
    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    /// <summary>
    /// System locale
    /// </summary>
    [YamlMember(Alias = "locale")]
    [JsonPropertyName("locale")]
    public string? Locale { get; set; }

    /// <summary>
    /// Keyboard configuration
    /// </summary>
    [YamlMember(Alias = "keyboard")]
    [JsonPropertyName("keyboard")]
    public KeyboardConfig? Keyboard { get; set; }

    /// <summary>
    /// User accounts configuration
    /// </summary>
    [YamlMember(Alias = "users")]
    [JsonPropertyName("users")]
    public List<UserConfig>? Users { get; set; }

    /// <summary>
    /// Packages to install
    /// </summary>
    [YamlMember(Alias = "packages")]
    [JsonPropertyName("packages")]
    public List<string>? Packages { get; set; }

    /// <summary>
    /// Package sources (apt repositories)
    /// </summary>
    [YamlMember(Alias = "apt")]
    [JsonPropertyName("apt")]
    public AptConfig? Apt { get; set; }

    /// <summary>
    /// Commands to run after system setup
    /// </summary>
    [YamlMember(Alias = "runcmd")]
    [JsonPropertyName("runcmd")]
    public List<string>? RunCommands { get; set; }

    /// <summary>
    /// Network configuration
    /// </summary>
    [YamlMember(Alias = "network")]
    [JsonPropertyName("network")]
    public NetworkConfig? Network { get; set; }

    /// <summary>
    /// SSH configuration
    /// </summary>
    [YamlMember(Alias = "ssh_deletekeys")]
    [JsonPropertyName("ssh_deletekeys")]
    public bool? SshDeleteKeys { get; set; }

    /// <summary>
    /// SSH key types to generate
    /// </summary>
    [YamlMember(Alias = "ssh_genkeytypes")]
    [JsonPropertyName("ssh_genkeytypes")]
    public List<string>? SshGenKeyTypes { get; set; }

    /// <summary>
    /// Power state configuration after cloud-init completion
    /// </summary>
    [YamlMember(Alias = "power_state")]
    [JsonPropertyName("power_state")]
    public PowerStateConfig? PowerState { get; set; }

    /// <summary>
    /// Write files configuration
    /// </summary>
    [YamlMember(Alias = "write_files")]
    [JsonPropertyName("write_files")]
    public List<WriteFileConfig>? WriteFiles { get; set; }
}

/// <summary>
/// Keyboard configuration
/// </summary>
public class KeyboardConfig
{
    [YamlMember(Alias = "layout")]
    [JsonPropertyName("layout")]
    public string? Layout { get; set; }

    [YamlMember(Alias = "model")]
    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [YamlMember(Alias = "options")]
    [JsonPropertyName("options")]
    public string? Options { get; set; }
}

/// <summary>
/// User account configuration
/// </summary>
public class UserConfig
{
    [YamlMember(Alias = "name")]
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [YamlMember(Alias = "groups")]
    [JsonPropertyName("groups")]
    public string? Groups { get; set; }

    [YamlMember(Alias = "sudo")]
    [JsonPropertyName("sudo")]
    public string? Sudo { get; set; }

    [YamlMember(Alias = "shell")]
    [JsonPropertyName("shell")]
    public string? Shell { get; set; }

    [YamlMember(Alias = "lock_passwd")]
    [JsonPropertyName("lock_passwd")]
    public bool? LockPasswd { get; set; }

    [YamlMember(Alias = "passwd")]
    [JsonPropertyName("passwd")]
    public string? Passwd { get; set; }

    [YamlMember(Alias = "ssh_authorized_keys")]
    [JsonPropertyName("ssh_authorized_keys")]
    public List<string>? SshAuthorizedKeys { get; set; }
}

/// <summary>
/// APT package manager configuration
/// </summary>
public class AptConfig
{
    [YamlMember(Alias = "sources")]
    [JsonPropertyName("sources")]
    public Dictionary<string, AptSourceConfig>? Sources { get; set; }

    [YamlMember(Alias = "preserve_sources_list")]
    [JsonPropertyName("preserve_sources_list")]
    public bool? PreserveSourcesList { get; set; }

    [YamlMember(Alias = "primary")]
    [JsonPropertyName("primary")]
    public List<AptPrimaryConfig>? Primary { get; set; }
}

/// <summary>
/// APT source configuration
/// </summary>
public class AptSourceConfig
{
    [YamlMember(Alias = "source")]
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    [YamlMember(Alias = "key")]
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [YamlMember(Alias = "keyid")]
    [JsonPropertyName("keyid")]
    public string? KeyId { get; set; }

    [YamlMember(Alias = "keyserver")]
    [JsonPropertyName("keyserver")]
    public string? KeyServer { get; set; }
}

/// <summary>
/// APT primary source configuration
/// </summary>
public class AptPrimaryConfig
{
    [YamlMember(Alias = "arches")]
    [JsonPropertyName("arches")]
    public List<string>? Arches { get; set; }

    [YamlMember(Alias = "uri")]
    [JsonPropertyName("uri")]
    public string? Uri { get; set; }
}

/// <summary>
/// Network configuration
/// </summary>
public class NetworkConfig
{
    [YamlMember(Alias = "version")]
    [JsonPropertyName("version")]
    public int? Version { get; set; }

    [YamlMember(Alias = "ethernets")]
    [JsonPropertyName("ethernets")]
    public Dictionary<string, EthernetConfig>? Ethernets { get; set; }
}

/// <summary>
/// Ethernet interface configuration
/// </summary>
public class EthernetConfig
{
    [YamlMember(Alias = "dhcp4")]
    [JsonPropertyName("dhcp4")]
    public bool? Dhcp4 { get; set; }

    [YamlMember(Alias = "dhcp6")]
    [JsonPropertyName("dhcp6")]
    public bool? Dhcp6 { get; set; }

    [YamlMember(Alias = "addresses")]
    [JsonPropertyName("addresses")]
    public List<string>? Addresses { get; set; }

    [YamlMember(Alias = "gateway4")]
    [JsonPropertyName("gateway4")]
    public string? Gateway4 { get; set; }

    [YamlMember(Alias = "nameservers")]
    [JsonPropertyName("nameservers")]
    public NameserversConfig? Nameservers { get; set; }
}

/// <summary>
/// Name servers configuration
/// </summary>
public class NameserversConfig
{
    [YamlMember(Alias = "addresses")]
    [JsonPropertyName("addresses")]
    public List<string>? Addresses { get; set; }
}

/// <summary>
/// Power state configuration
/// </summary>
public class PowerStateConfig
{
    [YamlMember(Alias = "mode")]
    [JsonPropertyName("mode")]
    public string? Mode { get; set; }

    [YamlMember(Alias = "timeout")]
    [JsonPropertyName("timeout")]
    public int? Timeout { get; set; }

    [YamlMember(Alias = "condition")]
    [JsonPropertyName("condition")]
    public bool? Condition { get; set; }
}

/// <summary>
/// Write file configuration
/// </summary>
public class WriteFileConfig
{
    [YamlMember(Alias = "path")]
    [JsonPropertyName("path")]
    public string? Path { get; set; }

    [YamlMember(Alias = "content")]
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [YamlMember(Alias = "permissions")]
    [JsonPropertyName("permissions")]
    public string? Permissions { get; set; }

    [YamlMember(Alias = "owner")]
    [JsonPropertyName("owner")]
    public string? Owner { get; set; }
}