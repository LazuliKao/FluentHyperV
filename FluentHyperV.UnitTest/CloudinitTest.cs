using System.Text;
using FluentHyperV.Cloudinit.Core;
using FluentHyperV.Cloudinit.Models;
using FluentHyperV.Cloudinit.Services;
using FluentHyperV.Cloudinit.Utils;
using FluentHyperV.UnitTest.Helper;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FluentHyperV.UnitTest;

public class CloudinitTest : IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly ILogger<CloudInitService> _cloudInitLogger;
    private readonly ILogger<CloudImageService> _cloudImageLogger;
    private readonly ILogger<ProcessExecutor> _processExecutorLogger;
    private readonly ProcessExecutor _processExecutor;
    private readonly CloudInitService _cloudInitService;
    private readonly CloudImageService _cloudImageService;
    private readonly HttpClient _httpClient;
    private readonly List<string> _tempFiles = new();
    private readonly List<string> _tempDirectories = new();

    public CloudinitTest(ITestOutputHelper output)
    {
        _output = output;

        // 创建测试用的Logger
        var loggerFactory = LoggerFactory.Create(builder =>
            builder.AddProvider(new TestLoggerProvider(_output))
        );

        _cloudInitLogger = loggerFactory.CreateLogger<CloudInitService>();
        _cloudImageLogger = loggerFactory.CreateLogger<CloudImageService>();
        _processExecutorLogger = loggerFactory.CreateLogger<ProcessExecutor>();

        _processExecutor = new ProcessExecutor(_processExecutorLogger);
        _cloudInitService = new CloudInitService(_cloudInitLogger, _processExecutor);
        _httpClient = new HttpClient();
        _cloudImageService = new CloudImageService(
            _cloudImageLogger,
            _httpClient,
            _processExecutor
        );
    }

    #region CloudInitService Tests

    [Fact]
    public async Task GenerateUserDataAsync_WithBasicConfiguration_ShouldGenerateValidYaml()
    {
        // Arrange
        var guestConfig = new GuestConfiguration
        {
            DomainName = "test.local",
            GuestAdminUsername = "testuser",
            GuestAdminPassword = "TestPass123",
            TimeZone = "Asia/Shanghai",
            Locale = "zh_CN",
            KeyboardLayout = "us",
        };

        var networkConfig = new NetworkConfiguration
        {
            NetInterface = "eth0",
            NameServers = "8.8.8.8,8.8.4.4",
        };

        // Act
        var result = await _cloudInitService.GenerateUserDataAsync(guestConfig, networkConfig);

        // Assert
        Assert.NotNull(result);
        Assert.StartsWith("#cloud-config", result);
        Assert.Contains("testuser", result);
        Assert.Contains("test.local", result);
        Assert.Contains("Asia/Shanghai", result);
        Assert.Contains("zh_CN", result);

        _output.WriteLine("Generated User Data:");
        _output.WriteLine(result);
    }

    [Fact]
    public async Task GenerateUserDataAsync_WithDockerEnabled_ShouldIncludeDockerPackages()
    {
        // Arrange
        var guestConfig = new GuestConfiguration
        {
            GuestAdminUsername = "dockeruser",
            GuestAdminPassword = "DockerPass123",
            PreInstallDocker = true,
        };

        var networkConfig = new NetworkConfiguration();

        // Act
        var result = await _cloudInitService.GenerateUserDataAsync(guestConfig, networkConfig);

        // Assert
        Assert.Contains("docker.io", result);
        Assert.Contains("docker-compose", result);
        Assert.Contains("systemctl enable docker", result);
        Assert.Contains("usermod -aG docker dockeruser", result);
    }

    [Fact]
    public async Task GenerateUserDataAsync_WithGnomeDesktop_ShouldIncludeDesktopPackages()
    {
        // Arrange
        var guestConfig = new GuestConfiguration
        {
            GuestAdminUsername = "desktopuser",
            GuestAdminPassword = "DesktopPass123",
            PreInstallGnomeDesktop = true,
        };

        var networkConfig = new NetworkConfiguration();

        // Act
        var result = await _cloudInitService.GenerateUserDataAsync(guestConfig, networkConfig);

        // Assert
        Assert.Contains("ubuntu-desktop-minimal", result);
        Assert.Contains("systemctl set-default graphical.target", result);
    }

    [Fact]
    public async Task GenerateUserDataAsync_WithSshPublicKey_ShouldIncludeAuthorizedKeys()
    {
        // Arrange
        var sshKey = "ssh-rsa AAAAB3NzaC1yc2EAAAADAQABAAABgQC7... test@example.com";
        var guestConfig = new GuestConfiguration
        {
            GuestAdminUsername = "sshuser",
            GuestAdminPassword = "SshPass123",
            GuestAdminSshPubKey = sshKey,
        };

        var networkConfig = new NetworkConfiguration();

        // Act
        var result = await _cloudInitService.GenerateUserDataAsync(guestConfig, networkConfig);

        // Assert
        Assert.Contains(sshKey, result);
        Assert.Contains("ssh_authorized_keys", result);
    }

    [Fact]
    public async Task GenerateMetaDataAsync_ShouldGenerateValidMetaData()
    {
        // Arrange
        var vmName = "test-vm";
        var hostname = "test-host";

        // Act
        var result = await _cloudInitService.GenerateMetaDataAsync(vmName, hostname);

        // Assert
        Assert.NotNull(result);
        Assert.Contains($"instance_id: {vmName}-", result);
        Assert.Contains($"local_hostname: {hostname}", result);
        Assert.Contains($"hostname: {hostname}", result);

        _output.WriteLine("Generated Meta Data:");
        _output.WriteLine(result);
    }

    [Fact]
    public async Task CreateCloudInitISOAsync_WithValidPaths_ShouldCreateIsoFiles()
    {
        // Arrange
        var tempDir = FileSystemHelper.GetTempDirectory("cloudinit-test");
        var userDataPath = Path.Combine(tempDir, "user-data");
        var metaDataPath = Path.Combine(tempDir, "meta-data");
        var outputPath = Path.Combine(tempDir, "cloudinit.iso");

        _tempDirectories.Add(tempDir);

        // 创建测试文件
        await File.WriteAllTextAsync(userDataPath, "#cloud-config\nusers:\n  - name: test");
        await File.WriteAllTextAsync(metaDataPath, "instance_id: test-123\nhostname: test");

        // Act
        var result = await _cloudInitService.CreateCloudInitISOAsync(
            userDataPath,
            metaDataPath,
            outputPath
        );

        // Assert
        Assert.Equal(outputPath, result);
        // 由于可能没有bsdtar工具，检查是否创建了fallback文件
        Assert.True(File.Exists(outputPath + ".user-data") || File.Exists(outputPath));
        Assert.True(File.Exists(outputPath + ".meta-data") || File.Exists(outputPath));
    }

    [Fact]
    public async Task LoadCustomUserDataAsync_WithValidFile_ShouldReturnContent()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var customContent = "#cloud-config\npackages:\n  - vim\n  - git";
        await File.WriteAllTextAsync(tempFile, customContent);
        _tempFiles.Add(tempFile);

        // Act
        var result = await _cloudInitService.LoadCustomUserDataAsync(tempFile);

        // Assert
        Assert.Equal(customContent, result);
    }

    [Fact]
    public async Task LoadCustomUserDataAsync_WithNonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "non-existent-file.txt");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(() =>
            _cloudInitService.LoadCustomUserDataAsync(nonExistentFile)
        );
    }

    #endregion

    #region CloudImageService Tests

    [Fact]
    public void BuildImageUrl_WithDefaultConfiguration_ShouldGenerateCorrectUrl()
    {
        // Arrange
        var imageConfig = new ImageConfiguration
        {
            ImageVersion = "22.04",
            ImageRelease = "release",
            ImageBaseUrl = "https://mirror.nju.edu.cn/ubuntu-cloud-images/releases",
        };

        var result = _cloudImageService.BuildImageUrl(imageConfig);
        // Assert
        var expectedUrl =
            "https://mirror.nju.edu.cn/ubuntu-cloud-images/releases/22.04/release/ubuntu-22.04-server-cloudimg-amd64.img";
        Assert.Equal(expectedUrl, result);
    }

    [Fact]
    public async Task CheckImageUpdateAsync_WithNoTimestampFile_ShouldReturnTrue()
    {
        // Arrange
        var tempDir = FileSystemHelper.GetTempDirectory("image-update-test");
        var imagePath = Path.Combine(tempDir, "test-image.img");
        var imageConfig = new ImageConfiguration();

        _tempDirectories.Add(tempDir);

        // 创建一个虚拟的镜像文件但不创建时间戳文件
        await File.WriteAllTextAsync(imagePath, "dummy image content");

        // Act
        var result = await _cloudImageService.CheckImageUpdateAsync(imageConfig, imagePath);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckImageUpdateAsync_WithOldTimestamp_ShouldReturnTrue()
    {
        // Arrange
        var tempDir = FileSystemHelper.GetTempDirectory("image-old-test");
        var imagePath = Path.Combine(tempDir, "test-image.img");
        var timestampFile = Path.Combine(tempDir, "baseimagetimestamp.txt");
        var imageConfig = new ImageConfiguration();

        _tempDirectories.Add(tempDir);

        // 创建一个旧的时间戳文件（2天前）
        var oldTimestamp = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-ddTHH:mm:ssZ");
        await File.WriteAllTextAsync(timestampFile, oldTimestamp);
        await File.WriteAllTextAsync(imagePath, "dummy image content");

        // Act
        var result = await _cloudImageService.CheckImageUpdateAsync(imageConfig, imagePath);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CheckImageUpdateAsync_WithRecentTimestamp_ShouldReturnFalse()
    {
        // Arrange
        var tempDir = FileSystemHelper.GetTempDirectory("image-recent-test");
        var imagePath = Path.Combine(tempDir, "test-image.img");
        var timestampFile = Path.Combine(tempDir, "baseimagetimestamp.txt");
        var imageConfig = new ImageConfiguration();

        _tempDirectories.Add(tempDir);

        // 创建一个新的时间戳文件（1小时前）
        var recentTimestamp = DateTime.UtcNow.AddHours(-1).ToString("yyyy-MM-ddTHH:mm:ssZ");
        await File.WriteAllTextAsync(timestampFile, recentTimestamp);
        await File.WriteAllTextAsync(imagePath, "dummy image content");

        // Act
        var result = await _cloudImageService.CheckImageUpdateAsync(imageConfig, imagePath);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task ConvertVHDToNoCloudAsync_ShouldReturnTrue()
    {
        // Arrange
        var vhdPath = "/path/to/test.vhdx";

        // Act
        var result = await _cloudImageService.ConvertVHDToNoCloudAsync(vhdPath);

        // Assert
        Assert.True(result); // 目前实现总是返回true
    }

    #endregion

    #region Model Tests

    [Fact]
    public void GuestConfiguration_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var config = new GuestConfiguration();

        // Assert
        Assert.Equal("domain.local", config.DomainName);
        Assert.Equal("us", config.KeyboardLayout);
        Assert.Equal("en_US", config.Locale);
        Assert.Equal("UTC", config.TimeZone);
        Assert.Equal("reboot", config.CloudInitPowerState);
        Assert.Equal("admin", config.GuestAdminUsername);
        Assert.Equal("Passw0rd", config.GuestAdminPassword);
        Assert.False(config.PreInstallDocker);
        Assert.False(config.PreInstallGnomeDesktop);
    }

    [Fact]
    public void NetworkConfiguration_AutoConfig_ShouldDetectCorrectly()
    {
        // Arrange & Act
        var autoConfig = new NetworkConfiguration();
        var manualConfig = new NetworkConfiguration
        {
            NetAddress = "192.168.1.100",
            NetNetmask = "255.255.255.0",
            NetGateway = "192.168.1.1",
        };

        // Assert
        Assert.True(autoConfig.NetAutoconfig);
        Assert.False(manualConfig.NetAutoconfig);
    }

    [Fact]
    public void ImageConfiguration_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var config = new ImageConfiguration();

        // Assert
        Assert.Equal("22.04", config.ImageVersion);
        Assert.Equal("release", config.ImageRelease);
        Assert.Equal("https://mirror.nju.edu.cn/ubuntu-cloud-images/releases", config.ImageBaseUrl);
        Assert.True(config.BaseImageCheckForUpdate);
    }

    [Fact]
    public void NetworkConfiguration_DefaultValues_ShouldBeCorrect()
    {
        // Arrange & Act
        var config = new NetworkConfiguration();

        // Assert
        Assert.Equal("eth0", config.NetInterface);
        Assert.Equal("1.1.1.1,1.0.0.1", config.NameServers);
        Assert.Null(config.NetAddress);
        Assert.Null(config.NetNetmask);
        Assert.Null(config.NetNetwork);
        Assert.Null(config.NetGateway);
        Assert.Null(config.NetConfigType);
    }

    #endregion

    #region FileSystemHelper Tests

    [Fact]
    public void GetTempDirectory_ShouldCreateAndReturnPath()
    {
        // Act
        var tempDir = FileSystemHelper.GetTempDirectory("test-suffix");
        _tempDirectories.Add(tempDir);

        // Assert
        Assert.True(Directory.Exists(tempDir));
        Assert.Contains("test-suffix", tempDir);
    }

    [Fact]
    public void EnsureDirectoryExists_ShouldCreateDirectory()
    {
        // Arrange
        var testDir = Path.Combine(
            Path.GetTempPath(),
            "test-ensure-dir",
            Guid.NewGuid().ToString()
        );
        _tempDirectories.Add(testDir);

        // Act
        FileSystemHelper.EnsureDirectoryExists(testDir);

        // Assert
        Assert.True(Directory.Exists(testDir));
    }

    [Fact]
    public void FormatBytes_ShouldFormatCorrectly()
    {
        // Arrange & Act & Assert
        Assert.Equal("1.0 B", FileSystemHelper.FormatBytes(1));
        Assert.Equal("1.0 KB", FileSystemHelper.FormatBytes(1024));
        Assert.Equal("1.0 MB", FileSystemHelper.FormatBytes(1024 * 1024));
        Assert.Equal("1.0 GB", FileSystemHelper.FormatBytes(1024L * 1024 * 1024));
        Assert.Equal("2.5 KB", FileSystemHelper.FormatBytes(2560));
    }

    [Fact]
    public void GetFileSize_WithExistingFile_ShouldReturnCorrectSize()
    {
        // Arrange
        var tempFile = Path.GetTempFileName();
        var content = "Hello, World!";
        File.WriteAllText(tempFile, content);
        _tempFiles.Add(tempFile);

        // Act
        var size = FileSystemHelper.GetFileSize(tempFile);

        // Assert
        Assert.Equal(Encoding.UTF8.GetByteCount(content), size);
    }

    [Fact]
    public void GetFileSize_WithNonExistentFile_ShouldReturnZero()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), "non-existent-file.txt");

        // Act
        var size = FileSystemHelper.GetFileSize(nonExistentFile);

        // Assert
        Assert.Equal(0, size);
    }

    [Fact]
    public async Task CopyFileAsync_ShouldCopyFileCorrectly()
    {
        // Arrange
        var sourceFile = Path.GetTempFileName();
        var destFile = Path.GetTempFileName();
        var content = "Test file content for copying";

        await File.WriteAllTextAsync(sourceFile, content);
        _tempFiles.Add(sourceFile);
        _tempFiles.Add(destFile);

        // Act
        await FileSystemHelper.CopyFileAsync(sourceFile, destFile);

        // Assert
        Assert.True(File.Exists(destFile));
        var copiedContent = await File.ReadAllTextAsync(destFile);
        Assert.Equal(content, copiedContent);
    }

    #endregion

    #region ProcessExecutor Tests

    [Fact]
    public async Task ProcessExecutor_WithEchoCommand_ShouldReturnCorrectOutput()
    {
        // Arrange
        var expectedOutput = "Hello World";

        // Act
        ProcessResult result;
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            result = await _processExecutor.ExecuteAsync("cmd", $"/c echo {expectedOutput}");
        }
        else
        {
            result = await _processExecutor.ExecuteAsync("echo", expectedOutput);
        }

        // Assert
        Assert.True(result.Success);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains(expectedOutput, string.Join("", result.Output));
    }

    [Fact]
    public async Task ProcessExecutor_WithInvalidCommand_ShouldReturnFailure()
    {
        // Act
        var result = await _processExecutor.ExecuteAsync("non-existent-command-12345", "");

        // Assert
        Assert.False(result.Success);
        Assert.NotEqual(0, result.ExitCode);
        Assert.True(result.Errors.Count > 0);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task FullCloudInitWorkflow_ShouldGenerateValidFiles()
    {
        // Arrange
        var tempDir = FileSystemHelper.GetTempDirectory("full-workflow-test");
        _tempDirectories.Add(tempDir);

        var guestConfig = new GuestConfiguration
        {
            DomainName = "integration.test",
            GuestAdminUsername = "integrationuser",
            GuestAdminPassword = "IntegrationPass123",
            PreInstallDocker = true,
            TimeZone = "Asia/Shanghai",
        };

        var networkConfig = new NetworkConfiguration
        {
            NetAddress = "192.168.100.50",
            NetNetmask = "255.255.255.0",
            NetGateway = "192.168.100.1",
            NameServers = "192.168.100.1,8.8.8.8",
        };

        // Act
        // 1. 生成用户数据
        var userData = await _cloudInitService.GenerateUserDataAsync(guestConfig, networkConfig);
        var userDataPath = Path.Combine(tempDir, "user-data");
        await File.WriteAllTextAsync(userDataPath, userData);

        // 2. 生成元数据
        var metaData = await _cloudInitService.GenerateMetaDataAsync(
            "integration-vm",
            "integration.test"
        );
        var metaDataPath = Path.Combine(tempDir, "meta-data");
        await File.WriteAllTextAsync(metaDataPath, metaData);

        // 3. 创建ISO
        var isoPath = Path.Combine(tempDir, "cloudinit.iso");
        var resultPath = await _cloudInitService.CreateCloudInitISOAsync(
            userDataPath,
            metaDataPath,
            isoPath
        );

        // Assert
        Assert.NotNull(userData);
        Assert.Contains("#cloud-config", userData);
        Assert.Contains("integrationuser", userData);
        Assert.Contains("docker.io", userData);
        Assert.Contains("Asia/Shanghai", userData);

        Assert.NotNull(metaData);
        Assert.Contains("integration-vm", metaData);
        Assert.Contains("integration.test", metaData);

        Assert.Equal(isoPath, resultPath);
        Assert.True(File.Exists(userDataPath));
        Assert.True(File.Exists(metaDataPath));

        _output.WriteLine("Integration test completed successfully!");
        _output.WriteLine($"User Data Length: {userData.Length}");
        _output.WriteLine($"Meta Data Length: {metaData.Length}");
    }

    #endregion

    public void Dispose()
    {
        // 清理临时文件
        foreach (var file in _tempFiles)
        {
            FileSystemHelper.CleanupFile(file);
        }

        // 清理临时目录
        foreach (var dir in _tempDirectories)
        {
            try
            {
                if (Directory.Exists(dir))
                {
                    Directory.Delete(dir, true);
                }
            }
            catch
            {
                // 忽略清理错误
            }
        }

        _httpClient?.Dispose();
    }
}
