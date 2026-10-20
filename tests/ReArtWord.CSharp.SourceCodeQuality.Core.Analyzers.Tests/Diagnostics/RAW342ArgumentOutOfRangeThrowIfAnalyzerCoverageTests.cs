namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW342 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW342ArgumentOutOfRangeThrowIfAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW342 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW342_既存実体テストで代表ケースを確認する()
    {
        return new RAW340ArgumentGuardClauseAnalyzerTests().RAW342_NET9以上で範囲guardのArgumentOutOfRangeException直接throwを検出する();
    }
}