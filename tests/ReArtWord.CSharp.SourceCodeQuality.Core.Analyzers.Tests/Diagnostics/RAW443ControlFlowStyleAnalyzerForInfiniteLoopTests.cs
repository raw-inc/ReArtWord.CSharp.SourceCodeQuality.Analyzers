namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

public sealed partial class RAW440ControlFlowStyleAnalyzerTests
{
    [Fact]
    public async Task RAW443_for条件省略を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    for (;;)
                    {
                        break;
                    }

                    for (int i = 0; ; i++)
                    {
                        break;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAW443"));
    }

    [Fact]
    public async Task RAW443_for定数true条件を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    const bool enabled = true;
                    for (; true; )
                    {
                        break;
                    }

                    for (; enabled; )
                    {
                        break;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAW443"));
    }

    [Fact]
    public async Task RAW443_終了条件付きforとfalse定数条件は非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(bool condition)
                {
                    bool enabled = true;
                    for (int i = 0; i < 10; i++)
                    {
                        enabled = !enabled;
                    }

                    for (; condition; )
                    {
                        condition = false;
                    }

                    const bool disabled = false;
                    for (; disabled; )
                    {
                        enabled = false;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW443");
    }

    [Fact]
    public async Task RAW443_単文本体ではRAW441と併発する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    for (;;) break;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW443");
        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW441");
    }

    [Fact]
    public async Task RAW443_tests配下でも検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    for (;;)
                    {
                        break;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW443");
    }

    [Fact]
    public async Task RAW443_Analyzer自己テストと生成コードは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    for (;;)
                    {
                        break;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> selfDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/tests/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests/Target.cs",
                "ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests");

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> generatedDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.g.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(selfDiagnostics, static diagnostic => diagnostic.Id == "RAW443");
        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id == "RAW443");
    }
}
