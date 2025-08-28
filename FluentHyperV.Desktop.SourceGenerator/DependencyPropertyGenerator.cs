using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentHyperV.Desktop.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class DependencyPropertyGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var ns = "FluentHyperV.Desktop";
        context.RegisterPostInitializationOutput(c =>
            c.AddSource(
                "DependencyPropertyAttribute.g.cs",
                $$"""
                #nullable enable
                namespace {{ns}};
                [System.AttributeUsage(System.AttributeTargets.Property)]
                public class DependencyPropertyAttribute(object? defaultValue = null) : System.Attribute
                {
                    public object? DefaultValue { get; } = defaultValue;
                }
                """
            )
        );
        var attributes = context.SyntaxProvider.ForAttributeWithMetadataName(
            $"{ns}.DependencyPropertyAttribute",
            predicate: static (node, _) => node is PropertyDeclarationSyntax,
            transform: static (ctx, _) =>
            {
                var property = (IPropertySymbol)ctx.TargetSymbol;
                var classSymbol = property.ContainingType;

                // 获取属性上的 DependencyPropertyAttribute
                var dependencyPropertyAttr = property
                    .GetAttributes()
                    .FirstOrDefault(attr =>
                        attr.AttributeClass?.Name == "DependencyPropertyAttribute"
                    );
                // 提取默认值
                var (defaultValueExpression, defaultValueType) = ExtractDefaultValueExpression(
                    dependencyPropertyAttr,
                    property.Type.ToDisplayString()
                );

                return new
                {
                    Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
                    ClassName = classSymbol.Name,
                    PropertyName = property.Name,
                    PropertyType = property.Type.ToDisplayString(),
                    PropertyTypeString = property.Type.ToString(),
                    DefaultValue = defaultValueExpression,
                    DefaultValueType = defaultValueType,
                    SyntaxTree = property.DeclaringSyntaxReferences.First().SyntaxTree,
                    TextSpan = property.DeclaringSyntaxReferences.First().Span,
                };
            }
        );
        context.RegisterSourceOutput(
            attributes,
            (ctx, data) =>
            {
                if (
                    data is
                    { DefaultValueType: { } defaultValueType, PropertyTypeString: { } requireType }
                )
                {
                    if (!IsTypeEqual(defaultValueType, requireType))
                    {
                        //report error of type mismatch
                        ctx.ReportDiagnostic(
                            Diagnostic.Create(
                                new(
                                    id: "DPSG001",
                                    title: "Default value type mismatch",
                                    messageFormat: string.Format(
                                        "The default value type '{0}' does not match the property type '{1}' for property '{2}'.",
                                        defaultValueType,
                                        requireType,
                                        data.PropertyName
                                    ),
                                    category: "DependencyPropertyGenerator",
                                    defaultSeverity: DiagnosticSeverity.Error,
                                    isEnabledByDefault: true,
                                    description: "The default value provided in the DependencyPropertyAttribute must be of the same type as the property or be convertible to the property type.",
                                    helpLinkUri: "",
                                    customTags: ""
                                ),
                                Location.Create(data.SyntaxTree, data.TextSpan),
                                defaultValueType,
                                requireType,
                                data.PropertyName
                            )
                        );
                    }
                }
                //ctx.ReportDiagnostic();
                var dProperty = data.PropertyName + "Property";
                ctx.AddSource(
                    $"{data.ClassName}.{data.PropertyName}.g.cs",
                    $$"""
                    namespace {{data.Namespace}};
                    partial class {{data.ClassName}}
                    {
                        public partial {{data.PropertyType}} {{data.PropertyName}}
                        {
                            get => ({{data.PropertyType}})GetValue({{dProperty}});
                            set => SetValue({{dProperty}}, value);
                        }
                        public static readonly DependencyProperty {{dProperty}} = DependencyProperty.Register(
                            nameof({{data.PropertyName}}),
                            typeof({{data.PropertyType}}),
                            typeof({{data.ClassName}}),
                            new({{data.DefaultValue}})
                        );
                    }
                    """
                );
            }
        );
    }

    private static bool IsTypeEqual(string typeA, string typeB)
    {
        // Normalize both types to their canonical form
        var normalizedTypeA = NormalizeTypeName(typeA);
        var normalizedTypeB = NormalizeTypeName(typeB);

        return string.Equals(normalizedTypeA, normalizedTypeB, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeTypeName(string typeName)
    {
        // Handle nullable types by removing the '?' suffix
        var cleanTypeName = typeName.TrimEnd('?');

        // Convert C# type aliases to their full .NET type names
        return cleanTypeName.ToLowerInvariant() switch
        {
            "int" => "system.int32",
            "int32" => "system.int32",
            "long" => "system.int64",
            "int64" => "system.int64",
            "string" => "system.string",
            "bool" => "system.boolean",
            "boolean" => "system.boolean",
            "double" => "system.double",
            "float" => "system.single",
            "single" => "system.single",
            "decimal" => "system.decimal",
            "byte" => "system.byte",
            "sbyte" => "system.sbyte",
            "short" => "system.int16",
            "int16" => "system.int16",
            "ushort" => "system.uint16",
            "uint16" => "system.uint16",
            "uint" => "system.uint32",
            "uint32" => "system.uint32",
            "ulong" => "system.uint64",
            "uint64" => "system.uint64",
            "char" => "system.char",
            "object" => "system.object",
            _ => cleanTypeName.ToLowerInvariant(),
        };
    }

    /// <summary>
    /// 从 DependencyPropertyAttribute 中提取默认值并生成相应的 C# 表达式
    /// </summary>
    /// <param name="attribute">DependencyPropertyAttribute 实例</param>
    /// <param name="type">类型</param>
    /// <returns>默认值的 C# 表达式字符串</returns>
    private static (string defaultValue, string? type) ExtractDefaultValueExpression(
        AttributeData? attribute,
        string type
    )
    {
        if (attribute?.ConstructorArguments.Length > 0)
        {
            var defaultValueArg = attribute.ConstructorArguments[0];
            if (!defaultValueArg.IsNull)
            {
                return (
                    "(" + type + ")" + defaultValueArg.ToCSharpString(),
                    defaultValueArg.Value?.GetType().ToString()
                );
                //var defaultValue = defaultValueArg.Value;
                //return ConvertValueToExpression(defaultValue);
            }
        }

        return ("default", null);
    }

    ///// <summary>
    ///// 将对象值转换为相应的 C# 表达式字符串
    ///// </summary>
    ///// <param name="value">要转换的值</param>
    ///// <returns>C# 表达式字符串</returns>
    //private static string ConvertValueToExpression(object? value)
    //{
    //    if (value == null)
    //    {
    //        return "null";
    //    }

    //    return value switch
    //    {
    //        string str => $"\"{EscapeString(str)}\"",
    //        bool b => b.ToString().ToLower(),
    //        int i => i.ToString(),
    //        uint ui => ui.ToString() + "u",
    //        long l => l.ToString() + "L",
    //        ulong ul => ul.ToString() + "UL",
    //        float f => f.ToString("G9") + "f",
    //        double d => d.ToString("G17") + "d",
    //        decimal dec => dec.ToString() + "m",
    //        char c => $"'{EscapeChar(c)}'",
    //        byte b => b.ToString(),
    //        sbyte sb => sb.ToString(),
    //        short s => s.ToString(),
    //        ushort us => us.ToString(),
    //        _ => value.ToString() ?? "default",
    //    };
    //}

    ///// <summary>
    ///// 转义字符串中的特殊字符
    ///// </summary>
    ///// <param name="str">要转义的字符串</param>
    ///// <returns>转义后的字符串</returns>
    //private static string EscapeString(string str)
    //{
    //    return str.Replace("\\", "\\\\")
    //        .Replace("\"", "\\\"")
    //        .Replace("\r", "\\r")
    //        .Replace("\n", "\\n")
    //        .Replace("\t", "\\t");
    //}

    ///// <summary>
    ///// 转义字符中的特殊字符
    ///// </summary>
    ///// <param name="c">要转义的字符</param>
    ///// <returns>转义后的字符</returns>
    //private static string EscapeChar(char c)
    //{
    //    return c switch
    //    {
    //        '\\' => "\\\\",
    //        '\'' => "\\'",
    //        '\r' => "\\r",
    //        '\n' => "\\n",
    //        '\t' => "\\t",
    //        '\0' => "\\0",
    //        _ => c.ToString(),
    //    };
    //}
}
