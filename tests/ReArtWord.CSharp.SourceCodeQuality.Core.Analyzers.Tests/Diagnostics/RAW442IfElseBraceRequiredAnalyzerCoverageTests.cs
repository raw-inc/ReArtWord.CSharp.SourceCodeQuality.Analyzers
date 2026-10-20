namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW442 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW442IfElseBraceRequiredAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW442 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW442_既存実体テストで代表ケースを確認する()
    {
        return new RAW440ControlFlowStyleAnalyzerTests().RAW442_ifElseの単文本体を検出する();
    }
}