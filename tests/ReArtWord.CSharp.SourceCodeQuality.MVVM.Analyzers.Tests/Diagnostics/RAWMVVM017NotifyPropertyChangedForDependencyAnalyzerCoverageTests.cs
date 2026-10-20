namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM017 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM017NotifyPropertyChangedForDependencyAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM017 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM017_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM012MvvmNotificationAnalyzerTests().RAWMVVM017_OnPropertyChangedのnameof依存通知を検出する();
    }
}