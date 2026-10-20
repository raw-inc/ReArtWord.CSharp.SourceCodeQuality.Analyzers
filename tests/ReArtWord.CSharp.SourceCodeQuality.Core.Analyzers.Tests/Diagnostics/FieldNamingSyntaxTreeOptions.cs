using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// コンパイラーが使用するファイル別・全体の診断設定をテストへ渡します。
/// </summary>
internal sealed class FieldNamingSyntaxTreeOptions(
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, ReportDiagnostic>> files,
    IReadOnlyDictionary<string, ReportDiagnostic> global) : SyntaxTreeOptionsProvider
{
    /// <summary>
    /// テストでは生成コードの指定を追加しません。
    /// </summary>
    /// <param name="tree">構文木です。</param>
    /// <param name="cancellationToken">取消トークンです。</param>
    /// <returns>生成コードか未指定の状態です。</returns>
    public override GeneratedKind IsGenerated(SyntaxTree tree, CancellationToken cancellationToken)
    {
        return GeneratedKind.Unknown;
    }

    /// <summary>
    /// ファイル別の診断設定を取得します。
    /// </summary>
    /// <param name="tree">対象ファイルです。</param>
    /// <param name="diagnosticId">診断IDです。</param>
    /// <param name="cancellationToken">取消トークンです。</param>
    /// <param name="severity">設定した重大度です。</param>
    /// <returns>設定があれば true を返します。</returns>
    public override bool TryGetDiagnosticValue(
        SyntaxTree tree, string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
    {
        severity = ReportDiagnostic.Default;
        return files.TryGetValue(tree.FilePath, out IReadOnlyDictionary<string, ReportDiagnostic>? rules)
            && rules.TryGetValue(diagnosticId, out severity);
    }

    /// <summary>
    /// 全体に適用する診断設定を取得します。
    /// </summary>
    /// <param name="diagnosticId">診断IDです。</param>
    /// <param name="cancellationToken">取消トークンです。</param>
    /// <param name="severity">設定した重大度です。</param>
    /// <returns>設定があれば true を返します。</returns>
    public override bool TryGetGlobalDiagnosticValue(
        string diagnosticId, CancellationToken cancellationToken, out ReportDiagnostic severity)
    {
        return global.TryGetValue(diagnosticId, out severity);
    }
}
