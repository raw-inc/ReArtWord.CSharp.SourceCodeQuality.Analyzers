namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW510 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW510AsyncCancellationTokenAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW510 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW510_既存実体テストで代表ケースを確認する()
    {
        return new RAW160AsyncContractAnalyzerTests().RAW510_CancellationTokenなしを検出する();
    }
}