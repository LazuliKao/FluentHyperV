using FluentHyperV.PowerShell;
using Xunit.Abstractions;

namespace FluentHyperV.UnitTest;

public class HyperVTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestInstance()
    {
        var ps = new PowerShellInstance();
        var resultRaw = ps.InvokeFunctionJson("Get-Help", new() { ["Name"] = "Get-VM" });
        if (resultRaw is null)
        {
            throw new Exception("Failed to get help for Get-VM");
        }
        var result = resultRaw.RootElement;
        testOutputHelper.WriteLine(result.GetRawText());
    }
}
