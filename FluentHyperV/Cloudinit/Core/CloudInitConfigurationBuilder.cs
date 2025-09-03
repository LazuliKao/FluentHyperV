using FluentHyperV.Cloudinit.Models;

namespace FluentHyperV.Cloudinit.Core;

/// <summary>
/// Fluent builder for creating cloud-init configurations
/// </summary>
public class CloudInitConfigurationBuilder
{
    private readonly CloudInitConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the CloudInitConfigurationBuilder
    /// </summary>
    public CloudInitConfigurationBuilder()
    {
        _configuration = new CloudInitConfiguration();
    }

    /// <summary>
    /// Sets the hostname for the system
    /// </summary>
    /// <param name="hostname">The hostname to set</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithHostname(string hostname)
    {
        _configuration.Hostname = hostname;
        return this;
    }

    /// <summary>
    /// Sets the fully qualified domain name
    /// </summary>
    /// <param name="fqdn">The FQDN to set</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithFqdn(string fqdn)
    {
        _configuration.Fqdn = fqdn;
        return this;
    }

    /// <summary>
    /// Sets the system timezone
    /// </summary>
    /// <param name="timezone">The timezone to set (e.g., "UTC", "America/New_York")</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithTimezone(string timezone)
    {
        _configuration.Timezone = timezone;
        return this;
    }

    /// <summary>
    /// Sets the system locale
    /// </summary>
    /// <param name="locale">The locale to set (e.g., "en_US", "zh_CN")</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithLocale(string locale)
    {
        _configuration.Locale = locale;
        return this;
    }

    /// <summary>
    /// Configures the keyboard settings
    /// </summary>
    /// <param name="layout">Keyboard layout (e.g., "us", "gb")</param>
    /// <param name="model">Keyboard model (default: "pc105")</param>
    /// <param name="options">Additional keyboard options</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithKeyboard(string layout, string? model = "pc105", string? options = null)
    {
        _configuration.Keyboard = new KeyboardConfig
        {
            Layout = layout,
            Model = model,
            Options = options
        };
        return this;
    }

