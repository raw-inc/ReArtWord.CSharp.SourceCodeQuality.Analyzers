namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW613 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW613MethodUnderscoreNamingAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW613 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW613_既存実体テストで代表ケースを確認する()
    {
        return new RAW612NamingConventionAnalyzerTests().RAW612_RAW613_RAW614_アンダースコア付き命名を検出する();
    }
}