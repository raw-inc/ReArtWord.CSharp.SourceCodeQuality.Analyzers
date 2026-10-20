namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW333 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW333CatchBlockThrowAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW333 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW333_既存実体テストで代表ケースを確認する()
    {
        return new RAW330ExceptionHandlingGovernanceAnalyzerTests().RAW333_catch内の式付きthrowを検出する();
    }
}