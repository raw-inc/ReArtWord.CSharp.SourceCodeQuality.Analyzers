namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW450 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW450ElseIfChainSwitchAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW450 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW450_既存実体テストで代表ケースを確認する()
    {
        return new RAW440ControlFlowStyleAnalyzerTests().RAW450_elseIf3連鎖以上を検出する();
    }
}