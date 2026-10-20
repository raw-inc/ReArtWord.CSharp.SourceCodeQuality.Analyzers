namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW461 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW461StringComparisonCultureAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW461 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW461_既存実体テストで代表ケースを確認する()
    {
        return new RAW460NullAndStringComparisonAnalyzerTests().RAW461_文字列演算子比較と比較方法なしAPIを検出する();
    }
}