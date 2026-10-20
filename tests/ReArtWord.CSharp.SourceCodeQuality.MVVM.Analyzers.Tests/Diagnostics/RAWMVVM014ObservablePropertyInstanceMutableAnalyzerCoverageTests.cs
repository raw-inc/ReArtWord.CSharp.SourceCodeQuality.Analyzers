namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM014 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM014ObservablePropertyInstanceMutableAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM014 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM014_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM013MvvmObservablePropertyAdditionalAnalyzerTests().RAWMVVM014_ObservableProperty対象fieldのstaticとreadonlyを検出する();
    }
}