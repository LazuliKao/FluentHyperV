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
    public bool CommonParameters { get; set; }
    public string aliases { get; set; }
    public string remarks { get; set; }
    public object alertSet { get; set; }
    public object description { get; set; }
    public object examples { get; set; }
    public string Synopsis { get; set; }
    public string ModuleName { get; set; }
    public string nonTerminatingErrors { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public object Component { get; set; }
    public object Role { get; set; }
    public object Functionality { get; set; }

    // ExtendedCmdletHelpInfo#details details;
    // ExtendedCmdletHelpInfo#syntax Syntax;
    // ExtendedCmdletHelpInfo#parameters parameters;
    // ExtendedCmdletHelpInfo#inputTypes inputTypes;
    // ExtendedCmdletHelpInfo#relatedLinks relatedLinks;
    // ExtendedCmdletHelpInfo#returnValues returnValues;

    // public ExtendedCmdletHelpInfoParameters[] parameters { get; set; }

    public object Syntax { get; set; }
    public ExtendedCmdletHelpInfoParameters[] parameters { get; set; }
    public object inputTypes { get; set; }
    public object relatedLinks { get; set; }
    public object returnValues { get; set; }
}

public partial class ExtendedCmdletHelpInfoDetails : IPSObjectMapper
{
    public object returnValues { get; set; }
}

public partial class ExtendedCmdletHelpInfoInputTypes : IPSObjectMapper
{
    public string inputType { get; set; }
}

public partial class ExtendedCmdletHelpInfoParameters : IPSObjectMapper
{
    public System.String name { get; set; }
    public System.String required { get; set; }
    public System.String pipelineInput { get; set; }
    public System.String isDynamic { get; set; }
    public System.String globbing { get; set; }

    public System.String parameterSetName { get; set; }

    // public  ExtendedCmdletHelpInfo#type type { get;set;
    public System.String position { get; set; }
    public System.String aliases { get; set; }
}
