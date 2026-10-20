namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM008 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM008ObservablePropertyPartialPropertyAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM008 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM008_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM006MvvmToolkitAttributeAnalyzerTests().RAWMVVM008_field上のObservablePropertyを検出する();
    }
}