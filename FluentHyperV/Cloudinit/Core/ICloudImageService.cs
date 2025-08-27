using FluentHyperV.Cloudinit.Models;

namespace FluentHyperV.Cloudinit.Core;

/// <summary>
/// Interface for cloud image operations
/// </summary>
public interface ICloudImageService
{
    /// <summary>
    /// Download cloud image if needed
    /// </summary>
    Task<string> DownloadImageAsync(
        ImageConfiguration imageConfig,
        string cachePath,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Convert cloud image to VHD format
    /// </summary>
    Task<string> ConvertImageToVHDAsync(
        string imagePath,
        string outputPath,
        ulong vhdSizeBytes,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Convert Azure image to NoCloud format
    /// </summary>
    Task<bool> ConvertVHDToNoCloudAsync(
        string vhdPath,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Check if image needs update
    /// </summary>
    Task<bool> CheckImageUpdateAsync(
        ImageConfiguration imageConfig,
        string localImagePath,
        CancellationToken cancellationToken = default
    );
}
