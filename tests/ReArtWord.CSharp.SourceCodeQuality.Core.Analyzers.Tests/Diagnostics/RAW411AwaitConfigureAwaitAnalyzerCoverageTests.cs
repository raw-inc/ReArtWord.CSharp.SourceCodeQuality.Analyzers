namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW411 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW411AwaitConfigureAwaitAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW411 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW411_既存実体テストで代表ケースを確認する()
    {
        return new RAW410ConstAndAwaitAnalyzerTests().RAW411_awaitTaskで検出する();
    }
}