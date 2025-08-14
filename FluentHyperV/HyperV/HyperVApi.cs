using FluentHyperV.Powershell;
using FluentHyperV.SourceGenerator;

namespace FluentHyperV.HyperV;

internal class HyperVApi : IDisposable
{
    private readonly PowerShellInstance _powerShellInstance = new("Import-Module Hyper-V");

    public PowerShellInstance PowerShellInstance => _powerShellInstance;

    public void Dispose() => _powerShellInstance.Dispose();
}

[PsObjectMapper]
public partial class GetVMResult
{
    public string Name { get; set; } = string.Empty;
}
