namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW151 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW151RecordOnlyFileDeclarationLimitAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW151 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW151_既存実体テストで代表ケースを確認する()
    {
        return new RAW150FileDeclarationCompositionAnalyzerTests().RAW151_recordOnlyでrecord2件を検出する();
    }
}