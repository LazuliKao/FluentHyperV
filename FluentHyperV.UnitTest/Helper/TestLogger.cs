using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace FluentHyperV.UnitTest.Helper;

/// <summary>
/// 测试用的Logger实现
/// </summary>
public class TestLogger : ILogger
{
    private readonly ITestOutputHelper _output;
    private readonly string _categoryName;

    public TestLogger(ITestOutputHelper output, string categoryName)
    {
        _output = output;
        _categoryName = categoryName;
    }

    public IDisposable BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter
    )
    {
        try
        {
            var message = formatter(state, exception);
            _output.WriteLine($"[{logLevel}] {_categoryName}: {message}");
            if (exception != null)
            {
                _output.WriteLine($"Exception: {exception}");
            }
        }
        catch
        {
            // 忽略测试日志错误
        }
    }
}
