using FluentHyperV.HyperV;
using FluentHyperV.PowerShell;
using Xunit.Abstractions;

namespace FluentHyperV.UnitTest;

public class HyperVTest(ITestOutputHelper testOutputHelper)
{
    private readonly HyperVApi _hyperV = new();
    private HyperVApi Api => _hyperV;

    [Fact]
    public void TestInstance()
    {
        var results = _hyperV.GetVM(new() { });
        testOutputHelper.WriteLine(results.Length.ToString());
        foreach (var virtualMachine in results)
        {
            testOutputHelper.WriteLine(virtualMachine.Name);
        }
    }

    [Fact]
    public void TestVhd()
    {
        //var results = _hyperV.GetVHD(
        //    new()
        //    {
        //        Path = [@"A:\HyperV\Win11VM\Win11Data.vhdx"],
        //        DiskNumber = 0,
        //        VMId = [],
        //    }
        //);
    }
}
