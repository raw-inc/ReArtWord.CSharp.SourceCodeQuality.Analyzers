namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW141 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW141DisposableLocalUsingRequiredAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW141 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW141_既存実体テストで代表ケースを確認する()
    {
        return new RAW140DisposableUsageAnalyzerTests().RAW141_nonUsingローカル宣言を検出する();
    }
}