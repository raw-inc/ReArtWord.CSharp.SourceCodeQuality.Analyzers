namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW310 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW310SuppressJustificationAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW310 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW310_既存実体テストで代表ケースを確認する()
    {
        return new RAW301SuppressionGovernanceAnalyzerTests().RAW310_Justification不足を検出する();
    }
}