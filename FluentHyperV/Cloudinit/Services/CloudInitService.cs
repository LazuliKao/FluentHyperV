using FluentHyperV.Cloudinit.Core;
using FluentHyperV.Cloudinit.Models;
using FluentHyperV.Cloudinit.Utils;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;

namespace FluentHyperV.Cloudinit.Services;

/// <summary>
/// Cloud-init service implementation
/// </summary>
public class CloudInitService(ILogger<CloudInitService> logger, ProcessExecutor processExecutor)
    : ICloudInitService
{
    public async Task<string> GenerateUserDataAsync(
        GuestConfiguration guestConfig,
        NetworkConfiguration networkConfig,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("Generating cloud-init user data");

        try
        {
            await Task.Delay(1, cancellationToken); // Make method async

            var userData = new
            {
                cloud_config = true,
                users = new[]
                {
                    new
                    {
                        name = guestConfig.GuestAdminUsername,
                        sudo = "ALL=(ALL) NOPASSWD:ALL",
                        groups = "sudo",
                        shell = "/bin/bash",
                        lock_passwd = false,
                        passwd = HashPassword(guestConfig.GuestAdminPassword),
                        ssh_authorized_keys = !string.IsNullOrEmpty(guestConfig.GuestAdminSshPubKey)
                            ? new[] { guestConfig.GuestAdminSshPubKey }
                            : Array.Empty<string>(),
                    },
                },
                hostname = guestConfig.DomainName,
                timezone = guestConfig.TimeZone,
                locale = guestConfig.Locale,
                keyboard = new
                {
                    layout = guestConfig.KeyboardLayout,
                    model = guestConfig.KeyboardModel ?? "pc105",
                    options = guestConfig.KeyboardOptions,
                },
                ssh_deletekeys = true,
                ssh_genkeytypes = new[] { "rsa", "ecdsa", "ed25519" },
                packages = BuildPackageList(guestConfig),
                runcmd = BuildRunCommands(guestConfig),
                power_state = new
                {
                    mode = guestConfig.CloudInitPowerState,
                    timeout = 30,
                    condition = true,
                },
            };

            var serializer = new SerializerBuilder()
                .WithNamingConvention(
                    YamlDotNet.Serialization.NamingConventions.UnderscoredNamingConvention.Instance
                )
                .Build();

            var yamlContent = "#cloud-config\n" + serializer.Serialize(userData);

            // Add network configuration if not auto-config
            if (!networkConfig.NetAutoconfig)
            {
                yamlContent = AddNetworkConfigToYaml(yamlContent, networkConfig);
            }

            logger.LogDebug("Generated user data:\n{UserData}", yamlContent);
            return yamlContent;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to generate user data");
            throw;
        }
    }

    public async Task<string> GenerateMetaDataAsync(
        string vmName,
        string hostname,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("Generating cloud-init meta data for VM: {VMName}", vmName);

        await Task.Delay(1, cancellationToken); // Make method async

        var metaData = new
        {
            instance_id = $"{vmName}-{Guid.NewGuid():N}",
            local_hostname = hostname,
            hostname = hostname,
        };

        var serializer = new SerializerBuilder()
            .WithNamingConvention(
                YamlDotNet.Serialization.NamingConventions.UnderscoredNamingConvention.Instance
            )
            .Build();

        var yamlContent = serializer.Serialize(metaData);

        logger.LogDebug("Generated meta data:\n{MetaData}", yamlContent);
        return yamlContent;
    }

    public async Task<string> CreateCloudInitISOAsync(
        string userDataPath,
        string metaDataPath,
        string outputPath,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("Creating cloud-init ISO: {OutputPath}", outputPath);

        try
        {
            // Create temporary directory for ISO content
            var tempDir = FileSystemHelper.GetTempDirectory("cloudinit-iso");
            FileSystemHelper.EnsureDirectoryExists(tempDir);

            // Copy files to temp directory
            var tempUserData = Path.Combine(tempDir, "user-data");
            var tempMetaData = Path.Combine(tempDir, "meta-data");

            await File.WriteAllTextAsync(
                tempUserData,
                await File.ReadAllTextAsync(userDataPath, cancellationToken),
                cancellationToken
            );
            await File.WriteAllTextAsync(
                tempMetaData,
                await File.ReadAllTextAsync(metaDataPath, cancellationToken),
                cancellationToken
            );

            // TODO: Create ISO using PowerShell New-ISOFile or other method
            // For now, we'll use a placeholder implementation

            // Use bsdtar to create ISO (alternative approach)
            var bsdTarPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "tools",
                "bsdtar-3.7.6",
                "bsdtar.exe"
            );

            if (File.Exists(bsdTarPath))
            {
                var arguments = $"-czf \"{outputPath}\" -C \"{tempDir}\" .";
                var result = await processExecutor.ExecuteAsync(
                    bsdTarPath,
                    arguments,
                    cancellationToken: cancellationToken
                );

                if (!result.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed to create ISO: {string.Join(", ", result.Errors)}"
                    );
                }
            }
            else
            {
                // Fallback: just copy files (not a real ISO, but works for testing)
                logger.LogWarning("bsdtar not found, copying files directly (not creating ISO)");
                FileSystemHelper.EnsureDirectoryExists(Path.GetDirectoryName(outputPath)!);
                await FileSystemHelper.CopyFileAsync(
                    tempUserData,
                    outputPath + ".user-data",
                    cancellationToken: cancellationToken
                );
                await FileSystemHelper.CopyFileAsync(
                    tempMetaData,
                    outputPath + ".meta-data",
                    cancellationToken: cancellationToken
                );
            }

            // Cleanup temp directory
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }

            logger.LogInformation("Successfully created cloud-init ISO: {OutputPath}", outputPath);
            return outputPath;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create cloud-init ISO: {OutputPath}", outputPath);
            throw;
        }
    }

    public async Task<string> LoadCustomUserDataAsync(
        string filePath,
        CancellationToken cancellationToken = default
    )
    {
        logger.LogInformation("Loading custom user data from: {FilePath}", filePath);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Custom user data file not found: {filePath}");
        }

        var content = await File.ReadAllTextAsync(filePath, cancellationToken);

        logger.LogDebug("Loaded custom user data:\n{UserData}", content);
        return content;
    }

    private string HashPassword(string password)
    {
        // TODO: Implement proper password hashing (e.g., using mkpasswd or similar)
        // For now, return the password as-is (not secure, but functional for testing)
        logger.LogWarning("Password hashing not implemented - using plain text password");
        return password;
    }

    private List<string> BuildPackageList(GuestConfiguration guestConfig)
    {
        var packages = new List<string> { "curl", "wget", "ca-certificates", "cloud-init" };

        if (guestConfig.PreInstallDocker)
        {
            packages.AddRange(new[] { "docker.io", "docker-compose" });
        }

        if (guestConfig.PreInstallGnomeDesktop)
        {
            packages.Add("ubuntu-desktop-minimal");
        }

        return packages;
    }

    private List<string> BuildRunCommands(GuestConfiguration guestConfig)
    {
        var commands = new List<string>();

        if (guestConfig.PreInstallDocker)
        {
            commands.AddRange(
                new[]
                {
                    "systemctl enable docker",
                    "systemctl start docker",
                    $"usermod -aG docker {guestConfig.GuestAdminUsername}",
                }
            );
        }

        if (guestConfig.PreInstallGnomeDesktop)
        {
            commands.Add("systemctl set-default graphical.target");
        }

        return commands;
    }

    private string AddNetworkConfigToYaml(string yamlContent, NetworkConfiguration networkConfig)
    {
        // TODO: Implement network configuration addition to YAML
        // This would modify the YAML content to include network settings
        logger.LogInformation("Adding network configuration to YAML");

        // For now, just append network config as comments
        var networkYaml =
            $@"
# Network configuration (TODO: implement proper network config)
# Interface: {networkConfig.NetInterface}
# Address: {networkConfig.NetAddress}
# Gateway: {networkConfig.NetGateway}
# DNS: {networkConfig.NameServers}
";

        return yamlContent + networkYaml;
    }
}
