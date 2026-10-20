using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Configuration;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// partial class の宣言数上限を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PartialClassDeclarationLimitAnalyzer : DiagnosticAnalyzer
{
    private const int DefaultMaxPartialClassDeclarationCount = 4;

    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.PartialClassDeclarationLimitRule);

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
            int maxPartialClassDeclarationCount = AnalyzerConfigurationReader.GetPartialClassDeclarationLimit(
                compilationContext.Options,
                DefaultMaxPartialClassDeclarationCount,
                compilationContext.CancellationToken);

            compilationContext.RegisterSymbolAction(
                symbolContext => AnalyzeNamedType(symbolContext, maxPartialClassDeclarationCount),
                SymbolKind.NamedType);
        });
    }

    /// <summary>
    /// 型宣言を解析します。
    /// </summary>
    /// <param name="context">シンボル解析コンテキストです。</param>
    /// <param name="maxPartialClassDeclarationCount">許容するpartial class宣言数上限です。</param>
    private static void AnalyzeNamedType(SymbolAnalysisContext context, int maxPartialClassDeclarationCount)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol
            || typeSymbol.TypeKind != TypeKind.Class
            || typeSymbol.IsImplicitlyDeclared)
        {
            return;
        }

        List<ClassDeclarationSyntax> partialDeclarations = [];
        foreach (SyntaxReference syntaxReference in typeSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax(context.CancellationToken) is not ClassDeclarationSyntax classDeclaration)
            {
                continue;
            }

            if (!classDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.PartialKeyword)))
            {
                continue;
            }

            if (!ProductionCodeFilter.IsProductionCode(classDeclaration.SyntaxTree, context.Compilation))
            {
                continue;
            }

            partialDeclarations.Add(classDeclaration);
        }

        if (partialDeclarations.Count <= maxPartialClassDeclarationCount)
        {
            return;
        }

        partialDeclarations.Sort(CompareClassDeclarationLocation);
        Location reportLocation = partialDeclarations[maxPartialClassDeclarationCount].Identifier.GetLocation();
        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.PartialClassDeclarationLimitRule,
                reportLocation,
                typeSymbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                partialDeclarations.Count,
                maxPartialClassDeclarationCount));
    }

    /// <summary>
    /// クラス宣言のソース上の安定順序を比較します。
    /// </summary>
    /// <param name="left">左辺のクラス宣言です。</param>
    /// <param name="right">右辺のクラス宣言です。</param>
    /// <returns>比較結果です。</returns>
    private static int CompareClassDeclarationLocation(ClassDeclarationSyntax left, ClassDeclarationSyntax right)
    {
        int pathComparison = string.Compare(left.SyntaxTree.FilePath, right.SyntaxTree.FilePath, StringComparison.Ordinal);
        if (pathComparison != 0)
        {
            return pathComparison;
        }

        return left.SpanStart.CompareTo(right.SpanStart);
    }
}
