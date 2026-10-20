namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM026 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM026ViewModelPublicPropertyNotificationAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM026 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM026_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM009MvvmViewModelConventionAnalyzerTests().RAWMVVM026_ViewModelの通知なしpublic可変プロパティを検出する();
    }
}