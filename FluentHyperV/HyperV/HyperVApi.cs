using FluentHyperV.PowerShell;
using FluentHyperV.SourceGenerator;

namespace FluentHyperV.HyperV;

internal class HyperVApi
{
    private readonly Lazy<HyperVInstance> _powerShellInstance = new(() => new HyperVInstance());

    public HyperVInstance PowerShellInstance => _powerShellInstance.Value;
}
//
// public partial class GetVMResult : IPSObjectMapper
// {
//     public string Name { get; set; } = string.Empty;
// }
