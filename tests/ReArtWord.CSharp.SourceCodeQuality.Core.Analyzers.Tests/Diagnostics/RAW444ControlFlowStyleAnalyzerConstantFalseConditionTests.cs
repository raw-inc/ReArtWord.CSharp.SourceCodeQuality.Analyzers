using System.Globalization;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

public sealed partial class RAW440ControlFlowStyleAnalyzerTests
{
    [Fact]
    public async Task RAW444_ifFalseを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    if (false)
                    {
                        System.Console.WriteLine("disabled");
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

        Assert.Contains(diagnostics, static diagnostic =>
            diagnostic.Id == "RAW444"
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("#if FALSE", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RAW444_elseIfFalseを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(int value)
                {
                    if (value == 0)
                    {
                        System.Console.WriteLine(value);
                    }
                    else if (false)
                    {
                        System.Console.WriteLine("disabled");
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

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW444");
    }

    [Fact]
    public async Task RAW444_whileFalseを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    while (false)
                    {
                        System.Console.WriteLine("disabled");
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

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW444");
    }

    [Fact]
    public async Task RAW444_constFalse条件を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    const bool disabled = false;
                    if (disabled)
                    {
                        System.Console.WriteLine("if");
                    }

                    while (disabled)
                    {
                        System.Console.WriteLine("while");
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

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAW444"));
    }

    [Fact]
    public async Task RAW444_実行時bool条件は非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(bool condition)
                {
                    bool disabled = false;
                    if (disabled)
                    {
                        condition = true;
                    }

                    while (condition)
                    {
                        condition = false;
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

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW444");
    }

    [Fact]
    public async Task RAW444_doWhileFalseとforFalseは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    do
                    {
                        System.Console.WriteLine("once");
                    }
                    while (false);

                    for (; false; )
                    {
                        System.Console.WriteLine("disabled");
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

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW444");
    }

    [Fact]
    public async Task RAW444_tests配下でも検出しAnalyzer自己テストと生成コードは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    if (false)
                    {
                        System.Console.WriteLine("disabled");
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> testDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

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

        Assert.Contains(testDiagnostics, static diagnostic => diagnostic.Id == "RAW444");
        Assert.DoesNotContain(selfDiagnostics, static diagnostic => diagnostic.Id == "RAW444");
        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id == "RAW444");
    }
}
