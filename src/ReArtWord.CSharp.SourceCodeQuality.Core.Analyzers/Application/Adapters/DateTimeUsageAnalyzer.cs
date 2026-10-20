using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Configuration;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// DateTime型利用を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DateTimeUsageAnalyzer : DiagnosticAnalyzer
{
    private const string DateTimeMetadataName = "System.DateTime";

    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.DateTimeUsageRecommendationRule);

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
            if (!TargetFrameworkVersionDetector.IsNet8OrGreater(compilationContext.Compilation))
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(AnalyzeIdentifierName, SyntaxKind.IdentifierName);
        });
    }

    /// <summary>
    /// 識別子名を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeIdentifierName(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not IdentifierNameSyntax identifierName)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(identifierName.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (!IsDateTimeReference(context, identifierName))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.DateTimeUsageRecommendationRule,
                identifierName.Identifier.GetLocation(),
                identifierName.Identifier.ValueText));
    }

    /// <summary>
    /// DateTime参照か判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="identifierName">識別子名です。</param>
    /// <returns>DateTime参照の場合は <see langword="true" /> を返します。</returns>
    private static bool IsDateTimeReference(SyntaxNodeAnalysisContext context, IdentifierNameSyntax identifierName)
    {
        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(identifierName, context.CancellationToken);
        ISymbol? symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();

        if (symbol is IAliasSymbol aliasSymbol)
        {
            return aliasSymbol.Target is ITypeSymbol aliasTarget
                && IsDateTimeType(context.SemanticModel.Compilation, aliasTarget);
        }

        if (symbol is ITypeSymbol typeSymbol)
        {
            return IsDateTimeType(context.SemanticModel.Compilation, typeSymbol);
        }

        TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(identifierName, context.CancellationToken);
        return IsDateTimeType(context.SemanticModel.Compilation, typeInfo.Type);
    }

    /// <summary>
    /// DateTime型か判定します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>DateTime型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsDateTimeType(Compilation compilation, ITypeSymbol? typeSymbol)
    {
        INamedTypeSymbol? dateTimeType = compilation.GetTypeByMetadataName(DateTimeMetadataName);
        return SymbolEqualityComparer.Default.Equals(typeSymbol, dateTimeType);
    }
}
