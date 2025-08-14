using System;

namespace FluentHyperV.SourceGenerator;

/// <summary>
/// Attribute to specify custom property name mapping
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PsPropertyAttribute : Attribute
{
    public string Name { get; }

    public PsPropertyAttribute(string name)
    {
        Name = name;
    }
}
