namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM025 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM025ObservablePropertyCollectionAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM025 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM025_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM013MvvmObservablePropertyAdditionalAnalyzerTests().RAWMVVM025_IEnumerableとIReadOnlyListのObservablePropertyを警告する();
    }
}