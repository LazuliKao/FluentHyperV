using FluentHyperV.Cloudinit.Core;
using FluentHyperV.Cloudinit.Models;
using FluentHyperV.Cloudinit.Utils;
using Microsoft.Extensions.Logging;

namespace FluentHyperV.Cloudinit.Services;

/// <summary>
/// Cloud image service implementation
/// </summary>
public class CloudImageService : ICloudImageService
{
    private readonly ILogger<CloudImageService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ProcessExecutor _processExecutor;

    public CloudImageService(
        ILogger<CloudImageService> logger,
        HttpClient httpClient,
        ProcessExecutor processExecutor
    )
    {
        _logger = logger;
        _httpClient = httpClient;
        _processExecutor = processExecutor;
    }

    public async Task<string> DownloadImageAsync(
        ImageConfiguration imageConfig,
        string cachePath,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Downloading image version: {ImageVersion}",
            imageConfig.ImageVersion
        );

        try
        {
            // Build image URL
            var imageUrl = BuildImageUrl(imageConfig);
            var imageName = Path.GetFileName(new Uri(imageUrl).LocalPath);
            var localImagePath = Path.Combine(cachePath, imageName);

            // Check if image already exists and if update check is needed
            if (File.Exists(localImagePath))
            {
                if (imageConfig.BaseImageCheckForUpdate)
                {
                    var needsUpdate = await CheckImageUpdateAsync(
                        imageConfig,
                        localImagePath,
                        cancellationToken
                    );
                    if (!needsUpdate)
                    {
                        _logger.LogInformation(
                            "Local image is up to date: {ImagePath}",
                            localImagePath
                        );
                        return localImagePath;
                    }

                    _logger.LogInformation("Local image needs update, downloading new version");
                }
                else
                {
                    _logger.LogInformation(
                        "Using existing local image: {ImagePath}",
                        localImagePath
                    );
                    return localImagePath;
                }
            }

            // Download image
            _logger.LogInformation("Downloading image from: {ImageUrl}", imageUrl);

            FileSystemHelper.EnsureDirectoryExists(cachePath);

            using var response = await _httpClient.GetAsync(
                imageUrl,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken
            );
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? 0;
            var downloadedBytes = 0L;

            using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var fileStream = new FileStream(
                localImagePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                8192,
                true
            );

            var buffer = new byte[8192];
            var isMoreToRead = true;

            do
            {
                var read = await contentStream.ReadAsync(
                    buffer,
                    0,
                    buffer.Length,
                    cancellationToken
                );
                if (read == 0)
                {
                    isMoreToRead = false;
                }
                else
                {
                    await fileStream.WriteAsync(buffer, 0, read, cancellationToken);
                    downloadedBytes += read;

                    if (totalBytes > 0)
                    {
                        var percentage = (double)downloadedBytes / totalBytes * 100;
                        _logger.LogInformation(
                            "Download progress: {Percentage:F1}% ({Downloaded}/{Total})",
                            percentage,
                            FileSystemHelper.FormatBytes(downloadedBytes),
                            FileSystemHelper.FormatBytes(totalBytes)
                        );
                    }
                }
            } while (isMoreToRead);

            // Save timestamp for update checking
            var timestampFile = Path.Combine(cachePath, "baseimagetimestamp.txt");
            await File.WriteAllTextAsync(
                timestampFile,
                DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                cancellationToken
            );

            _logger.LogInformation("Successfully downloaded image: {ImagePath}", localImagePath);
            return localImagePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to download image: {ImageVersion}",
                imageConfig.ImageVersion
            );
            throw;
        }
    }

    public async Task<string> ConvertImageToVHDAsync(
        string imagePath,
        string outputPath,
        ulong vhdSizeBytes,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation(
            "Converting image to VHD: {ImagePath} -> {OutputPath}",
            imagePath,
            outputPath
        );

        try
        {
            FileSystemHelper.EnsureDirectoryExists(Path.GetDirectoryName(outputPath)!);

            // TODO: Use qemu-img to convert image to VHD format
            // This would typically call the qemu-img tool from the tools directory
            var qemuImgPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "tools",
                "qemu-img-4.1.0",
                "qemu-img.exe"
            );

            if (!File.Exists(qemuImgPath))
            {
                throw new FileNotFoundException($"qemu-img tool not found at: {qemuImgPath}");
            }

            var sizeInBytes = vhdSizeBytes.ToString();
            var arguments =
                $"convert -f qcow2 -O vpc -o subformat=dynamic \"{imagePath}\" \"{outputPath}\"";

            if (vhdSizeBytes > 0)
            {
                // Resize VHD to specified size
                var resizeArgs = $"resize \"{outputPath}\" {sizeInBytes}";

                var convertResult = await _processExecutor.ExecuteAsync(
                    qemuImgPath,
                    arguments,
                    cancellationToken: cancellationToken
                );
                if (!convertResult.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed to convert image: {string.Join(", ", convertResult.Errors)}"
                    );
                }

                var resizeResult = await _processExecutor.ExecuteAsync(
                    qemuImgPath,
                    resizeArgs,
                    cancellationToken: cancellationToken
                );
                if (!resizeResult.Success)
                {
                    _logger.LogWarning(
                        "Failed to resize VHD, but conversion succeeded: {Errors}",
                        string.Join(", ", resizeResult.Errors)
                    );
                }
            }
            else
            {
                var result = await _processExecutor.ExecuteAsync(
                    qemuImgPath,
                    arguments,
                    cancellationToken: cancellationToken
                );
                if (!result.Success)
                {
                    throw new InvalidOperationException(
                        $"Failed to convert image: {string.Join(", ", result.Errors)}"
                    );
                }
            }

            _logger.LogInformation("Successfully converted image to VHD: {OutputPath}", outputPath);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert image to VHD: {ImagePath}", imagePath);
            throw;
        }
    }

    public async Task<bool> ConvertVHDToNoCloudAsync(
        string vhdPath,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Converting VHD to NoCloud format: {VHDPath}", vhdPath);

        try
        {
            // TODO: Implement VHD to NoCloud conversion using WSL
            // This would call the Convert-VHDToNoCloud.ps1 script or implement the logic in C#

            await Task.Delay(100, cancellationToken); // Placeholder to make method truly async
            _logger.LogWarning(
                "VHD to NoCloud conversion not yet implemented - TODO: Call PowerShell script or implement WSL mounting logic"
            );

            // For now, return true and log that this needs implementation
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert VHD to NoCloud: {VHDPath}", vhdPath);
            return false;
        }
    }

    public async Task<bool> CheckImageUpdateAsync(
        ImageConfiguration imageConfig,
        string localImagePath,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogDebug("Checking if image needs update: {LocalImagePath}", localImagePath);

        try
        {
            var cacheDir = Path.GetDirectoryName(localImagePath)!;
            var timestampFile = Path.Combine(cacheDir, "baseimagetimestamp.txt");

            if (!File.Exists(timestampFile))
            {
                _logger.LogInformation("No timestamp file found, assuming update needed");
                return true;
            }

            var timestampText = await File.ReadAllTextAsync(timestampFile, cancellationToken);
            if (!DateTime.TryParse(timestampText, out var lastUpdate))
            {
                _logger.LogWarning("Invalid timestamp format, assuming update needed");
                return true;
            }

            // Check if image is older than 24 hours
            var updateThreshold = DateTime.UtcNow.AddHours(-24);
            var needsUpdate = lastUpdate < updateThreshold;

            _logger.LogDebug(
                "Image last updated: {LastUpdate}, needs update: {NeedsUpdate}",
                lastUpdate,
                needsUpdate
            );
            return needsUpdate;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to check image update status: {LocalImagePath}",
                localImagePath
            );
            return true; // Assume update needed on error
        }
    }

    public string BuildImageUrl(ImageConfiguration imageConfig)
    {
        // TODO: Build proper image URL based on configuration
        // This is a simplified version - the full implementation would handle different distributions and versions

        var baseUrl = imageConfig.ImageBaseUrl.TrimEnd('/');
        var version = imageConfig.ImageVersion;
        var release = imageConfig.ImageRelease;

        // Example for Ubuntu: https://mirror.nju.edu.cn/ubuntu-cloud-images/releases/22.04/release/ubuntu-22.04-server-cloudimg-amd64.img
        return $"{baseUrl}/{version}/{release}/ubuntu-{version}-server-cloudimg-amd64.img";
    }
}
