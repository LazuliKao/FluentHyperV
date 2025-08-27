using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FluentHyperV.UnitTest.Helper;

/// <summary>
/// 测试用的Logger Provider，将日志输出到测试输出
/// </summary>
public class TestLoggerProvider : ILoggerProvider
{
    private readonly ITestOutputHelper _output;

    public TestLoggerProvider(ITestOutputHelper output)
    {
        _output = output;
    }

    public ILogger CreateLogger(string categoryName)
    {
        return new TestLogger(_output, categoryName);
    }

    public void Dispose() { }
}
