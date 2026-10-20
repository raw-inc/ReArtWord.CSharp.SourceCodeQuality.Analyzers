namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW443 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW443ForInfiniteLoopAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW443 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW443_既存実体テストで代表ケースを確認する()
    {
        return new RAW440ControlFlowStyleAnalyzerTests().RAW443_for条件省略を検出する();
    }
}