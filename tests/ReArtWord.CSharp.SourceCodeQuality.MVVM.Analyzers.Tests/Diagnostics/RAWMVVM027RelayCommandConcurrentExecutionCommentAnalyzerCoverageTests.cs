namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAWMVVM027 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAWMVVM027RelayCommandConcurrentExecutionCommentAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAWMVVM027 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAWMVVM027_既存実体テストで代表ケースを確認する()
    {
        return new RAWMVVM015MvvmServiceAndRelayCommandAdditionalAnalyzerTests().RAWMVVM027_AllowConcurrentExecutionsの日本語コメント不足を検出する();
    }
}