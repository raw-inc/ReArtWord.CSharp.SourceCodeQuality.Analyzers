namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW444 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW444ConstantFalseConditionAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW444 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW444_既存実体テストで代表ケースを確認する()
    {
        return new RAW440ControlFlowStyleAnalyzerTests().RAW444_ifFalseを検出する();
    }
}