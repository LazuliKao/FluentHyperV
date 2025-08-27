using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace FluentHyperV.Desktop.SourceGenerator;

//[Generator(LanguageNames.CSharp)]
//public class DependencyPropertyGenerator : IIncrementalGenerator
//{
//    public void Initialize(IncrementalGeneratorInitializationContext context)
//    {
//        var ns = "FluentHyperV.Desktop";
//        context.RegisterPostInitializationOutput(c =>
//            c.AddSource(
//                "DependencyPropertyAttribute.g.cs",
//                $$"""
//                namespace {{ns}};
//                [System.AttributeUsage(System.AttributeTargets.Property)]
//                public class DependencyPropertyAttribute : System.Attribute { }
//                """
//            )
//        );
//        var d = context.SyntaxProvider.ForAttributeWithMetadataName(
//            $"{ns}.DependencyPropertyAttribute",
//            predicate: static (node, _) => node is PropertyDeclarationSyntax,
//            transform: static (ctx, _) =>
//            {
//                var property = (IPropertySymbol)ctx.TargetSymbol;
//                var classSymbol = property.ContainingType;
//                return new
//                {
//                    Namespace = classSymbol.ContainingNamespace.ToDisplayString(),
//                    ClassName = classSymbol.Name,
//                    PropertyName = property.Name,
//                    PropertyType = property.Type.ToDisplayString()
//                };
//            })
//        );
//    }
//}

[Generator(LanguageNames.CSharp)]
public class ServiceGenerator : IIncrementalGenerator
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

        // 2. 筛选实现了 INotifyPropertyChanged 的类
        var notifyClasses = classDeclarations.Where(symbol =>
            symbol!.AllInterfaces.Any(i =>
                i.ToDisplayString() switch
                {
                    "System.ComponentModel.INotifyPropertyChanged" => true,
                    "System.ComponentModel.INotifyPropertyChanging" => true,
                    "Wpf.Ui.Abstractions.Controls.INavigationAware" => true,
                    var v => v.StartsWith("Wpf.Ui.Abstractions.Controls.INavigableView<"),
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
                sb.AppendLine("using Microsoft.Extensions.DependencyInjection;");
                sb.AppendLine($"namespace {ns};");
                sb.AppendLine("public static class Registry");
                sb.AppendLine("{");
                sb.AppendLine(
                    "    public static void AddProjectPageAndViewModels(this IServiceCollection services)"
                );
                sb.AppendLine("    {");
                foreach (var symbol in list.Distinct(SymbolEqualityComparer.Default))
                {
                    if (symbol is INamedTypeSymbol symbolNamed)
                    {
                        var hasTransientAttr = symbolNamed
                            .GetAttributes()
                            .Any(attr =>
                                attr.AttributeClass?.MetadataName
                                    is "DependencyInjectionTransient"
                                        or "DependencyInjectionTransientAttribute"
                            );
                        sb.AppendLine(
                            hasTransientAttr
                                ? $"        services.AddTransient<{symbolNamed.ToDisplayString()}>();"
                                : $"        services.AddSingleton<{symbolNamed.ToDisplayString()}>();"
                        );
                    }
                }

                sb.AppendLine("  }");
                sb.AppendLine("}");
                spc.AddSource("ModelRegistry.g.cs", SourceText.From(sb.ToString(), Encoding.UTF8));
            }
        );
        context.RegisterSourceOutput(
            context.CompilationProvider,
            (s, c) =>
                s.AddSource(
                    "DependencyInjectionTransientAttribute.g.cs",
                    $$"""
                    namespace {{c.AssemblyName}};
                    public class DependencyInjectionTransientAttribute : System.Attribute { }
                    """
                )
        );
    }
}
