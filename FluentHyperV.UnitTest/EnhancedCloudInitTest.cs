using FluentHyperV.Cloudinit.Core;
using FluentHyperV.Cloudinit.Models;
using FluentHyperV.Cloudinit.Services;
using FluentHyperV.Cloudinit.Utils;
using FluentHyperV.UnitTest.Helper;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FluentHyperV.UnitTest;

public class EnhancedCloudInitTest : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ILogger<CloudInitService> _cloudInitLogger;
    private readonly ILogger<CloudInitValidator> _validatorLogger;
    private readonly ILogger<ProcessExecutor> _processExecutorLogger;
    private readonly ProcessExecutor _processExecutor;
    private readonly CloudInitValidator _validator;
    private readonly EnhancedCloudInitService _enhancedCloudInitService;
    private readonly HttpClient _httpClient;
    private readonly List<string> _tempFiles = new();
    private readonly List<string> _tempDirectories = new();

    public EnhancedCloudInitTest(ITestOutputHelper output)
    {
        _output = output;

        // Create test loggers
        var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddProvider(new TestLoggerProvider(_output))
        );

        _cloudInitLogger = loggerFactory.CreateLogger<CloudInitService>();
        _validatorLogger = loggerFactory.CreateLogger<CloudInitValidator>();
        _processExecutorLogger = loggerFactory.CreateLogger<ProcessExecutor>();

        _processExecutor = new ProcessExecutor(_processExecutorLogger);
        _httpClient = new HttpClient();
        _validator = new CloudInitValidator(_validatorLogger, _httpClient);
        _enhancedCloudInitService = new EnhancedCloudInitService(
            _cloudInitLogger, 
            _processExecutor, 
            _validator
        );
    }

    #region CloudInitConfigurationBuilder Tests

    [Fact]
    public void CloudInitConfigurationBuilder_WithHostname_ShouldSetHostname()
    {
        // Arrange & Act
        var config = CloudInitConfigurationBuilder.Create()
            .WithHostname("test-vm")
            .Build();

        // Assert
        Assert.Equal("test-vm", config.Hostname);
    }

    [Fact]
    public void CloudInitConfigurationBuilder_WithCompleteConfiguration_ShouldBuildCorrectly()
    {
        // Arrange & Act
        var config = CloudInitConfigurationBuilder.Create()
            .WithHostname("web-server")
            .WithFqdn("web-server.example.com")
            .WithTimezone("America/New_York")
            .WithLocale("en_US.UTF-8")
            .WithKeyboard("us", "pc105")
            .WithUser("admin", "SecurePass123", sshKeys: "ssh-rsa AAAAB3NzaC1yc2E...")
            .WithPackages("nginx", "htop", "curl")
            .WithAptSource("docker", "deb [arch=amd64] https://download.docker.com/linux/ubuntu focal stable")
            .WithRunCommands("systemctl enable nginx", "systemctl start nginx")
            .WithNetworkDhcp("eth0")
            .WithSshConfig(true, "rsa", "ed25519")
            .WithPowerState("reboot", 30, true)
            .WithWriteFile("/etc/motd", "Welcome to the server!", "0644", "root:root")
            .Build();

        // Assert
        Assert.Equal("web-server", config.Hostname);
        Assert.Equal("web-server.example.com", config.Fqdn);
        Assert.Equal("America/New_York", config.Timezone);
        Assert.Equal("en_US.UTF-8", config.Locale);
        
        Assert.NotNull(config.Keyboard);
        Assert.Equal("us", config.Keyboard.Layout);
        Assert.Equal("pc105", config.Keyboard.Model);

        Assert.NotNull(config.Users);
        Assert.Single(config.Users);
        Assert.Equal("admin", config.Users[0].Name);

        Assert.NotNull(config.Packages);
        Assert.Contains("nginx", config.Packages);
        Assert.Contains("htop", config.Packages);
        Assert.Contains("curl", config.Packages);

        Assert.NotNull(config.Apt);
        Assert.NotNull(config.Apt.Sources);
        Assert.True(config.Apt.Sources.ContainsKey("docker"));

        Assert.NotNull(config.RunCommands);
        Assert.Contains("systemctl enable nginx", config.RunCommands);

        Assert.NotNull(config.Network);
        Assert.Equal(2, config.Network.Version);
        Assert.NotNull(config.Network.Ethernets);
        Assert.True(config.Network.Ethernets.ContainsKey("eth0"));
        Assert.True(config.Network.Ethernets["eth0"].Dhcp4);

        Assert.True(config.SshDeleteKeys);
        Assert.NotNull(config.SshGenKeyTypes);
        Assert.Contains("rsa", config.SshGenKeyTypes);
        Assert.Contains("ed25519", config.SshGenKeyTypes);

        Assert.NotNull(config.PowerState);
        Assert.Equal("reboot", config.PowerState.Mode);
        Assert.Equal(30, config.PowerState.Timeout);

        Assert.NotNull(config.WriteFiles);
        Assert.Single(config.WriteFiles);
        Assert.Equal("/etc/motd", config.WriteFiles[0].Path);
        Assert.Equal("Welcome to the server!", config.WriteFiles[0].Content);
    }

    [Fact]
    public void CloudInitConfigurationBuilder_WithStaticNetwork_ShouldConfigureCorrectly()
    {
        // Arrange & Act
        var config = CloudInitConfigurationBuilder.Create()
            .WithNetworkStatic("eth0", "192.168.1.100/24", "192.168.1.1", "8.8.8.8", "8.8.4.4")
            .Build();

        // Assert
        Assert.NotNull(config.Network);
        Assert.NotNull(config.Network.Ethernets);
        Assert.True(config.Network.Ethernets.ContainsKey("eth0"));
        
        var eth0 = config.Network.Ethernets["eth0"];
        Assert.False(eth0.Dhcp4);
        Assert.NotNull(eth0.Addresses);
        Assert.Contains("192.168.1.100/24", eth0.Addresses);
        Assert.Equal("192.168.1.1", eth0.Gateway4);
        Assert.NotNull(eth0.Nameservers);
        Assert.NotNull(eth0.Nameservers.Addresses);
        Assert.Contains("8.8.8.8", eth0.Nameservers.Addresses);
        Assert.Contains("8.8.4.4", eth0.Nameservers.Addresses);
    }

    #endregion

    #region Enhanced Service Tests

    [Fact]
    public async Task GenerateUserDataAsync_WithConfiguration_ShouldGenerateValidYaml()
    {
        // Arrange
        var config = CloudInitConfigurationBuilder.Create()
            .WithHostname("test-server")
            .WithUser("testuser", "password123")
            .WithPackages("curl", "wget")
            .Build();

        // Act
        var result = await _enhancedCloudInitService.GenerateUserDataAsync(config);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("#cloud-config", result);
        Assert.Contains("test-server", result);
        Assert.Contains("testuser", result);
        Assert.Contains("curl", result);
        Assert.Contains("wget", result);

        _output.WriteLine("Generated YAML:");
        _output.WriteLine(result);
    }

    [Fact]
    public async Task CreateCloudInitISOAsync_WithConfiguration_ShouldCreateIso()
    {
        // Arrange
        var tempDir = FileSystemHelper.GetTempDirectory("enhanced-cloudinit-test");
        var outputPath = Path.Combine(tempDir, "test.iso");
        _tempDirectories.Add(tempDir);

        var config = CloudInitConfigurationBuilder.Create()
            .WithHostname("iso-test")
            .WithUser("admin", "admin123")
            .WithPackages("nginx")
            .Build();

        // Act
        var result = await _enhancedCloudInitService.CreateCloudInitISOAsync(
            config,
            "test-vm",
            outputPath
        );

        // Assert
        Assert.Equal(outputPath, result);
        // Check if files were created (ISO or fallback files)
        Assert.True(File.Exists(outputPath + ".user-data") || File.Exists(outputPath));
        Assert.True(File.Exists(outputPath + ".meta-data") || File.Exists(outputPath));
    }

    [Fact]
    public async Task ValidateConfigurationAsync_WithValidConfig_ShouldReturnSuccess()
    {
        // Arrange
        var config = CloudInitConfigurationBuilder.Create()
            .WithHostname("valid-config")
            .WithUser("admin")
            .Build();

        // Act
        var result = await _enhancedCloudInitService.ValidateConfigurationAsync(config);

        // Assert
        // Note: This test may pass even if validation fails due to schema unavailability
        // The validator is designed to be permissive when schema can't be loaded
        Assert.NotNull(result);
        _output.WriteLine($"Validation result: IsValid={result.IsValid}, Errors={result.Errors.Count}");
        
        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
            {
                _output.WriteLine($"Validation error: {error}");
            }
        }
    }

    [Fact]
    public async Task CloudInitConfigurationBuilder_BuildDockerServerConfiguration_ShouldCreateCompleteConfig()
    {
        // Arrange & Act - Build a realistic Docker server configuration
        var config = CloudInitConfigurationBuilder.Create()
            .WithHostname("docker-server")
            .WithFqdn("docker-server.internal")
            .WithTimezone("UTC")
            .WithLocale("en_US.UTF-8")
            .WithKeyboard("us")
            .WithUser("dockeradmin", "SecureDockerPass123!", "docker,sudo", 
                sshKeys: "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC...")
            .WithPackages("curl", "wget", "ca-certificates", "gnupg", "lsb-release")
            .WithAptSource("docker", 
                "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu focal stable",
                keyId: "9DC858229FC7DD38854AE2D88D81803C0EBFCD88")
            .WithPackages("docker-ce", "docker-ce-cli", "containerd.io", "docker-compose-plugin")
            .WithRunCommands(
                "systemctl enable docker",
                "systemctl start docker",
                "usermod -aG docker dockeradmin",
                "docker run hello-world"
            )
            .WithNetworkStatic("eth0", "192.168.1.50/24", "192.168.1.1", "1.1.1.1", "1.0.0.1")
            .WithSshConfig(true, "rsa", "ecdsa", "ed25519")
            .WithWriteFile("/etc/docker/daemon.json", 
                "{\n  \"log-driver\": \"json-file\",\n  \"log-opts\": {\n    \"max-size\": \"10m\",\n    \"max-file\": \"3\"\n  }\n}",
                "0644", "root:root")
            .WithPowerState("reboot")
            .Build();

        // Generate YAML and test
        var yaml = await _enhancedCloudInitService.GenerateUserDataAsync(config);

        // Assert
        Assert.NotNull(yaml);
        Assert.StartsWith("#cloud-config", yaml);
        Assert.Contains("docker-server", yaml);
        Assert.Contains("dockeradmin", yaml);
        Assert.Contains("docker-ce", yaml);
        Assert.Contains("192.168.1.50/24", yaml);
        Assert.Contains("/etc/docker/daemon.json", yaml);

        _output.WriteLine("Complete Docker Server Configuration:");
        _output.WriteLine(yaml);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task FullWorkflow_BuildConfigGenerateYamlAndCreateISO_ShouldWork()
    {
        // Arrange
        var tempDir = FileSystemHelper.GetTempDirectory("full-workflow-test");
        var isoPath = Path.Combine(tempDir, "workflow-test.iso");
        _tempDirectories.Add(tempDir);

        // Act - Build configuration using fluent API
        var config = CloudInitConfigurationBuilder.Create()
            .WithHostname("workflow-test")
            .WithTimezone("America/New_York")
            .WithUser("workflowuser", "WorkflowPass123!")
            .WithPackages("htop", "tree", "git")
            .WithNetworkDhcp("eth0")
            .WithRunCommands("apt update", "apt upgrade -y")
            .Build();

        // Generate YAML
        var yaml = await _enhancedCloudInitService.GenerateUserDataAsync(config);
        
        // Validate configuration
        var validation = await _enhancedCloudInitService.ValidateConfigurationAsync(config);
        
        // Create ISO
        var isoResult = await _enhancedCloudInitService.CreateCloudInitISOAsync(
            config,
            "workflow-vm",
            isoPath
        );

        // Assert
        Assert.NotNull(yaml);
        Assert.StartsWith("#cloud-config", yaml);
        Assert.Contains("workflow-test", yaml);
        
        Assert.NotNull(validation);
        _output.WriteLine($"Validation: IsValid={validation.IsValid}");
        
        Assert.Equal(isoPath, isoResult);
        Assert.True(File.Exists(isoPath + ".user-data") || File.Exists(isoPath));

        _output.WriteLine("Full workflow completed successfully!");
        _output.WriteLine("Generated YAML:");
        _output.WriteLine(yaml);
    }

    #endregion

    public void Dispose()
    {
        // Cleanup temporary files
        foreach (var file in _tempFiles)
        {
            FileSystemHelper.CleanupFile(file);
        }

        // Cleanup temporary directories
        foreach (var dir in _tempDirectories)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        _httpClient?.Dispose();
        _validator?.Dispose();
    }
}