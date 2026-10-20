using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// アンダースコアなしの命名規則と、明示的な有効化を確認します。
/// </summary>
public sealed class RAW120APrivateFieldNamingAnalyzerTests
{
    /// <summary>
    /// 選択した形式だけを private フィールドへ要求します。
    /// </summary>
    /// <param name="name">フィールド名です。</param>
    /// <param name="valid">規則に適合するかです。</param>
    /// <returns>単独の命名規則として成立することの確認です。</returns>
    [Theory]
    [InlineData("count", true)]
    [InlineData("countValue", true)]
    [InlineData("count2", true)]
    [InlineData("_count", false)]
    [InlineData("Count", false)]
    [InlineData("count_value", false)]
    public async Task RAW120A_一般規則と併用しても専用規則で一度だけ診断する(string name, bool valid)
    {
        string source = $"class Target {{ private int {name}; }}";
        ImmutableArray<Diagnostic> diagnostics = await FieldNamingTestHelper.AnalyzeAsync(
            source,
            new Dictionary<string, ReportDiagnostic> { ["RAW120A"] = ReportDiagnostic.Error }).ConfigureAwait(true);
        if (valid)
        {
            Assert.Empty(diagnostics);
            return;
        }

        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("RAW120A", diagnostic.Id);
        Assert.Equal(name, source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }

    /// <summary>
    /// 設定していない選択式の規則は診断せず、一般規則だけを適用します。
    /// </summary>
    /// <param name="name">フィールド名です。</param>
    /// <param name="generalRuleExpected">一般規則の診断が必要かです。</param>
    /// <returns>利用先の設定ファイルに依存しない既定動作の確認です。</returns>
    [Theory]
    [InlineData("count", false)]
    [InlineData("Count", false)]
    [InlineData("_count", true)]
    public async Task RAW120A_RAW120B_既定では無効である(string name, bool generalRuleExpected)
    {
        ImmutableArray<Diagnostic> diagnostics = await FieldNamingTestHelper.AnalyzeAsync(
            $"class Target {{ private int {name}; }}").ConfigureAwait(true);
        if (generalRuleExpected)
        {
            Assert.Equal("RAW612", Assert.Single(diagnostics).Id);
            return;
        }

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// 選択式の規則を有効にしてもテストコードと生成コードは対象外です。
    /// </summary>
    /// <param name="path">ソースの配置先です。</param>
    /// <returns>既存の適用範囲を維持する確認です。</returns>
    [Theory]
    [InlineData("/repo/tests/Target.cs")]
    [InlineData("/repo/obj/Target.cs")]
    [InlineData("/repo/src/Target.g.cs")]
    public async Task RAW120A_RAW120B_対象外のソースを検査しない(string path)
    {
        Assert.Empty(await FieldNamingTestHelper.AnalyzeAsync(
            "class Target { private int Count; }",
            new Dictionary<string, ReportDiagnostic> { ["RAW120A"] = ReportDiagnostic.Error, ["RAW120B"] = ReportDiagnostic.Error },
            path).ConfigureAwait(true));
    }
}
