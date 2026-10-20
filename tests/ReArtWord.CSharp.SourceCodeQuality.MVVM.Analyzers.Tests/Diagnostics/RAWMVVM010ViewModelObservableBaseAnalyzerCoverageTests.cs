namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM010 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM010ViewModelObservableBaseAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM010 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM010_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM009MvvmViewModelConventionAnalyzerTests().RAWMVVM010_ViewModel命名クラスへObservable継承を要求する();
    }
}