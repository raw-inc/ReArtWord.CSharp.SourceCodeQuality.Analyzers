namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM016 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM016ManualPropertyChangedInvocationAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM016 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM016_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM012MvvmNotificationAnalyzerTests().RAWMVVM016_PropertyChangedとPropertyChanging手動発火を検出する();
    }
}