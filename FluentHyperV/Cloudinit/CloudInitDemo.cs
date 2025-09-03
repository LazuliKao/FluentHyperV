using FluentHyperV.Cloudinit.Core;
using FluentHyperV.Cloudinit.Models;
using FluentHyperV.Cloudinit.Services;
using FluentHyperV.Cloudinit.Utils;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace FluentHyperV.Cloudinit;

/// <summary>
/// Standalone demo program showing the enhanced cloud-init configuration capabilities
/// </summary>
public class CloudInitDemo
{
    public static async Task<string> RunDemo()
    {
        Console.WriteLine("=== FluentHyperV Enhanced Cloud-Init Configuration Demo ===\n");

        // Create a logger for demo purposes
        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<CloudInitService>();
        var validatorLogger = loggerFactory.CreateLogger<CloudInitValidator>();
        
        // Create services
        var processExecutor = new ProcessExecutor(loggerFactory.CreateLogger<ProcessExecutor>());
        using var httpClient = new HttpClient();
        using var validator = new CloudInitValidator(validatorLogger, httpClient);
        
        var enhancedService = new EnhancedCloudInitService(logger, processExecutor, validator);

        // Demo 1: Basic web server configuration
        Console.WriteLine("Demo 1: Creating a basic web server configuration\n");
        
        var webServerConfig = CloudInitConfigurationBuilder.Create()
            .WithHostname("web-server")
            .WithFqdn("web-server.example.com")
            .WithTimezone("America/New_York")
            .WithLocale("en_US.UTF-8")
            .WithKeyboard("us")
            .WithUser("webadmin", "SecureWebPass123!", "sudo,www-data", 
                sshKeys: "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC7... webadmin@workstation")
            .WithPackages("nginx", "htop", "curl", "wget", "certbot", "python3-certbot-nginx")
            .WithNetworkStatic("eth0", "192.168.1.100/24", "192.168.1.1", "8.8.8.8", "8.8.4.4")
            .WithRunCommands(
                "systemctl enable nginx",
                "systemctl start nginx",
                "ufw allow 'Nginx Full'",
                "ufw --force enable"
            )
            .WithWriteFile("/var/www/html/index.html", 
                "<h1>Welcome to the Web Server!</h1><p>Configured with FluentHyperV Cloud-Init</p>",
                "0644", "www-data:www-data")
            .WithSshConfig(true, "rsa", "ecdsa", "ed25519")
            .WithPowerState("reboot")
            .Build();

        var webServerYaml = await enhancedService.GenerateUserDataAsync(webServerConfig);
        Console.WriteLine("Generated Web Server Cloud-Init Configuration:");
        Console.WriteLine(webServerYaml);
        Console.WriteLine("\n" + new string('=', 80) + "\n");

        // Demo 2: Docker development server configuration
        Console.WriteLine("Demo 2: Creating a Docker development server configuration\n");
        
        var dockerConfig = CloudInitConfigurationBuilder.Create()
            .WithHostname("docker-dev")
            .WithFqdn("docker-dev.internal")
            .WithTimezone("UTC")
            .WithLocale("en_US.UTF-8")
            .WithUser("developer", "DevPass123!", "docker,sudo",
                sshKeys: "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQD... developer@laptop")
            .WithPackages("curl", "wget", "ca-certificates", "gnupg", "lsb-release", "git", "vim")
            .WithAptSource("docker", 
                "deb [arch=amd64 signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu focal stable",
                keyId: "9DC858229FC7DD38854AE2D88D81803C0EBFCD88",
                keyServer: "keyserver.ubuntu.com")
            .WithPackages("docker-ce", "docker-ce-cli", "containerd.io", "docker-compose-plugin")
            .WithNetworkDhcp("eth0")
            .WithRunCommands(
                "curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg",
                "systemctl enable docker",
                "systemctl start docker",
                "usermod -aG docker developer",
                "docker run --rm hello-world"
            )
            .WithWriteFile("/etc/docker/daemon.json",
                "{\n  \"log-driver\": \"json-file\",\n  \"log-opts\": {\n    \"max-size\": \"10m\",\n    \"max-file\": \"3\"\n  },\n  \"storage-driver\": \"overlay2\"\n}",
                "0644", "root:root")
            .WithWriteFile("/home/developer/docker-compose.yml",
                "version: '3.8'\nservices:\n  app:\n    image: nginx:alpine\n    ports:\n      - \"8080:80\"\n    restart: unless-stopped",
                "0644", "developer:developer")
            .Build();

        var dockerYaml = await enhancedService.GenerateUserDataAsync(dockerConfig);
        Console.WriteLine("Generated Docker Development Server Cloud-Init Configuration:");
        Console.WriteLine(dockerYaml);
        Console.WriteLine("\n" + new string('=', 80) + "\n");

        // Demo 3: Validate configuration (showing validation capability)
        Console.WriteLine("Demo 3: Validating configuration against cloud-init schema\n");
        
        try
        {
            var validationResult = await enhancedService.ValidateConfigurationAsync(dockerConfig);
            Console.WriteLine($"Validation Result: IsValid = {validationResult.IsValid}");
            
            if (!validationResult.IsValid)
            {
                Console.WriteLine("Validation Errors:");
                foreach (var error in validationResult.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }
            else
            {
                Console.WriteLine("✓ Configuration is valid according to cloud-init schema");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Validation failed: {ex.Message}");
            Console.WriteLine("Note: This might be due to network connectivity or schema availability");
        }

        Console.WriteLine("\n" + new string('=', 80) + "\n");

        // Demo 4: Show the fluent builder pattern capabilities
        Console.WriteLine("Demo 4: Complex configuration with all features\n");

        var complexConfig = CloudInitConfigurationBuilder.Create()
            .WithHostname("production-server")
            .WithFqdn("prod.company.com")
            .WithTimezone("Europe/London")
            .WithLocale("en_GB.UTF-8")
            .WithKeyboard("gb")
            .WithUser("admin", "AdminPass123!", "sudo", sshKeys: 
                "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC... admin@management")
            .WithUser("service", null, "service", sshKeys:
                "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQD... service@automation")
            .WithPackages("htop", "iotop", "netstat-nat", "tcpdump", "rsyslog", "logrotate")
            .WithAptSource("elastic", 
                "deb https://artifacts.elastic.co/packages/8.x/apt stable main",
                key: "https://artifacts.elastic.co/GPG-KEY-elasticsearch")
            .WithPackages("elasticsearch")
            .WithNetworkStatic("eth0", "10.0.1.50/24", "10.0.1.1", "1.1.1.1", "1.0.0.1")
            .WithRunCommands(
                "systemctl enable elasticsearch",
                "systemctl start elasticsearch",
                "systemctl enable rsyslog",
                "systemctl start rsyslog"
            )
            .WithWriteFile("/etc/elasticsearch/elasticsearch.yml",
                "cluster.name: production\nnode.name: prod-node-1\nnetwork.host: 0.0.0.0\ndiscovery.type: single-node",
                "0644", "elasticsearch:elasticsearch")
            .WithWriteFile("/etc/logrotate.d/application",
                "/var/log/application/*.log {\n  daily\n  rotate 30\n  compress\n  delaycompress\n  missingok\n  notifempty\n}",
                "0644", "root:root")
            .WithSshConfig(true, "rsa", "ecdsa", "ed25519")
            .WithPowerState("reboot", 60, true)
            .Build();

        var complexYaml = await enhancedService.GenerateUserDataAsync(complexConfig);
        Console.WriteLine("Generated Complex Production Server Configuration:");
        Console.WriteLine(complexYaml);

        Console.WriteLine("\n=== Demo Complete ===");
        Console.WriteLine("The enhanced cloud-init configuration system provides:");
        Console.WriteLine("✓ Fluent API for building configurations");
        Console.WriteLine("✓ Support for hostname, network, packages, apt sources");
        Console.WriteLine("✓ JSON schema validation against official cloud-init schema");
        Console.WriteLine("✓ YAML generation for cloud-init consumption");
        Console.WriteLine("✓ ISO generation capability");
        Console.WriteLine("✓ Comprehensive configuration options");

        return complexYaml;
    }
}