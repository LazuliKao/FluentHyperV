using System;
using FluentHyperV.SourceGenerator;

namespace FluentHyperV.Models;

[PsObjectMapper(IgnoreCase = true)]
public partial class VirtualMachine
{
    public string? Name { get; set; }
    public string? Id { get; set; }
    public string? State { get; set; }
    public long? MemoryAssigned { get; set; }
    public int? ProcessorCount { get; set; }
    public DateTime? CreationTime { get; set; }
    public bool? DynamicMemoryEnabled { get; set; }

    [PsProperty("VMId")]
    public string? VirtualMachineId { get; set; }

    [PsIgnore]
    public string? InternalProperty { get; set; }
}
