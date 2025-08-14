using FluentHyperV.Models;
using FluentHyperV.Powershell;
using Json.More;
using Xunit.Abstractions;

namespace FluentHyperV.UnitTest;

public class PowershellTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestInstance()
    {
        using var instance = new PowerShellInstance();
        var result = instance.InvokeFunction<HelpResult>(
            "Get-Help",
            onError: err =>
            {
                testOutputHelper.WriteLine(err.Message);
            }
        );
        Assert.NotNull(result);
        testOutputHelper.WriteLine(result.ToJsonDocument().RootElement.ToJsonString());
    }
}
