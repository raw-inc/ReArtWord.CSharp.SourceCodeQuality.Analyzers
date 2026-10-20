namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW611 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW611UnsafeAsyncMethodSuffixAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW611 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW611_既存実体テストで代表ケースを確認する()
    {
        return new RAW601MethodNamingSuffixAnalyzerTests().RAW611_asyncかつunsafe対象でUnsafeAsync接尾辞なしを検出する();
    }
}