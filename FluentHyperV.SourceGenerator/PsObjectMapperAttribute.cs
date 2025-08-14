using System;

namespace FluentHyperV.SourceGenerator;

/// <summary>
/// Attribute to mark classes that should have PSObject mapper methods generated
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PsObjectMapperAttribute : Attribute
{
    /// <summary>
    /// Optional prefix for property names in PSObject
    /// </summary>
    public string? PropertyPrefix { get; set; }

    /// <summary>
    /// Whether to ignore case when mapping properties
    /// </summary>
    public bool IgnoreCase { get; set; } = true;
}
