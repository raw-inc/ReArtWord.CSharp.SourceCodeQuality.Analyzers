using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Configuration;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// .NET10以降のDllImport利用に対してLibraryImportへの移行検討を促すAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class LibraryImportRecommendationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.LibraryImportRecommendationRule);

    /// <summary>
    /// Analyzer初期化処理です。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationStartAction(static compilationContext =>
        {
            Compilation compilation = compilationContext.Compilation;
            if (!TargetFrameworkVersionDetector.IsNet10OrGreater(compilation)
                || compilation.GetTypeByMetadataName("System.Runtime.InteropServices.LibraryImportAttribute") is null)
            {
                return;
            }

            INamedTypeSymbol? dllImportAttribute = compilation.GetTypeByMetadataName("System.Runtime.InteropServices.DllImportAttribute");
            if (dllImportAttribute is null
                || SymbolEqualityComparer.Default.Equals(dllImportAttribute.ContainingAssembly, compilation.Assembly))
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                syntaxContext => AnalyzeAttribute(syntaxContext, dllImportAttribute),
                SyntaxKind.Attribute);
        });
    }

    /// <summary>
    /// 実際のDllImport属性の利用箇所を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="dllImportAttribute">参照先のDllImport属性型です。</param>
    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context, INamedTypeSymbol dllImportAttribute)
    {
        if (context.Node is not AttributeSyntax attribute
            || !ProductionCodeFilter.IsProductionCode(attribute.SyntaxTree, context.Compilation))
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(attribute, context.CancellationToken).Symbol is not IMethodSymbol constructor
            || !SymbolEqualityComparer.Default.Equals(constructor.ContainingType, dllImportAttribute))
        {
            return;
        }

        SyntaxToken identifier = attribute.Name.GetLastToken();
        context.ReportDiagnostic(Diagnostic.Create(DiagnosticCatalog.LibraryImportRecommendationRule, identifier.GetLocation()));
    }
}
