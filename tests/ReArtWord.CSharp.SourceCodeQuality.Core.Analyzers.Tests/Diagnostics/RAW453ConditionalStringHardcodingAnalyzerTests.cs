using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW453ConditionalStringHardcodingAnalyzerTests
{
    [Fact]
    public async Task RAW453_条件構文の通常文字列を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public int Run(string value)
                {
                    if (value == "A")
                    {
                        return 1;
                    }
                    else if (value != "B")
                    {
                        return 2;
                    }

                    while (value == "C")
                    {
                        break;
                    }

                    do
                    {
                        value = "D";
                    }
                    while (value == "E");

                    for (; value == "F"; )
                    {
                        break;
                    }

                    return value == "G" ? 3 : 4;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConditionalStringHardcodingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.True(diagnostics.Count(static diagnostic => diagnostic.Id == "RAW453") >= 6);
    }

    [Fact]
    public async Task RAW453_raw文字列と補間文字列を検出する()
    {
        const string source = """"
            public sealed class Target
            {
                public bool Run(string value, string suffix)
                {
                    if (value == """Alpha""")
                    {
                        return true;
                    }

                    if (value == $"{suffix}")
                    {
                        return true;
                    }

                    return false;
                }
            }
            """";

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConditionalStringHardcodingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAW453"));
    }

    [Fact]
    public async Task RAW453_戻り値と条件外二項式は非検出()
    {
        const string source = """
            public sealed class Target
            {
                private const string Expected = "A";

                public string Convert(bool condition)
                {
                    return condition ? "A" : "B";
                }

                public bool Compare(string value)
                {
                    string local = "A";
                    return value == "A" || value == local;
                }

                public bool IsExpected(string value)
                {
                    if (value == Expected || value == nameof(Target))
                    {
                        return true;
                    }

                    return false;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConditionalStringHardcodingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW453");
    }

    [Fact]
    public async Task RAW453_tests配下でも検出する()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(string value)
                {
                    return value == "A" ? true : false;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConditionalStringHardcodingAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW453");
    }

    [Fact]
    public async Task RAW453_Analyzer自己テストと生成コードは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(string value)
                {
                    return value == "A" ? true : false;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> selfDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConditionalStringHardcodingAnalyzer(),
                "/repo/tests/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests/Target.cs",
                "ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests");
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> generatedDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConditionalStringHardcodingAnalyzer(),
                "/repo/src/Target.g.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(selfDiagnostics, static diagnostic => diagnostic.Id == "RAW453");
        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id == "RAW453");
    }
}
