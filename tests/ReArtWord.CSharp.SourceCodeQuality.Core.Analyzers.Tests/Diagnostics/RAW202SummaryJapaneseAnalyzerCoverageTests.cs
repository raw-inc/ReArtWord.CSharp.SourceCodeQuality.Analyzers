namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW202 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW202SummaryJapaneseAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW202 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW202_既存実体テストで代表ケースを確認する()
    {
        return new RAW201SummaryDocumentationAnalyzerTests().RAW202_英語のみsummaryで検出される();
    }
}