namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM007 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM007NotifyPropertyChangedForNameofAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM007 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM007_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM006MvvmToolkitAttributeAnalyzerTests().RAWMVVM007_NotifyPropertyChangedFor属性の文字列を検出する();
    }
}