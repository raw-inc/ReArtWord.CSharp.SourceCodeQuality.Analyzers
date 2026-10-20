using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// ファイル宣言構成の集計スナップショットです。
/// </summary>
internal sealed class FileCompositionSnapshot(
    string fileName,
    int classOrStructCount,
    int recordCount,
    Location? classStructViolationLocation,
    Location? recordOnlyViolationLocation)
{
    /// <summary>
    /// ファイル表示名を取得します。
    /// </summary>
    public string FileName { get; } = fileName;

    /// <summary>
    /// class/struct宣言数を取得します。
    /// </summary>
    public int ClassOrStructCount { get; } = classOrStructCount;

    /// <summary>
    /// record宣言数を取得します。
    /// </summary>
    public int RecordCount { get; } = recordCount;

    /// <summary>
    /// class/struct違反位置を取得します。
    /// </summary>
    public Location? ClassStructViolationLocation { get; } = classStructViolationLocation;

    /// <summary>
    /// record-only違反位置を取得します。
    /// </summary>
    public Location? RecordOnlyViolationLocation { get; } = recordOnlyViolationLocation;

    /// <summary>
    /// class/struct と record が同居しているか判定します。
    /// </summary>
    public bool IsMixedFile => ClassOrStructCount > 0 && RecordCount > 0;
}
