using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// アンダースコア付きの命名規則と一般命名規則の併用を確認します。
/// </summary>
public sealed class RAW120BPrivateFieldNamingAnalyzerTests
{
    /// <summary>
    /// 有効な専用規則だけで private フィールドを検査します。
    /// </summary>
    /// <param name="name">フィールド名です。</param>
    /// <param name="valid">規則に適合するかです。</param>
    /// <returns>診断と修正対象の確認です。</returns>
    [Theory]
    [InlineData("_count", true)]
    [InlineData("_countValue", true)]
    [InlineData("_count2", true)]
    [InlineData("count", false)]
    [InlineData("Count", false)]
    [InlineData("_Count", false)]
    [InlineData("__count", false)]
    [InlineData("_count_value", false)]
    [InlineData("_1count", false)]
    [InlineData("_", false)]
    public async Task RAW120B_一般規則と併用しても相反する修正を求めない(string name, bool valid)
    {
        string source = $"class Target {{ private int {name}; }}";
        ImmutableArray<Diagnostic> diagnostics = await FieldNamingTestHelper.AnalyzeAsync(
            source,
            new Dictionary<string, ReportDiagnostic> { ["RAW120B"] = ReportDiagnostic.Error }).ConfigureAwait(true);
        if (valid)
        {
            Assert.Empty(diagnostics);
            return;
        }

        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("RAW120B", diagnostic.Id);
        Assert.Equal(name, source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }

    /// <summary>
    /// private の省略や static・readonly の指定でも同じ規則を使います。
    /// </summary>
    /// <param name="declaration">フィールド宣言です。</param>
    /// <returns>アクセス範囲を意味情報で判定する確認です。</returns>
    [Theory]
    [InlineData("int _count;")]
    [InlineData("private static int _count;")]
    [InlineData("private readonly int _count;")]
    [InlineData("private int @_count;")]
    public async Task RAW120B_暗黙のprivateと修飾子付きフィールドを許可する(string declaration)
    {
        Assert.Empty(await FieldNamingTestHelper.AnalyzeAsync(
            $"class Target {{ {declaration} }}",
            new Dictionary<string, ReportDiagnostic> { ["RAW120B"] = ReportDiagnostic.Error }).ConfigureAwait(true));
    }

    /// <summary>
    /// 専用規則の対象外となる変数では一般規則を維持します。
    /// </summary>
    /// <param name="member">対象外の宣言を含むメンバーです。</param>
    /// <returns>一般規則の適用範囲を狭め過ぎないことの確認です。</returns>
    [Theory]
    [InlineData("public int _count;")]
    [InlineData("internal int _count;")]
    [InlineData("protected int _count;")]
    [InlineData("private const int _count = 1;")]
    [InlineData("void Run() { int _count = 1; }")]
    public async Task RAW120B_対象外の変数ではRAW612を維持する(string member)
    {
        Diagnostic diagnostic = Assert.Single(await FieldNamingTestHelper.AnalyzeAsync(
            $"class Target {{ {member} }}",
            new Dictionary<string, ReportDiagnostic> { ["RAW120B"] = ReportDiagnostic.Error }).ConfigureAwait(true));
        Assert.Equal("RAW612", diagnostic.Id);
    }
}
