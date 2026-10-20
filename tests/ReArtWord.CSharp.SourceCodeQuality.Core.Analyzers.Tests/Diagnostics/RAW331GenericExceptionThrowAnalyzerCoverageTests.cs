namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW331 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW331GenericExceptionThrowAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW331 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW331_既存実体テストで代表ケースを確認する()
    {
        return new RAW330ExceptionHandlingGovernanceAnalyzerTests().RAW331_汎用Exception直接throwを検出する();
    }
}