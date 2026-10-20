using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW440ControlFlowStyleAnalyzerLocationTests
{
    [Fact]
    public async Task RAW440_whileTrue条件式へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public void Run()
                {
                    while (/*診断位置*/true)
                    {
                        break;
                    }
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW440", source, expectedStart, "true");
    }

    [Fact]
    public async Task RAW441_ループ単文本体へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public void Run(bool condition)
                {
                    while (condition) /*診断位置*/break;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW441", source, expectedStart, "break;");
    }

    [Fact]
    public async Task RAW442_if単文本体へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public void Run(bool condition)
                {
                    if (condition) /*診断位置*/condition = false;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW442", source, expectedStart, "condition = false;");
    }

    [Fact]
    public async Task RAW443_条件省略は条件スロットへ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public void Run()
                {
                    for (;/*診断位置*/; )
                    {
                        break;
                    }
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW443", source, expectedStart, string.Empty);
    }

    [Fact]
    public async Task RAW443_定数true条件は条件式へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public void Run()
                {
                    for (; /*診断位置*/true; )
                    {
                        break;
                    }
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW443", source, expectedStart, "true");
    }

    [Fact]
    public async Task RAW444_false条件は条件式へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public void Run()
                {
                    if (/*診断位置*/false)
                    {
                        System.Console.WriteLine("disabled");
                    }
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW444", source, expectedStart, "false");
    }
}
