namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW332 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW332CapturedExceptionThrowAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW332 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW332_既存実体テストで代表ケースを確認する()
    {
        return new RAW330ExceptionHandlingGovernanceAnalyzerTests().RAW332_catch変数の再スローを検出する();
    }
}