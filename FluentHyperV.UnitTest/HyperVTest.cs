using FluentHyperV.HyperV;
using FluentHyperV.PowerShell;
using Xunit.Abstractions;

namespace FluentHyperV.UnitTest;

public class HyperVTest(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void TestInstance()
    {
        var hyperV = new HyperVApi();
        var results = hyperV.Get_VM(new HyperVApi.Get_VMArguments { });
        testOutputHelper.WriteLine(results.Length.ToString());
        foreach (var virtualMachine in results)
        {
            testOutputHelper.WriteLine(virtualMachine.Name);
        }
    }
}
