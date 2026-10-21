using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// if、else if、elseの本体に対する波括弧の検査を検証します。
/// </summary>
// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW442IfElseBraceRequiredAnalyzerCoverageTests
{
    /// <summary>
    /// 波括弧付きのelse ifを、本番コードとテストコードの両方で許可します。
    /// </summary>
    /// <param name="filePath">解析対象のファイルパスです。</param>
    /// <param name="assemblyName">解析対象のアセンブリ名です。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("/repo/src/Grade.cs", "ReArtWord.SourceCodeQuality.Domain")]
    [InlineData("/repo/tests/GradeTests.cs", "ReArtWord.SourceCodeQuality.Domain.Tests")]
    public async Task RAW442_elseIfの本体に波括弧があれば非検出(string filePath, string assemblyName)
    {
        const string source = """
            public static class Grade
            {
                public static int Rank(int score)
                {
                    if (90 <= score)
                    {
                        return 1;
                    }
                    else if (60 <= score)
                    {
                        return 2;
                    }

                    return 3;
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ControlFlowStyleAnalyzer(),
            filePath,
            assemblyName);

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// else ifに続くelseも含め、各本体に波括弧があれば許可します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public async Task RAW442_elseIfと末尾のelseの本体に波括弧があれば非検出()
    {
        const string source = """
            public static class Grade
            {
                public static int Rank(int score)
                {
                    if (90 <= score)
                    {
                        return 1;
                    }
                    else if (60 <= score)
                    {
                        return 2;
                    }
                    else
                    {
                        return 3;
                    }
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ControlFlowStyleAnalyzer(),
            "/repo/src/Grade.cs",
            "ReArtWord.SourceCodeQuality.Domain");

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// 波括弧を省略した本体だけに、エラーを1件報告します。
    /// </summary>
    /// <param name="statement">診断位置を指定したif文です。</param>
    /// <param name="expectedText">診断対象となる本体です。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("if (first) /*診断位置*/return 1;", "return 1;")]
    [InlineData("if (first) { return 0; } else /*診断位置*/return 1;", "return 1;")]
    [InlineData("if (first) { return 0; } else if (second) /*診断位置*/return 1;", "return 1;")]
    [InlineData("if (first) { return 0; } else if (second) /*診断位置*/return 1; else { return 2; }", "return 1;")]
    [InlineData("if (first) { return 0; } else if (second) { return 2; } else /*診断位置*/return 1;", "return 1;")]
    [InlineData("if (first) /*診断位置*/if (second) { return 1; }", "if (second) { return 1; }")]
    public async Task RAW442_波括弧を省略した本体へ一度だけ報告する(string statement, string expectedText)
    {
        ArgumentNullException.ThrowIfNull(expectedText);

        string markedSource = $$"""
            public sealed class Target
            {
                public int Run(bool first, bool second)
                {
                    {{statement}}

                    return -1;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ControlFlowStyleAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain");

        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("RAW442", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW442", source, expectedStart, expectedText);
    }
}
