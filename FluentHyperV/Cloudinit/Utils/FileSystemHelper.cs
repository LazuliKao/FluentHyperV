namespace FluentHyperV.Cloudinit.Utils;

/// <summary>
/// File system utility methods
/// </summary>
public static class FileSystemHelper
{
    /// <summary>
    /// Clean up file safely without throwing exception
    /// </summary>
    public static void CleanupFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    /// <summary>
    /// Ensure directory exists
    /// </summary>
    public static void EnsureDirectoryExists(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }
    }

    /// <summary>
    /// Get cache directory path
    /// </summary>
    public static string GetCacheDirectory(string imageVersion)
    {
        var basePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "hyperv-vm-provisioning",
            "data",
            "cache",
            $"CloudImage-ubuntu-{imageVersion}"
        );
        EnsureDirectoryExists(basePath);
        return basePath;
    }

    /// <summary>
    /// Get temporary directory path
    /// </summary>
    public static string GetTempDirectory(string? suffix = null)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), "hyperv-provisioning");
        if (!string.IsNullOrEmpty(suffix))
        {
            tempPath = Path.Combine(tempPath, suffix);
        }
        EnsureDirectoryExists(tempPath);
        return tempPath;
    }

    /// <summary>
    /// Copy file with progress callback
    /// </summary>
    public static async Task CopyFileAsync(
        string sourcePath,
        string destinationPath,
        IProgress<long>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        const int bufferSize = 1024 * 1024; // 1MB buffer

        using var sourceStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read);
        using var destinationStream = new FileStream(
            destinationPath,
            FileMode.Create,
            FileAccess.Write
        );

        var buffer = new byte[bufferSize];
        var totalBytesRead = 0L;
        int bytesRead;

        while (
            (bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken))
            > 0
        )
        {
            await destinationStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);
            totalBytesRead += bytesRead;
            progress?.Report(totalBytesRead);
        }
    }

    /// <summary>
    /// Get file size in bytes
    /// </summary>
    public static long GetFileSize(string filePath)
    {
        return File.Exists(filePath) ? new FileInfo(filePath).Length : 0;
    }

    /// <summary>
    /// Format bytes to human readable string
    /// </summary>
    public static string FormatBytes(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        var counter = 0;
        decimal number = bytes;

        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }

        return string.Format("{0:n1} {1}", number, suffixes[counter]);
    }
}
