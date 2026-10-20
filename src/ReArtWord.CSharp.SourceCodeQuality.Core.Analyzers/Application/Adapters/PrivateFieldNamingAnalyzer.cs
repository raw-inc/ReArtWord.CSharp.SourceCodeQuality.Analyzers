using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// privateフィールド命名規約を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PrivateFieldNamingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.PrivateFieldNamingWithUnderscoreRule,
            DiagnosticCatalog.PrivateFieldNamingWithoutUnderscoreRule,
            DiagnosticCatalog.PrivateFieldNamingConflictRule);

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
        context.RegisterSymbolAction(AnalyzeField, SymbolKind.Field);
    }

    /// <summary>
    /// フィールド命名を解析します。
    /// </summary>
    /// <param name="context">シンボル解析コンテキストです。</param>
    private static void AnalyzeField(SymbolAnalysisContext context)
    {
        if (context.Symbol is not IFieldSymbol fieldSymbol)
        {
            return;
        }

        if (!PrivateFieldNamingPolicy.IsTarget(fieldSymbol))
        {
            return;
        }

        Location? reportLocation = GetReportLocation(fieldSymbol, context);
        if (reportLocation?.SourceTree is not SyntaxTree syntaxTree)
        {
            return;
        }

        bool withUnderscore = PrivateFieldNamingPolicy.IsEnabled(
            DiagnosticCatalog.PrivateFieldNamingWithUnderscoreRule, syntaxTree, context.Compilation, context.CancellationToken);
        bool withoutUnderscore = PrivateFieldNamingPolicy.IsEnabled(
            DiagnosticCatalog.PrivateFieldNamingWithoutUnderscoreRule, syntaxTree, context.Compilation, context.CancellationToken);
        if (withUnderscore && withoutUnderscore)
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticCatalog.PrivateFieldNamingConflictRule, reportLocation));
            return;
        }

        string fieldName = fieldSymbol.Name;
        if (withUnderscore && !IsLowerCamelCaseWithLeadingUnderscore(fieldName))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.PrivateFieldNamingWithUnderscoreRule,
                    reportLocation,
                    fieldName));
        }

        if (withoutUnderscore && !IsLowerCamelCaseWithoutLeadingUnderscore(fieldName))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.PrivateFieldNamingWithoutUnderscoreRule,
                    reportLocation,
                    fieldName));
        }
    }

    /// <summary>
    /// 診断報告位置を取得します。
    /// </summary>
    /// <param name="fieldSymbol">対象フィールドです。</param>
    /// <param name="context">シンボル解析コンテキストです。</param>
    /// <returns>報告可能な位置です。取得できない場合は <see langword="null" /> を返します。</returns>
    private static Location? GetReportLocation(IFieldSymbol fieldSymbol, SymbolAnalysisContext context)
    {
        foreach (Location location in fieldSymbol.Locations)
        {
            if (!location.IsInSource || location.SourceTree is null)
            {
                continue;
            }

            if (!ProductionCodeFilter.IsProductionCode(location.SourceTree, context.Compilation))
            {
                continue;
            }

            return location;
        }

        return null;
    }

    /// <summary>
    /// 先頭アンダースコア付きlowerCamelCaseか判定します。
    /// </summary>
    /// <param name="name">判定対象名です。</param>
    /// <returns>形式に一致する場合は <see langword="true" /> を返します。</returns>
    private static bool IsLowerCamelCaseWithLeadingUnderscore(string name)
    {
        if (name.Length < 2 || name[0] != '_' || !char.IsLower(name[1]))
        {
            return false;
        }

        for (int index = 2; index < name.Length; index++)
        {
            if (!char.IsLetterOrDigit(name[index]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// アンダースコア無しlowerCamelCaseか判定します。
    /// </summary>
    /// <param name="name">判定対象名です。</param>
    /// <returns>形式に一致する場合は <see langword="true" /> を返します。</returns>
    private static bool IsLowerCamelCaseWithoutLeadingUnderscore(string name)
    {
        if (name.Length == 0 || name[0] == '_' || !char.IsLower(name[0]))
        {
            return false;
        }

        for (int index = 1; index < name.Length; index++)
        {
            if (!char.IsLetterOrDigit(name[index]))
            {
                return false;
            }
        }

        return true;
    }
}
