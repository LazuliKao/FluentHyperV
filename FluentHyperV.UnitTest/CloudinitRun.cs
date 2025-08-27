using FluentHyperV.Cloudinit.Models;
using FluentHyperV.Cloudinit.Services;
using FluentHyperV.Cloudinit.Utils;
using FluentHyperV.UnitTest.Helper;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FluentHyperV.UnitTest;

public class CloudinitRun
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

    public CloudinitRun(ITestOutputHelper output)
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

    [Fact]
    public async Task TestDownloadImage()
    {
        var config = new ImageConfiguration()
        {
            ImageVersion = "24.04",
            ImageRelease = "release",
            ImageBaseUrl = "https://mirror.nju.edu.cn/ubuntu-cloud-images/releases",
        };
        var url = _cloudImageService.BuildImageUrl(config);
        _output.WriteLine($"Image URL: {url}");
        var cacheDir = FileSystemHelper.GetCacheDirectory(config.ImageVersion);
        await _cloudImageService.DownloadImageAsync(config, cacheDir);
    }
}
