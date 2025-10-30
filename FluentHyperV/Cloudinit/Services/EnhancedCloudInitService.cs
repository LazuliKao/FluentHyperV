using FluentHyperV.Cloudinit.Core;
using FluentHyperV.Cloudinit.Models;
using FluentHyperV.Cloudinit.Utils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using YamlDotNet.Serialization;

namespace FluentHyperV.Cloudinit.Services;

/// <summary>
/// Enhanced cloud-init service with configuration builder support
/// </summary>
public class EnhancedCloudInitService : CloudInitService, IEnhancedCloudInitService
{
    private readonly ICloudInitValidator _validator;
    private readonly ILogger<CloudInitService> _logger;

    public EnhancedCloudInitService(
        ILogger<CloudInitService> logger, 
        ProcessExecutor processExecutor,
        ICloudInitValidator validator) 
        : base(logger, processExecutor)
    {
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Generate cloud-init user data from a CloudInitConfiguration
    /// </summary>
    public async Task<string> GenerateUserDataAsync(
        CloudInitConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating cloud-init user data from configuration");

        try
        {
            await Task.Delay(1, cancellationToken); // Make method async

            var serializer = new SerializerBuilder()
                .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.UnderscoredNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();

            var yamlContent = "#cloud-config\n" + serializer.Serialize(configuration);

            _logger.LogDebug("Generated enhanced user data:\n{UserData}", yamlContent);
            return yamlContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate enhanced user data");
            throw;
        }
    }

    /// <summary>
    /// Generate and validate cloud-init user data
    /// </summary>
    public async Task<string> GenerateAndValidateUserDataAsync(
        CloudInitConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating and validating cloud-init user data");

        var yamlContent = await GenerateUserDataAsync(configuration, cancellationToken);
        
        // Validate the configuration
        var validationResult = await ValidateConfigurationAsync(configuration, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            var errorMessage = $"Configuration validation failed: {string.Join(", ", validationResult.Errors)}";
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        _logger.LogInformation("Cloud-init configuration validated successfully");
        return yamlContent;
    }

    /// <summary>
    /// Create a complete cloud-init ISO from a CloudInitConfiguration
    /// </summary>
    public async Task<string> CreateCloudInitISOAsync(
        CloudInitConfiguration configuration,
        string vmName,
        string outputPath,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating cloud-init ISO from configuration for VM: {VMName}", vmName);

        try
        {
            // Generate user data
            var userData = await GenerateUserDataAsync(configuration, cancellationToken);
            
            // Generate meta data using hostname from configuration or VM name
            var hostname = configuration.Hostname ?? vmName;
            var metaData = await GenerateMetaDataAsync(vmName, hostname, cancellationToken);

            // Create temporary files
            var tempDir = FileSystemHelper.GetTempDirectory("enhanced-cloudinit-iso");
            var userDataPath = Path.Combine(tempDir, "user-data");
            var metaDataPath = Path.Combine(tempDir, "meta-data");

            await File.WriteAllTextAsync(userDataPath, userData, cancellationToken);
            await File.WriteAllTextAsync(metaDataPath, metaData, cancellationToken);

            // Create ISO using the base service method
            var result = await CreateCloudInitISOAsync(userDataPath, metaDataPath, outputPath, cancellationToken);

            // Cleanup temp directory
            try
            {
                Directory.Delete(tempDir, true);
            }
            catch
            {
                // Ignore cleanup errors
            }

            _logger.LogInformation("Successfully created enhanced cloud-init ISO: {OutputPath}", outputPath);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create enhanced cloud-init ISO");
            throw;
        }
    }

    /// <summary>
    /// Validate a cloud-init configuration
    /// </summary>
    public async Task<ValidationResult> ValidateConfigurationAsync(
        CloudInitConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating cloud-init configuration");

        try
        {
            // Convert configuration to JSON for validation
            var json = JsonConvert.SerializeObject(configuration, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented
            });

            var result = await _validator.ValidateAsync(json, cancellationToken);
            
            if (result.IsValid)
            {
                _logger.LogInformation("Configuration validation successful");
            }
            else
            {
                _logger.LogWarning("Configuration validation failed with {ErrorCount} errors", result.Errors.Count);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during configuration validation");
            return ValidationResult.Failure($"Validation error: {ex.Message}");
        }
    }
}