namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM003 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM003PartialHookRequiredAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM003 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM003_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM002MvvmToolkitPropertyAnalyzerTests().RAWMVVM002とRAWMVVM003_SetPropertyを同じ位置へ検出する();
    }
}