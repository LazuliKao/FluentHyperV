using System;
using System.Management.Automation;
using FluentHyperV.SourceGenerator;

namespace FluentHyperV.Models;

public partial class VirtualMachine : IPSObjectMapper
{
    public string? Name { get; set; }
    public string? Id { get; set; }
    public string? State { get; set; }
    public long? MemoryAssigned { get; set; }
    public int? ProcessorCount { get; set; }
    public DateTime? CreationTime { get; set; }
    public bool? DynamicMemoryEnabled { get; set; }

    //[PsProperty("VMId")]
    public string? VirtualMachineId { get; set; }

    //[PsIgnore]
    public string? InternalProperty { get; set; }
}

public partial class HelpResult : IPSObjectMapper
{
    public String Name { get; set; }
    public String Category { get; set; }
    public String Synopsis { get; set; }
    public String Component { get; set; }
    public String Role { get; set; }
    public String Functionality { get; set; }
    public Int32 Length { get; set; }
}
