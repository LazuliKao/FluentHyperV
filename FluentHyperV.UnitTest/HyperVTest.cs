using FluentHyperV.HyperV;
using FluentHyperV.PowerShell;
using Microsoft.HyperV.PowerShell;
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
            testOutputHelper.WriteLine(
                $"Name: {virtualMachine.Name} Generation: {virtualMachine.Generation}"
            );
        }
    }

    [Fact]
    public void TestNewVM()
    {
        var testVmName = "test";
        if (_hyperV.GetVM(new()).FirstOrDefault(x => x.Name == testVmName) is { } vmLegacy)
        {
            _hyperV.RemoveVM(new() { VM = [vmLegacy], Name = null });
        }
        var vm = _hyperV.NewVM(
            new()
            {
                NewVHDPath = @"Y:\tmp\test.vhdx",
                NewVHDSizeBytes = 1u * 1024u * 1_024u * 1_024u,
                VHDPath = null,
                Path = @"Y:\tmp",
                Name = testVmName,
                Generation = 2,
                MemoryStartupBytes = 1024 * 1_024 * 1_024,
                SwitchName = "WAN",
                //BootDevice = BootDevice.CD,

                GuestStateIsolationType = GuestIsolationType.TrustedLaunch,
            }
        );

        var fw = _hyperV.SetVMFirmware(
            new()
            {
                VM = vm,
                VMName = null,
                VMFirmware = null,
                EnableSecureBoot = OnOffState.Off,
            }
        );
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
