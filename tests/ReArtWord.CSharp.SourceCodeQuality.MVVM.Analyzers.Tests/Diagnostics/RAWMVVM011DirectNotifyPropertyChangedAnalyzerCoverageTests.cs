namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM011 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM011DirectNotifyPropertyChangedAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM011 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM011_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM009MvvmViewModelConventionAnalyzerTests().RAWMVVM011_INotifyPropertyChanged直接実装を検出する();
    }
}