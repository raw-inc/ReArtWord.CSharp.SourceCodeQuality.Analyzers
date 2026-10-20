namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM020 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM020RelayCommandReturnTypeAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM020 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM020_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM015MvvmServiceAndRelayCommandAdditionalAnalyzerTests().RAWMVVM018から020_RelayCommand対象メソッド規約違反を検出する();
    }
}