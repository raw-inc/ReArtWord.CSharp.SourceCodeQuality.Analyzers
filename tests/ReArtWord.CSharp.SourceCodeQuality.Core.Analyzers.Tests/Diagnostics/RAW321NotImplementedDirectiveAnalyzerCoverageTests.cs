namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW321 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW321NotImplementedDirectiveAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW321 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW321_既存実体テストで代表ケースを確認する()
    {
        return new RAW320CommentDirectiveGovernanceAnalyzerTests().RAW321_NotImplementedExceptionにTODOコメントがない場合を検出する();
    }
}