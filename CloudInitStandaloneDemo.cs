// Demo console program to showcase cloud-init configuration functionality
// This is a standalone program that can be run independently to test the implementation

using FluentHyperV.Cloudinit.Core;
using FluentHyperV.Cloudinit.Models;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

Console.WriteLine("=== FluentHyperV Enhanced Cloud-Init Configuration Demo ===\n");

// Demo 1: Basic web server configuration using fluent API
Console.WriteLine("Demo 1: Creating a web server configuration using fluent API\n");

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

// Generate YAML
var serializer = new SerializerBuilder()
    .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.UnderscoredNamingConvention.Instance)
    .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
    .Build();

var webServerYaml = "#cloud-config\n" + serializer.Serialize(webServerConfig);
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

var dockerYaml = "#cloud-config\n" + serializer.Serialize(dockerConfig);
Console.WriteLine("Generated Docker Development Server Cloud-Init Configuration:");
Console.WriteLine(dockerYaml);
Console.WriteLine("\n" + new string('=', 80) + "\n");

// Demo 3: Show JSON serialization for validation
Console.WriteLine("Demo 3: JSON serialization for schema validation\n");

var json = JsonConvert.SerializeObject(dockerConfig, new JsonSerializerSettings
{
    NullValueHandling = NullValueHandling.Ignore,
    Formatting = Formatting.Indented
});

Console.WriteLine("Configuration as JSON (for schema validation):");
Console.WriteLine(json);
Console.WriteLine("\n" + new string('=', 80) + "\n");

Console.WriteLine("=== Demo Complete ===");
Console.WriteLine("The enhanced cloud-init configuration system provides:");
Console.WriteLine("✓ Fluent API for building configurations");
Console.WriteLine("✓ Support for hostname, network, packages, apt sources");
Console.WriteLine("✓ JSON schema validation capability (when service is available)");
Console.WriteLine("✓ YAML generation for cloud-init consumption");
Console.WriteLine("✓ ISO generation capability (when full service is available)");
Console.WriteLine("✓ Comprehensive configuration options");
Console.WriteLine("✓ Programmable object-oriented interface");