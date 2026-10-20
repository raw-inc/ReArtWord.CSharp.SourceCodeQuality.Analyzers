namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Configuration;

/// <summary>
/// Analyzer追加設定のプロパティ名を定義します。
/// </summary>
internal static class AnalyzerPropertyNames
{
    /// <summary>
    /// RAW101のクラス行数上限プロパティ名です。
    /// </summary>
    public const string Raw101MaxClassLineCount = "RAW101MaxClassLineCount";

    /// <summary>
    /// RAW130のpartial class宣言数上限プロパティ名です。
    /// </summary>
    public const string Raw130MaxPartialClassDeclarationCount = "RAW130MaxPartialClassDeclarationCount";

    /// <summary>
    /// RAW455の三項演算子条件数上限プロパティ名です。
    /// </summary>
    public const string Raw455TernaryConditionCountThreshold = "RAW455TernaryConditionCountThreshold";

    /// <summary>
    /// RAW301のNoWarn許可IDリストプロパティ名です。
    /// </summary>
    public const string Raw301AllowedNoWarnIds = "RAW301AllowedNoWarnIds";
}
