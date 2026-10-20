using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// mixed file内recordの追跡情報です。
/// </summary>
internal sealed class MixedRecordInfo(
    INamedTypeSymbol symbol,
    Location reportLocation,
    SyntaxTree syntaxTree,
    string recordName)
{
    /// <summary>
    /// recordシンボルを取得します。
    /// </summary>
    public INamedTypeSymbol Symbol { get; } = symbol;

    /// <summary>
    /// 診断報告位置を取得します。
    /// </summary>
    public Location ReportLocation { get; } = reportLocation;

    /// <summary>
    /// 宣言元構文木を取得します。
    /// </summary>
    public SyntaxTree SyntaxTree { get; } = syntaxTree;

    /// <summary>
    /// record名を取得します。
    /// </summary>
    public string RecordName { get; } = recordName;

    /// <summary>
    /// 外部参照検出フラグを取得または設定します。
    /// </summary>
    public bool HasExternalReference { get; set; }
}
