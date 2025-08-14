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
        var result = instance.InvokeFunction("Get-Help");
        Assert.NotNull(result);
        foreach (var psPropertyInfo in result.Properties)
        {
            testOutputHelper.WriteLine(psPropertyInfo.Name);
        }
    }
}
