namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW511 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW511FireAndForgetAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW511 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW511_既存実体テストで代表ケースを確認する()
    {
        return new RAW160AsyncContractAnalyzerTests().RAW511_discard代入のfireAndForgetを検出する();
    }
}