namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW152 のID別入口を保証する委譲テストです。
/// </summary>
public sealed class RAW152MixedFileRecordExternalReferenceAnalyzerCoverageTests
{
    /// <summary>
    /// 既存の実体テストで RAW152 の代表ケースを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public Task RAW152_既存実体テストで代表ケースを確認する()
    {
        return new RAW150FileDeclarationCompositionAnalyzerTests().RAW152_mixedFileのrecordを外部参照した場合に検出する();
    }
}