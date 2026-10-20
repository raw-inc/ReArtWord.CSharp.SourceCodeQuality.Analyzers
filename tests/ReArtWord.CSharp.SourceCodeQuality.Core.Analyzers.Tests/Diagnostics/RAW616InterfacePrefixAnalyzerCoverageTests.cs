namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW616 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW616InterfacePrefixAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW616 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW616_既存実体テストで代表ケースを確認する()
    {
        return new RAW612NamingConventionAnalyzerTests().RAW615_RAW616_RAW617_型命名違反を検出する();
    }
}