namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM004 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM004NotifyPropertyChangedForRequiredAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM004 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM004_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM002MvvmToolkitPropertyAnalyzerTests().RAWMVVM004_OnPropertyChangedを検出する();
    }
}