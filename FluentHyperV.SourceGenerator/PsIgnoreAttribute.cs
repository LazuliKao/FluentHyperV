using System;

namespace FluentHyperV.SourceGenerator;

/// <summary>
/// Attribute to mark properties that should be ignored during mapping
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class PsIgnoreAttribute : Attribute { }
