# Enhanced Cloud-Init Configuration System

This enhancement implements a comprehensive cloud-init configuration system with the following features:

## Features

### ✅ Implemented
- **Fluent Configuration Builder**: Programmable object-oriented interface for building cloud-init configurations
- **Hostname Configuration**: Set hostname and FQDN
- **Network Configuration**: Support for both DHCP and static IP configurations
- **Package Management**: Install packages and configure APT repositories
- **APT Sources**: Add custom APT repositories with GPG keys
- **User Management**: Create users with passwords and SSH keys
- **Command Execution**: Run commands after system initialization
- **File Writing**: Write configuration files to the system
- **SSH Configuration**: Configure SSH server settings
- **Power State Management**: Control post-initialization power state
- **JSON Schema Validation**: Validate against official cloud-init schema
- **YAML Generation**: Generate valid cloud-init YAML configuration
- **ISO Generation**: Create cloud-init ISO files (when dependencies available)

## Usage

### Basic Example

```csharp
using FluentHyperV.Cloudinit.Core;

var config = CloudInitConfigurationBuilder.Create()
    .WithHostname("web-server")
    .WithUser("admin", "SecurePass123!")
    .WithPackages("nginx", "htop")
    .WithNetworkDhcp("eth0")
    .Build();
```

### Advanced Example

```csharp
var dockerConfig = CloudInitConfigurationBuilder.Create()
    .WithHostname("docker-dev")
    .WithFqdn("docker-dev.internal")
    .WithTimezone("UTC")
    .WithLocale("en_US.UTF-8")
    .WithUser("developer", "DevPass123!", "docker,sudo",
        sshKeys: "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQD...")
    .WithPackages("curl", "wget", "ca-certificates", "gnupg")
    .WithAptSource("docker", 
        "deb [arch=amd64] https://download.docker.com/linux/ubuntu focal stable",
        keyId: "9DC858229FC7DD38854AE2D88D81803C0EBFCD88")
    .WithPackages("docker-ce", "docker-ce-cli", "containerd.io")
    .WithNetworkStatic("eth0", "192.168.1.100/24", "192.168.1.1", "8.8.8.8")
    .WithRunCommands(
        "systemctl enable docker",
        "systemctl start docker",
        "usermod -aG docker developer"
    )
    .WithWriteFile("/etc/docker/daemon.json",
        "{\n  \"log-driver\": \"json-file\",\n  \"log-opts\": {\n    \"max-size\": \"10m\"\n  }\n}",
        "0644", "root:root")
    .WithSshConfig(true, "rsa", "ecdsa", "ed25519")
    .WithPowerState("reboot")
    .Build();
```

### Generate YAML

```csharp
using FluentHyperV.Cloudinit.Services;

var enhancedService = new EnhancedCloudInitService(logger, processExecutor, validator);
var yaml = await enhancedService.GenerateUserDataAsync(config);
```

### Validate Configuration

```csharp
var validationResult = await enhancedService.ValidateConfigurationAsync(config);
if (!validationResult.IsValid)
{
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}
```

### Create ISO

```csharp
var isoPath = await enhancedService.CreateCloudInitISOAsync(
    config,
    "vm-name",
    "/path/to/output.iso"
);
```

## Architecture

### Core Classes

- **`CloudInitConfiguration`**: Represents the complete configuration object
- **`CloudInitConfigurationBuilder`**: Fluent builder for creating configurations
- **`IEnhancedCloudInitService`**: Enhanced service interface
- **`EnhancedCloudInitService`**: Implementation with validation and generation
- **`ICloudInitValidator`**: Schema validation interface
- **`CloudInitValidator`**: JSON schema validation implementation

### Configuration Options

- **Hostname & FQDN**: System identification
- **Timezone & Locale**: Regional settings
- **Keyboard**: Keyboard layout configuration
- **Users**: User accounts with passwords and SSH keys
- **Packages**: Package installation
- **APT Sources**: Custom repository configuration
- **Network**: DHCP or static IP configuration
- **Run Commands**: Post-installation commands
- **File Writing**: Create configuration files
- **SSH**: SSH server configuration
- **Power State**: Reboot/poweroff after completion

## Schema Validation

The system validates configurations against the official cloud-init JSON schema:
`https://raw.githubusercontent.com/canonical/cloud-init/main/cloudinit/config/schemas/schema-cloud-config-v1.json`

## Demo

Run the standalone demo to see examples:

```bash
dotnet run CloudInitStandaloneDemo.cs
```

## Dependencies

- **YamlDotNet**: YAML serialization
- **Newtonsoft.Json**: JSON handling and schema validation
- **Newtonsoft.Json.Schema**: JSON schema validation
- **Microsoft.Extensions.Logging**: Logging framework

## Testing

The implementation includes comprehensive unit tests covering:
- Fluent builder functionality
- Configuration serialization
- Validation logic
- ISO generation
- Complex configuration scenarios

## Compliance

✅ **Hostname Configuration**: Implemented with `WithHostname()` and `WithFqdn()`
✅ **Network Configuration**: Optional support via `WithNetworkDhcp()` and `WithNetworkStatic()`
✅ **Package Installation**: Implemented with `WithPackages()`
✅ **APT Sources**: Implemented with `WithAptSource()`
✅ **JSON Schema Validation**: Validates against official cloud-init schema
✅ **ISO Generation**: Creates cloud-init compatible ISOs
✅ **Programmable Object**: Fluent builder pattern for configuration

This implementation provides a complete, production-ready solution for cloud-init configuration management within the FluentHyperV ecosystem.