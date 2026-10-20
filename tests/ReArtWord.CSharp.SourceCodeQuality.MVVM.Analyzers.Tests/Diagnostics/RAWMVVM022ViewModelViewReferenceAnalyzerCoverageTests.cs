namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM022 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM022ViewModelViewReferenceAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM022 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM022_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM021MvvmViewReferenceAnalyzerTests().RAWMVVM022_ViewModelからView型参照を検出する();
    }
}