namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW458 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW458OperatorSpacingAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW458 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW458_既存実体テストで代表ケースを確認する()
    {
        return new RAW457OperatorSeparatorSpacingAnalyzerTests().RAW458_代入二項ラムダ矢印のSpace不足を検出する();
    }
}