namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW402 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW402CommentBlankLineAfterIndentAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW402 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW402_既存実体テストで代表ケースを確認する()
    {
        return new RAW401CommentSpacingAnalyzerTests().RAW402_インデント増加直後の空行を検出する();
    }
}