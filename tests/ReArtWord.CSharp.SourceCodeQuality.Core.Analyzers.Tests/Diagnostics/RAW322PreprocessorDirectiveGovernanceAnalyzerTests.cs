using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW322PreprocessorDirectiveGovernanceAnalyzerTests
{
    [Theory]
    [InlineData("#if FALSE")]
    [InlineData("#if (FALSE)")]
    [InlineData("#if false")]
    [InlineData("#if (false)")]
    public async Task RAW322_FALSE系条件のTODO_FIXME不足を検出する(string directive)
    {
        string source = CreateSource(directive);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PreprocessorDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW322");
    }

    [Theory]
    [InlineData("TODO")]
    [InlineData("FIXME")]
    public async Task RAW322_直前TODO_FIXMEコメントは非検出(string directiveToken)
    {
        string source = CreateSource($"""
                    // {directiveToken}: 無効化理由が解消したら削除する
            #if FALSE
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PreprocessorDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW322");
    }

    [Fact]
    public async Task RAW322_FYIコメントは検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    // FYI: 判断材料を残す
            #if FALSE
                    System.Console.WriteLine("disabled");
            #endif
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PreprocessorDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW322");
    }

    [Fact]
    public async Task RAW322_コメントとディレクティブ間の空行を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    // TODO: 無効化理由が解消したら削除する

            #if FALSE
                    System.Console.WriteLine("disabled");
            #endif
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PreprocessorDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW322");
    }

    [Fact]
    public async Task RAW322_DEBUGや複合条件やelifFalseは対象外()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
            #if DEBUG
                    System.Console.WriteLine("debug");
            #endif

            #if FALSE || DEBUG
                    System.Console.WriteLine("combined");
            #endif

            #if DEBUG
                    System.Console.WriteLine("debug");
            #elif false
                    System.Console.WriteLine("disabled");
            #endif
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PreprocessorDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW322");
    }

    [Fact]
    public async Task RAW322_FALSEシンボル定義時は全環境無効として扱わない()
    {
        string source = CreateSource("#if FALSE");

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PreprocessorDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["FALSE"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW322");
    }

    [Fact]
    public async Task RAW322_既存コメント空行ルールと違反しない良い例()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    int value = 1;

                    // TODO: 無効化理由が解消したら削除する
            #if FALSE
                    value = 2;
            #endif
                    System.Console.WriteLine(value);
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> preprocessorDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PreprocessorDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> directiveDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> spacingDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> blankLineDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ExecutableBlankLineAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(preprocessorDiagnostics, static diagnostic => diagnostic.Id == "RAW322");
        Assert.DoesNotContain(directiveDiagnostics, static diagnostic => diagnostic.Id == "RAW320");
        Assert.DoesNotContain(spacingDiagnostics, static diagnostic => diagnostic.Id is "RAW401" or "RAW402");
        Assert.DoesNotContain(blankLineDiagnostics, static diagnostic => diagnostic.Id == "RAW431");
    }

    private static string CreateSource(string directive)
    {
        const string template = """
            public sealed class Target
            {
                public void Run()
                {
            __DIRECTIVE__
                    System.Console.WriteLine("disabled");
            #endif
                }
            }
            """;

        return template.Replace("__DIRECTIVE__", directive, StringComparison.Ordinal);
    }
}