    /// <summary>
    /// Adds a user account to the configuration
    /// </summary>
    /// <param name="name">Username</param>
    /// <param name="password">User password (will be hashed)</param>
    /// <param name="groups">Groups to add user to (default: "sudo")</param>
    /// <param name="sudo">Sudo privileges (default: "ALL=(ALL) NOPASSWD:ALL")</param>
    /// <param name="shell">User shell (default: "/bin/bash")</param>
    /// <param name="sshKeys">SSH public keys for the user</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithUser(
        string name, 
        string? password = null, 
        string groups = "sudo", 
        string sudo = "ALL=(ALL) NOPASSWD:ALL",
        string shell = "/bin/bash",
        params string[] sshKeys)
    {
        _configuration.Users ??= new List<UserConfig>();
        
        _configuration.Users.Add(new UserConfig
        {
            Name = name,
            Groups = groups,
            Sudo = sudo,
            Shell = shell,
            LockPasswd = false,
            Passwd = password,
            SshAuthorizedKeys = sshKeys?.ToList()
        });
        
        return this;
    }

    /// <summary>
    /// Adds packages to be installed
    /// </summary>
    /// <param name="packages">Package names to install</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithPackages(params string[] packages)
    {
        _configuration.Packages ??= new List<string>();
        _configuration.Packages.AddRange(packages);
        return this;
    }

    /// <summary>
    /// Adds an APT repository source
    /// </summary>
    /// <param name="name">Repository name</param>
    /// <param name="source">Repository source line</param>
    /// <param name="key">GPG key for the repository</param>
    /// <param name="keyId">GPG key ID</param>
    /// <param name="keyServer">GPG key server</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithAptSource(
        string name, 
        string source, 
        string? key = null, 
        string? keyId = null, 
        string? keyServer = null)
    {
        _configuration.Apt ??= new AptConfig();
        _configuration.Apt.Sources ??= new Dictionary<string, AptSourceConfig>();
        
        _configuration.Apt.Sources[name] = new AptSourceConfig
        {
            Source = source,
            Key = key,
            KeyId = keyId,
            KeyServer = keyServer
        };
        
        return this;
    }

    /// <summary>
    /// Adds commands to run after system setup
    /// </summary>
    /// <param name="commands">Commands to execute</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithRunCommands(params string[] commands)
    {
        _configuration.RunCommands ??= new List<string>();
        _configuration.RunCommands.AddRange(commands);
        return this;
    }

    /// <summary>
    /// Configures network with DHCP
    /// </summary>
    /// <param name="interfaceName">Network interface name (default: "eth0")</param>
    /// <param name="dhcp4">Enable DHCPv4 (default: true)</param>
    /// <param name="dhcp6">Enable DHCPv6 (default: false)</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithNetworkDhcp(string interfaceName = "eth0", bool dhcp4 = true, bool dhcp6 = false)
    {
        _configuration.Network ??= new NetworkConfig { Version = 2 };
        _configuration.Network.Ethernets ??= new Dictionary<string, EthernetConfig>();
        
        _configuration.Network.Ethernets[interfaceName] = new EthernetConfig
        {
            Dhcp4 = dhcp4,
            Dhcp6 = dhcp6
        };
        
        return this;
    }

    /// <summary>
    /// Configures network with static IP
    /// </summary>
    /// <param name="interfaceName">Network interface name</param>
    /// <param name="ipAddress">Static IP address with CIDR (e.g., "192.168.1.100/24")</param>
    /// <param name="gateway">Gateway IP address</param>
    /// <param name="nameservers">DNS server addresses</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithNetworkStatic(
        string interfaceName, 
        string ipAddress, 
        string? gateway = null, 
        params string[] nameservers)
    {
        _configuration.Network ??= new NetworkConfig { Version = 2 };
        _configuration.Network.Ethernets ??= new Dictionary<string, EthernetConfig>();
        
        var ethernetConfig = new EthernetConfig
        {
            Dhcp4 = false,
            Dhcp6 = false,
            Addresses = new List<string> { ipAddress }
        };

        if (!string.IsNullOrEmpty(gateway))
        {
            ethernetConfig.Gateway4 = gateway;
        }

        if (nameservers?.Length > 0)
        {
            ethernetConfig.Nameservers = new NameserversConfig
            {
                Addresses = nameservers.ToList()
            };
        }

        _configuration.Network.Ethernets[interfaceName] = ethernetConfig;
        return this;
    }

    /// <summary>
    /// Configures SSH settings
    /// </summary>
    /// <param name="deleteKeys">Whether to delete existing SSH host keys</param>
    /// <param name="keyTypes">SSH key types to generate</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithSshConfig(bool deleteKeys = true, params string[] keyTypes)
    {
        _configuration.SshDeleteKeys = deleteKeys;
        
        if (keyTypes?.Length > 0)
        {
            _configuration.SshGenKeyTypes = keyTypes.ToList();
        }
        else
        {
            _configuration.SshGenKeyTypes = new List<string> { "rsa", "ecdsa", "ed25519" };
        }
        
        return this;
    }

    /// <summary>
    /// Configures power state after cloud-init completion
    /// </summary>
    /// <param name="mode">Power state mode ("reboot", "poweroff", "halt")</param>
    /// <param name="timeout">Timeout in seconds</param>
    /// <param name="condition">Whether to execute the power state change</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithPowerState(string mode = "reboot", int timeout = 30, bool condition = true)
    {
        _configuration.PowerState = new PowerStateConfig
        {
            Mode = mode,
            Timeout = timeout,
            Condition = condition
        };
        
        return this;
    }

    /// <summary>
    /// Adds a file to be written to the system
    /// </summary>
    /// <param name="path">File path</param>
    /// <param name="content">File content</param>
    /// <param name="permissions">File permissions (e.g., "0644")</param>
    /// <param name="owner">File owner (e.g., "root:root")</param>
    /// <returns>The builder instance for method chaining</returns>
    public CloudInitConfigurationBuilder WithWriteFile(
        string path, 
        string content, 
        string permissions = "0644", 
        string owner = "root:root")
    {
        _configuration.WriteFiles ??= new List<WriteFileConfig>();
        
        _configuration.WriteFiles.Add(new WriteFileConfig
        {
            Path = path,
            Content = content,
            Permissions = permissions,
            Owner = owner
        });
        
        return this;
    }

    /// <summary>
    /// Builds the cloud-init configuration
    /// </summary>
    /// <returns>The configured CloudInitConfiguration instance</returns>
    public CloudInitConfiguration Build()
    {
        return _configuration;
    }

    /// <summary>
    /// Creates a new CloudInitConfigurationBuilder instance
    /// </summary>
    /// <returns>A new builder instance</returns>
    public static CloudInitConfigurationBuilder Create()
    {
        return new CloudInitConfigurationBuilder();
    }
}