using FluentHyperV.Cloudinit.Models;

namespace FluentHyperV.Cloudinit.Core;

/// <summary>
/// Enhanced interface for cloud-init operations with configuration builder support
/// </summary>
public interface IEnhancedCloudInitService : ICloudInitService
{
    /// <summary>
    /// Generate cloud-init user data from a CloudInitConfiguration
    /// </summary>
    Task<string> GenerateUserDataAsync(
        CloudInitConfiguration configuration,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Generate and validate cloud-init user data
    /// </summary>
    Task<string> GenerateAndValidateUserDataAsync(
        CloudInitConfiguration configuration,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Create a complete cloud-init ISO from a CloudInitConfiguration
    /// </summary>
    Task<string> CreateCloudInitISOAsync(
        CloudInitConfiguration configuration,
        string vmName,
        string outputPath,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Validate a cloud-init configuration
    /// </summary>
    Task<ValidationResult> ValidateConfigurationAsync(
        CloudInitConfiguration configuration,
        CancellationToken cancellationToken = default
    );
}