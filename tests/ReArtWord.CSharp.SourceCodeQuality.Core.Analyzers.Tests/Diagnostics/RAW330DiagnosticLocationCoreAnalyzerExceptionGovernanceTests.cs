using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed partial class RAW130DiagnosticLocationCoreAnalyzerTests
{
    [Fact]
    public async Task RAW330_throwキーワードへ報告する()
    {
        const string markedSource = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    try
                    {
                    }
                    catch (InvalidOperationException)
                    {
                        /*診断位置*/throw;
                    }
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics = await AnalyzeExceptionAsync(source);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW330", source, expectedStart, "throw");
    }

    [Fact]
    public async Task RAW331_Exception型参照へ報告する()
    {
        const string markedSource = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    throw new /*診断位置*/Exception("invalid");
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics = await AnalyzeExceptionAsync(source);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW331", source, expectedStart, "Exception");
    }

    [Fact]
    public async Task RAW332_catch変数識別子へ報告する()
    {
        const string markedSource = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    try
                    {
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw /*診断位置*/ex;
                    }
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics = await AnalyzeExceptionAsync(source);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW332", source, expectedStart, "ex");
    }

    [Fact]
    public async Task RAW333_throwキーワードへ報告する()
    {
        const string markedSource = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    try
                    {
                    }
                    catch (InvalidOperationException)
                    {
                        /*診断位置*/throw new InvalidOperationException("wrapped");
                    }
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics = await AnalyzeExceptionAsync(source);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW333", source, expectedStart, "throw");
    }

    [Fact]
    public async Task RAW334_disableキーワードへ報告する()
    {
        const string markedSource = """
            #pragma warning /*診断位置*/disable CA2007
            public sealed class Target
            {
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new WarningSuppressionDirectiveAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW334", source, expectedStart, "disable");
    }

    private static async Task<IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic>> AnalyzeExceptionAsync(string source)
    {
        return await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ExceptionHandlingGovernanceAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain").ConfigureAwait(false);
    }
}
