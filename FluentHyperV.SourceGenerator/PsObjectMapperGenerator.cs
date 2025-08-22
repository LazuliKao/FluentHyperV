using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FluentHyperV.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class PsObjectMapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 注册源输出
        context.RegisterPostInitializationOutput(static context =>
        {
            context.AddSource(
                "IPSObject.g.cs",
                """
                #nullable enable
                using System.Management.Automation;

                namespace FluentHyperV.SourceGenerator;

                public interface IPSObjectMapper
                {
                    void LoadFrom(PSObject psObject, Action<Exception>? onError=null);
                }
                """
            );
            context.AddSource(
                "PsIgnoreAttribute.g.cs",
                """
                using System;

                namespace FluentHyperV.SourceGenerator;

                /// <summary>
                /// Attribute to mark properties that should be ignored during mapping
                /// </summary>
                [AttributeUsage(AttributeTargets.Property)]
                public class PsIgnoreAttribute : Attribute { }
                """
            );
            context.AddSource(
                "PsPropertyAttribute.g.cs",
                """
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
                """
            );
        });

        // 创建增量提供器来查找实现 IPSObjectMapper 的类
        var classDeclarations = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)
            )
            .Where(static m => m is not null);

        // 注册源输出生成器
        context.RegisterSourceOutput(
            classDeclarations,
            static (spc, source) => Execute(source!, spc)
        );
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        // 检查是否为类声明且实现了接口
        return node is ClassDeclarationSyntax { BaseList.Types.Count: > 0 };
    }

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(
        GeneratorSyntaxContext context
    )
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

        // 检查是否实现了 IPSObjectMapper 接口
        foreach (
            var baseType in classDeclarationSyntax.BaseList?.Types
                ?? Enumerable.Empty<BaseTypeSyntax>()
        )
        {
            var typeInfo = context.SemanticModel.GetTypeInfo(baseType.Type);
            if (typeInfo.Type?.Name == "IPSObjectMapper")
            {
                return classDeclarationSyntax;
            }
        }

        return null;
    }

    private static void Execute(
        ClassDeclarationSyntax classDeclaration,
        SourceProductionContext context
    )
    {
        if (classDeclaration is null)
            return;

        var namespaceName = GetNamespace(classDeclaration);
        var className = classDeclaration.Identifier.ValueText;

        // 获取所有公共属性
        var properties = classDeclaration
            .Members.OfType<PropertyDeclarationSyntax>()
            .Where(p => p.Modifiers.Any(SyntaxKind.PublicKeyword))
            .Where(p =>
                p.AccessorList?.Accessors.Any(a => a.IsKind(SyntaxKind.SetAccessorDeclaration))
                == true
            )
            .ToList();

        if (!properties.Any())
            return;

        var sourceBuilder = new StringBuilder();

        // 添加 using 语句
        sourceBuilder.AppendLine("using System;");
        sourceBuilder.AppendLine("using System.Management.Automation;");
        sourceBuilder.AppendLine();

        // 添加命名空间
        if (!string.IsNullOrEmpty(namespaceName))
        {
            sourceBuilder.AppendLine($"namespace {namespaceName}");
            sourceBuilder.AppendLine("{");
        }

        // 生成部分类
        sourceBuilder.AppendLine($"    public partial class {className}");
        sourceBuilder.AppendLine("    {");
        sourceBuilder.AppendLine(
            "        public void LoadFrom(PSObject psObject, Action<Exception?>? onError = null)"
        );
        sourceBuilder.AppendLine("        {");
        sourceBuilder.AppendLine("            if (psObject == null)");
        sourceBuilder.AppendLine("                return;");
        sourceBuilder.AppendLine();

        // 为每个属性生成映射代码
        foreach (var property in properties)
        {
            var propertyName = property.Identifier.ValueText;
            var propertyType = property.Type.ToString();

            // 检查是否有 PsIgnore 特性
            if (HasPsIgnoreAttribute(property))
                continue;

            // 获取 PSObject 属性名（可能通过 PsProperty 特性自定义）
            var psPropertyName = GetPsPropertyName(property);

            sourceBuilder.AppendLine($"            // 映射属性: {propertyName}");
            sourceBuilder.AppendLine(
                $"            if (psObject.Properties[\"{psPropertyName}\"] != null)"
            );
            sourceBuilder.AppendLine("            {");
            sourceBuilder.AppendLine($"                try");
            sourceBuilder.AppendLine("                {");
            sourceBuilder.AppendLine(
                $"                    var value = psObject.Properties[\"{psPropertyName}\"].Value;"
            );
            sourceBuilder.AppendLine($"                    if (value != null)");
            sourceBuilder.AppendLine("                    {");

            // 对于属性类型生成不同的转换代码
            GeneratePropertyAssignment(sourceBuilder, propertyName, propertyType);

            sourceBuilder.AppendLine("                    }");
            sourceBuilder.AppendLine($"                }}");
            sourceBuilder.AppendLine($"                catch (Exception ex)");
            sourceBuilder.AppendLine("                {");
            sourceBuilder.AppendLine("                    onError?.Invoke(ex);");
            sourceBuilder.AppendLine("                }");
            sourceBuilder.AppendLine("            }");
            sourceBuilder.AppendLine();
        }

        sourceBuilder.AppendLine("        }");
        sourceBuilder.AppendLine("    }");

        if (!string.IsNullOrEmpty(namespaceName))
        {
            sourceBuilder.AppendLine("}");
        }

        context.AddSource($"{className}.LoadFrom.g.cs", sourceBuilder.ToString());
    }

    private static string GetNamespace(ClassDeclarationSyntax classDeclaration)
    {
        var namespaceSyntax = classDeclaration
            .Ancestors()
            .OfType<NamespaceDeclarationSyntax>()
            .FirstOrDefault();
        if (namespaceSyntax != null)
            return namespaceSyntax.Name.ToString();

        var fileScopedNamespace = classDeclaration
            .Ancestors()
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .FirstOrDefault();
        return fileScopedNamespace?.Name.ToString() ?? "";
    }

    private static bool HasPsIgnoreAttribute(PropertyDeclarationSyntax property)
    {
        return property
            .AttributeLists.SelectMany(al => al.Attributes)
            .Any(attr => attr.Name.ToString().Contains("PsIgnore"));
    }

    private static bool HasInterface(ClassDeclarationSyntax classDeclaration, string interfaceName)
    {
        return classDeclaration.BaseList?.Types.Any(t => t.Type.ToString() == interfaceName)
            ?? false;
    }

    private static string GetPsPropertyName(PropertyDeclarationSyntax property)
    {
        var psPropertyAttr = property
            .AttributeLists.SelectMany(al => al.Attributes)
            .FirstOrDefault(attr => attr.Name.ToString().Contains("PsProperty"));

        if (psPropertyAttr?.ArgumentList?.Arguments.Count > 0)
        {
            var firstArg = psPropertyAttr.ArgumentList.Arguments[0];
            if (firstArg.Expression is LiteralExpressionSyntax literal)
            {
                return literal.Token.ValueText;
            }
        }

        return property.Identifier.ValueText;
    }

    private static void GeneratePropertyAssignment(
        StringBuilder sourceBuilder,
        string propertyName,
        string propertyType
    )
    {
        // 处理不同的数据类型
        switch (propertyType.ToLower())
        {
            case "string":
                sourceBuilder.AppendLine(
                    $"                        {propertyName} = value.ToString();"
                );
                break;
            case "int":
            case "int32":
                sourceBuilder.AppendLine(
                    $"                        {propertyName} = Convert.ToInt32(value);"
                );
                break;
            case "long":
            case "int64":
                sourceBuilder.AppendLine(
                    $"                        {propertyName} = Convert.ToInt64(value);"
                );
                break;
            case "double":
                sourceBuilder.AppendLine(
                    $"                        {propertyName} = Convert.ToDouble(value);"
                );
                break;
            case "float":
                sourceBuilder.AppendLine(
                    $"                        {propertyName} = Convert.ToSingle(value);"
                );
                break;
            case "bool":
            case "boolean":
                sourceBuilder.AppendLine(
                    $"                        {propertyName} = Convert.ToBoolean(value);"
                );
                break;
            case "datetime":
                sourceBuilder.AppendLine(
                    $"                        {propertyName} = Convert.ToDateTime(value);"
                );
                break;
            case "guid":
                sourceBuilder.AppendLine(
                    $"                        {propertyName} = Guid.Parse(value.ToString());"
                );
                break;
            default:
                // 对于数组或集合类型 需要处理每个元素

                // 对于可空类型和其他类型
                var targetType = propertyType.TrimEnd('?');
                if (propertyType == "object" || propertyType == "dynamic")
                {
                    sourceBuilder.AppendLine($"                        {propertyName} = value;");
                    return;
                }

                if (TryExtractArrayType(propertyType, out var arrayType))
                {
                    sourceBuilder.AppendLine(
                        "\n                        "
                            + $$"""
                            var array = (object[])((PSObject)value).Properties.First().Value;
                            {{propertyName}} = array.Select(x =>
                            {
                                var e = new {{arrayType}}();
                                e.LoadFrom((PSObject)x);
                                return e;
                            })
                            .ToArray();
                            """.Replace("\n", "\n                        ")
                    );
                }
                else
                {
                    sourceBuilder.AppendLine(
                        $"                        {propertyName} = ({propertyType})Convert.ChangeType(value, typeof({targetType}));"
                    );
                }

                break;
        }
    }

    private static bool TryExtractArrayType(string propertyType, out string elementType)
    {
        if (propertyType.StartsWith("IEnumerable<") && propertyType.EndsWith(">"))
        {
            elementType = propertyType.Substring(1, propertyType.Length - 2);
            // elementType = propertyType[11..^1]; // 提取类型
            return true;
        }

        if (propertyType.StartsWith("List<") && propertyType.EndsWith(">"))
        {
            elementType = propertyType.Substring(5, propertyType.Length - 6);
            // elementType = propertyType[5..^1]; // 提取类型
            return true;
        }

        if (propertyType.StartsWith("Array<") && propertyType.EndsWith(">"))
        {
            elementType = propertyType.Substring(6, propertyType.Length - 7);
            // elementType = propertyType[6..^1]; // 提取类型
            return true;
        }

        if (propertyType.EndsWith("[]"))
        {
            // 处理数组类型
            elementType = propertyType.Substring(0, propertyType.Length - 2);
            // elementType = propertyType[..^2]; // 提取类型
            return true;
        }

        elementType = string.Empty;
        return false;
    }
}
