using FluentHyperV.Cloudinit.Core;
using FluentHyperV.Cloudinit.Models;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace FluentHyperV.UnitTest;

/// <summary>
/// Simple test to verify the basic functionality of CloudInitConfigurationBuilder
/// without external dependencies
/// </summary>
public class SimpleCloudInitTest
{
    [Fact]
    public void CloudInitConfigurationBuilder_BasicTest_ShouldWork()
    {
        // Arrange & Act
        var config = CloudInitConfigurationBuilder.Create()
            .WithHostname("test-server")
            .WithTimezone("UTC")
            .WithUser("admin", "password123")
            .WithPackages("curl", "wget", "nginx")
            .WithNetworkDhcp("eth0")
            .Build();

        // Assert
        Assert.Equal("test-server", config.Hostname);
        Assert.Equal("UTC", config.Timezone);
        Assert.NotNull(config.Users);
        Assert.Single(config.Users);
        Assert.Equal("admin", config.Users[0].Name);
        Assert.NotNull(config.Packages);
        Assert.Contains("nginx", config.Packages);
        Assert.NotNull(config.Network);
        Assert.NotNull(config.Network.Ethernets);
        Assert.True(config.Network.Ethernets.ContainsKey("eth0"));
    }

    [Fact]
    public void CloudInitConfiguration_SerializesToYaml_ShouldWork()
    {
        // Arrange
        var config = CloudInitConfigurationBuilder.Create()
            .WithHostname("yaml-test")
            .WithUser("yamluser")
            .WithPackages("test-package")
            .Build();

        // Act
        var serializer = new SerializerBuilder()
            .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.UnderscoredNamingConvention.Instance)
            .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
            .Build();

        var yaml = "#cloud-config\n" + serializer.Serialize(config);

        // Assert
        Assert.NotNull(yaml);
        Assert.StartsWith("#cloud-config", yaml);
        Assert.Contains("yaml-test", yaml);
        Assert.Contains("yamluser", yaml);
        Assert.Contains("test-package", yaml);
    }

    [Fact]
    public void CloudInitConfiguration_SerializesToJson_ShouldWork()
    {
        // Arrange
        var config = CloudInitConfigurationBuilder.Create()
            .WithHostname("json-test")
            .WithUser("jsonuser")
            .WithPackages("json-package")
            .Build();

        // Act
        var json = JsonConvert.SerializeObject(config, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        });

        // Assert
        Assert.NotNull(json);
        Assert.Contains("json-test", json);
        Assert.Contains("jsonuser", json);
        Assert.Contains("json-package", json);
    }

    [Fact]
    public void CloudInitConfigurationBuilder_ComplexConfiguration_ShouldBuildCorrectly()
    {
        // Arrange & Act
        var config = CloudInitConfigurationBuilder.Create()
            .WithHostname("complex-server")
            .WithFqdn("complex-server.example.com")
            .WithTimezone("America/New_York")
            .WithLocale("en_US.UTF-8")
            .WithKeyboard("us", "pc105")
            .WithUser("admin", "SecurePass123", "sudo", sshKeys: "ssh-rsa AAAAB3NzaC1yc2E...")
            .WithUser("service", null, "service")
            .WithPackages("nginx", "htop", "curl", "docker.io")
            .WithAptSource("docker", "deb [arch=amd64] https://download.docker.com/linux/ubuntu focal stable")
            .WithRunCommands("systemctl enable nginx", "systemctl start nginx")
            .WithNetworkStatic("eth0", "192.168.1.100/24", "192.168.1.1", "8.8.8.8", "8.8.4.4")
            .WithSshConfig(true, "rsa", "ed25519")
            .WithPowerState("reboot", 30, true)
            .WithWriteFile("/etc/motd", "Welcome to the server!", "0644", "root:root")
            .Build();

        // Assert - Check all major components
        Assert.Equal("complex-server", config.Hostname);
        Assert.Equal("complex-server.example.com", config.Fqdn);
        Assert.Equal("America/New_York", config.Timezone);
        Assert.Equal("en_US.UTF-8", config.Locale);

        // Keyboard
        Assert.NotNull(config.Keyboard);
        Assert.Equal("us", config.Keyboard.Layout);
        Assert.Equal("pc105", config.Keyboard.Model);

        // Users
        Assert.NotNull(config.Users);
        Assert.Equal(2, config.Users.Count);
        Assert.Equal("admin", config.Users[0].Name);
        Assert.Equal("service", config.Users[1].Name);

        // Packages
        Assert.NotNull(config.Packages);
        Assert.Contains("nginx", config.Packages);
        Assert.Contains("docker.io", config.Packages);

        // APT Sources
        Assert.NotNull(config.Apt);
        Assert.NotNull(config.Apt.Sources);
        Assert.True(config.Apt.Sources.ContainsKey("docker"));

        // Run Commands
        Assert.NotNull(config.RunCommands);
        Assert.Contains("systemctl enable nginx", config.RunCommands);

        // Network
        Assert.NotNull(config.Network);
        Assert.Equal(2, config.Network.Version);
        Assert.NotNull(config.Network.Ethernets);
        Assert.True(config.Network.Ethernets.ContainsKey("eth0"));
        
        var eth0 = config.Network.Ethernets["eth0"];
        Assert.False(eth0.Dhcp4);
        Assert.NotNull(eth0.Addresses);
        Assert.Contains("192.168.1.100/24", eth0.Addresses);
        Assert.Equal("192.168.1.1", eth0.Gateway4);

        // SSH
        Assert.True(config.SshDeleteKeys);
        Assert.NotNull(config.SshGenKeyTypes);
        Assert.Contains("rsa", config.SshGenKeyTypes);

        // Power State
        Assert.NotNull(config.PowerState);
        Assert.Equal("reboot", config.PowerState.Mode);

        // Write Files
        Assert.NotNull(config.WriteFiles);
        Assert.Single(config.WriteFiles);
        Assert.Equal("/etc/motd", config.WriteFiles[0].Path);
    }
}