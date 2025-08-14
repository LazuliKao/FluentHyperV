using FluentHyperV.Powershell;
using FluentHyperV.SourceGenerator;

namespace FluentHyperV.HyperV;

internal class HyperVApi : IDisposable
{
    private readonly PowerShellInstance _powerShellInstance = new("Import-Module Hyper-V");

    public PowerShellInstance PowerShellInstance => _powerShellInstance;

    public void Dispose() => _powerShellInstance.Dispose();
}

public partial class GetVMResult : IPSObjectMapper
{
    public string Name { get; set; } = string.Empty;
}
