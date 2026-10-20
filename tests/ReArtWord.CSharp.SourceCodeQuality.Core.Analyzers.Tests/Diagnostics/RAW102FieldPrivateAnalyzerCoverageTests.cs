namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW102 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW102FieldPrivateAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW102 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW102_既存実体テストで代表ケースを確認する()
    {
        return new RAW501ModernSynchronizationAndFieldAnalyzerTests().RAW102_NET6以上でpublicフィールドを検出する();
    }
}