namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW610 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW610UnsafeMethodSuffixAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW610 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW610_既存実体テストで代表ケースを確認する()
    {
        return new RAW601MethodNamingSuffixAnalyzerTests().RAW610_unsafeメソッドでUnsafe接尾辞なしを検出する();
    }
}