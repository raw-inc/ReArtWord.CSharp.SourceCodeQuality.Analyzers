namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW341 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW341ArgumentExceptionThrowIfStringAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW341 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW341_既存実体テストで代表ケースを確認する()
    {
        return new RAW340ArgumentGuardClauseAnalyzerTests().RAW341_NET8以上で文字列guardのArgumentException直接throwを検出する();
    }
}