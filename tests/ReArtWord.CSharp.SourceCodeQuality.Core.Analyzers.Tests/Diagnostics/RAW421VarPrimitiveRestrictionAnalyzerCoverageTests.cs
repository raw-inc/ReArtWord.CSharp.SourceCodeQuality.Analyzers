namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW421 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW421VarPrimitiveRestrictionAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW421 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW421_既存実体テストで代表ケースを確認する()
    {
        return new RAW420VarUsageAnalyzerTests().RAW421_プリミティブリテラル利用は検出し_RAW420を抑止する();
    }
}