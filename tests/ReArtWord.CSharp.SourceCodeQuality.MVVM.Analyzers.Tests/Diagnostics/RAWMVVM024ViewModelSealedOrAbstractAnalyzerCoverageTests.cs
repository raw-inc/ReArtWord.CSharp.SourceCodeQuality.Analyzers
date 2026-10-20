namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM024 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM024ViewModelSealedOrAbstractAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM024 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM024_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM009MvvmViewModelConventionAnalyzerTests().RAWMVVM009_Observable継承クラスへViewModel接尾辞を要求する();
    }
}