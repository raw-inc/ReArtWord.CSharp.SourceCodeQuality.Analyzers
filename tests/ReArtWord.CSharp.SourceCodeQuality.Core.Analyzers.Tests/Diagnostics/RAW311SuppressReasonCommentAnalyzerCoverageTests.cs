namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW311 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW311SuppressReasonCommentAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW311 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW311_既存実体テストで代表ケースを確認する()
    {
        return new RAW301SuppressionGovernanceAnalyzerTests().RAW311_抑制理由コメント不足を検出する();
    }
}