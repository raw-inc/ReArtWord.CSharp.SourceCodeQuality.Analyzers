using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW431ExecutableBlankLineAnalyzerTests
{
    [Fact]
    public async Task RAW431_通常メソッド内の連続空行を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    int first = 1;


                    int second = 2;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ExecutableBlankLineAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic =>
            diagnostic.Id == "RAW431"
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("行間を潰す", StringComparison.Ordinal)
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("適切な粒度で1行", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RAW431_実行ブロック全般の連続空行を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public Target()
                {
                    int first = 1;


                    int second = 2;
                }

                public int Value
                {
                    get
                    {
                        int first = 1;


                        return first;
                    }
                }

                public void Run()
                {
                    void Local()
                    {
                        int first = 1;


                        int second = 2;
                    }

                    System.Action action = () =>
                    {
                        int first = 1;


                        int second = 2;
                    };

                    Local();
                    action();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ExecutableBlankLineAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Equal(4, diagnostics.Count(static diagnostic => diagnostic.Id == "RAW431"));
    }

    [Fact]
    public async Task RAW431_1空行は非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    int first = 1;

                    int second = 2;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ExecutableBlankLineAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW431");
    }

    [Fact]
    public async Task RAW431_3行以上の連続空行は範囲ごとに1回検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    int first = 1;



                    int second = 2;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ExecutableBlankLineAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Single(diagnostics, static diagnostic => diagnostic.Id == "RAW431");
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    public async Task RAW431_改行コード差分でも連続空行を検出する(string newline)
    {
        const string template = """
            public sealed class Target
            {
                public void Run()
                {
                    int first = 1;


                    int second = 2;
                }
            }
            """;

        string source = template.Replace("\n", newline, StringComparison.Ordinal);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ExecutableBlankLineAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW431");
    }

    [Fact]
    public async Task RAW431_tests配下でも検出しAnalyzer自己テストと生成コードは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    int first = 1;


                    int second = 2;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> testDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ExecutableBlankLineAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> selfDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ExecutableBlankLineAnalyzer(),
                "/repo/tests/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests/Target.cs",
                "ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests");

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> generatedDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ExecutableBlankLineAnalyzer(),
                "/repo/src/Target.g.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(testDiagnostics, static diagnostic => diagnostic.Id == "RAW431");
        Assert.DoesNotContain(selfDiagnostics, static diagnostic => diagnostic.Id == "RAW431");
        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id == "RAW431");
    }
}
