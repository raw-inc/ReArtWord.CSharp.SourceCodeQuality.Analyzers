using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW456TernaryExpressionStyleAnalyzerLocationTests
{
    [Fact]
    public async Task RAW456_単一行違反は疑問符へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public bool Run(bool condition)
                {
                    return condition /*診断位置*/? true : false;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW456", source, expectedStart, "?");
    }

    [Fact]
    public async Task RAW456_条件式改行違反は疑問符へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public bool Run(bool condition)
                {
                    return condition
                        /*診断位置*/? true
                        : false;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW456", source, expectedStart, "?");
    }

    [Fact]
    public async Task RAW456_真側式改行違反は疑問符へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public bool Run(bool condition)
                {
                    return condition /*診断位置*/?
                        true
                        : false;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW456", source, expectedStart, "?");
    }

    [Fact]
    public async Task RAW456_整列違反はコロンへ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public bool Run(bool condition)
                {
                    return condition
                        ? true
                       /*診断位置*/: false;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW456", source, expectedStart, ":");
    }
}
