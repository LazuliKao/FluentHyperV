using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FluentHyperV.Desktop.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public class ConverterInstanceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 1. 找所有类
        var classDeclarations = context
            .SyntaxProvider.CreateSyntaxProvider(
                predicate: (s, _) => s is ClassDeclarationSyntax, // 粗筛：语法层面是类
                transform: (ctx, _) =>
                {
                    var classDecl = (ClassDeclarationSyntax)ctx.Node;
                    var symbol = ctx.SemanticModel.GetDeclaredSymbol(classDecl) as INamedTypeSymbol;
                    return symbol;
                }
            )
            .Where(symbol => symbol is not null);

        // 2. 筛选实现了 IValueConverter 的类
        var notifyClasses = classDeclarations.Where(symbol =>
            symbol!.AllInterfaces.Any(i =>
                i.ToDisplayString() switch
                {
                    "System.Windows.Data.IValueConverter" => true,
                    "System.Windows.Data.IMultiValueConverter" => true,
                    _ => false,
                }
            )
        );
        // 3. 聚合所有符号
        var collected = context.CompilationProvider.Combine(notifyClasses.Collect());
        // 4. 输出代码
        context.RegisterSourceOutput(
            collected,
            (spc, v) =>
            {
                var (c, list) = v;
                var ns = c.AssemblyName;
                var sb = new StringBuilder();
                sb.AppendLine("using System;");
                sb.AppendLine("using System.Windows.Data;");
                sb.AppendLine($"namespace {ns}.Converters;");
                sb.AppendLine("public static class ConverterInstance");
                sb.AppendLine("{");
                var subClass =
                    new List<(
                        INamedTypeSymbol symbolNamed,
                        string name,
                        INamedTypeSymbol containingType,
                        string accessibility
                    )>();

                void GenerateInstance(
                    INamedTypeSymbol symbol,
                    string name,
                    string accessibility,
                    int indent = 1
                )
                {
                    var fieldName =
                        $"_{name.Substring(0, 1).ToLowerInvariant()}{name.Substring(1)}";
                    var indentSpace = new string(' ', 4 * indent);
                    sb.AppendLine(
                        $"{indentSpace}private static Lazy<{symbol.ToDisplayString()}> {fieldName} = new(() => new());"
                    );
                    sb.AppendLine(
                        $"{indentSpace}{accessibility} static {symbol.ToDisplayString()} {name} => {fieldName}.Value;"
                    );
                    sb.AppendLine();
                }
                foreach (var symbol in list.Distinct(SymbolEqualityComparer.Default))
                {
                    if (symbol is INamedTypeSymbol symbolNamed)
                    {
                        //symbolNamed.ContainingNamespace
                        var ct = symbolNamed.ContainingType;
                        var name = symbolNamed.Name;
                        if (name.EndsWith("Converter"))
                            name = name.Remove(name.Length - "Converter".Length);
                        var accessibility =
                            symbolNamed.DeclaredAccessibility == Accessibility.Public
                                ? "public"
                                : "internal";
                        if (ct is not null)
                        {
                            subClass.Add((symbolNamed, name, ct, accessibility));
                        }
                        else
                        {
                            GenerateInstance(symbolNamed, name, accessibility);
                        }
                    }
                }
                foreach (
                    var grouping in from x in subClass
                    let type = x.containingType.Name
                    group x by type
                )
                {
                    var containingClass = grouping.Key;
                    sb.AppendLine(
                        $$"""
                            public static class {{containingClass}}
                            {
                        """
                    );
                    foreach (var (symbolNamed, name, _, accessibility) in grouping)
                    {
                        GenerateInstance(symbolNamed, name, accessibility, 2);
                    }

                    sb.AppendLine("    }");
                }
                sb.AppendLine("}");
                spc.AddSource(
                    "ConverterInstance.g.cs",
                    SourceText.From(sb.ToString(), Encoding.UTF8)
                );
            }
        );
    }
}
