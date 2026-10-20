using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// private フィールドの命名規則について、対象と有効な設定を共有します。
/// </summary>
internal static class PrivateFieldNamingPolicy
{
    /// <summary>
    /// 専用の命名規則で扱うフィールドか判定します。
    /// </summary>
    /// <param name="field">対象フィールドです。</param>
    /// <returns>手書きの private フィールドで、定数ではない場合は true を返します。</returns>
    public static bool IsTarget(IFieldSymbol field)
    {
        return !field.IsImplicitlyDeclared
            && !field.IsConst
            && field.DeclaredAccessibility == Accessibility.Private;
    }

    /// <summary>
    /// 選択式の命名規則が対象ファイルで有効か判定します。
    /// </summary>
    /// <param name="rule">選択する命名規則です。</param>
    /// <param name="tree">対象ファイルの構文木です。</param>
    /// <param name="compilation">コンパイル設定です。</param>
    /// <param name="cancellationToken">解析の取消トークンです。</param>
    /// <returns>規則が抑制されていない場合は true を返します。</returns>
    public static bool IsEnabled(
        DiagnosticDescriptor rule,
        SyntaxTree tree,
        Compilation compilation,
        CancellationToken cancellationToken)
    {
        CompilationOptions options = compilation.Options;
        if (options.SpecificDiagnosticOptions.TryGetValue(rule.Id, out ReportDiagnostic configured)
            && configured != ReportDiagnostic.Default)
        {
            return rule.GetEffectiveSeverity(options) != ReportDiagnostic.Suppress;
        }

        SyntaxTreeOptionsProvider? provider = options.SyntaxTreeOptionsProvider;
        if (provider is not null
            && (provider.TryGetDiagnosticValue(tree, rule.Id, cancellationToken, out configured)
                || provider.TryGetGlobalDiagnosticValue(rule.Id, cancellationToken, out configured)))
        {
            if (configured == ReportDiagnostic.Default)
            {
                return rule.IsEnabledByDefault;
            }

            CompilationOptions effectiveOptions = options.WithSpecificDiagnosticOptions(
                options.SpecificDiagnosticOptions.SetItem(rule.Id, configured));
            return rule.GetEffectiveSeverity(effectiveOptions) != ReportDiagnostic.Suppress;
        }

        return rule.GetEffectiveSeverity(options) != ReportDiagnostic.Suppress;
    }
}
