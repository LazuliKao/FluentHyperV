using FluentHyperV.Cloudinit.Models;

namespace FluentHyperV.Cloudinit.Core;

/// <summary>
/// Interface for cloud-init operations
/// </summary>
public interface ICloudInitService
{
    /// <summary>
    /// Generate cloud-init user data YAML
    /// </summary>
    Task<string> GenerateUserDataAsync(
        GuestConfiguration guestConfig,
        NetworkConfiguration networkConfig,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Generate cloud-init meta data
    /// </summary>
    Task<string> GenerateMetaDataAsync(
        string vmName,
        string hostname,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Create cloud-init ISO
    /// </summary>
    Task<string> CreateCloudInitISOAsync(
        string userDataPath,
        string metaDataPath,
        string outputPath,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Load custom user data from file
    /// </summary>
    Task<string> LoadCustomUserDataAsync(
        string filePath,
        CancellationToken cancellationToken = default
    );
}
