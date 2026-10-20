using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW160AsyncContractAnalyzerLocationTests
{
    [Fact]
    public async Task RAW160_戻り値型へ報告する()
    {
        const string markedSource = """
            using System.Collections.Generic;
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async /*診断位置*/Task<List<int>> LoadAsync(CancellationToken cancellationToken = default)
                {
                    await Task.Delay(1, cancellationToken);
                    return new List<int>();
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW160", source, expectedStart, "Task<List<int>>");
    }

    [Fact]
    public async Task RAW510_パラメータなしは閉じ括弧位置へ報告する()
    {
        const string markedSource = """
            using System.Threading.Tasks;

            public sealed class Target
            {
                public Task RunAsync(/*診断位置*/)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW510", source, expectedStart, string.Empty);
    }

    [Fact]
    public async Task RAW510_CancellationTokenが最終引数でない場合は該当パラメータへ報告する()
    {
        const string markedSource = """
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public Task RunAsync(/*診断位置*/CancellationToken cancellationToken = default, int value = 0)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(
            diagnostics,
            "RAW510",
            source,
            expectedStart,
            "CancellationToken cancellationToken = default");
    }

    [Fact]
    public async Task RAW510_defaultなしは該当パラメータへ報告する()
    {
        const string markedSource = """
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public Task RunAsync(/*診断位置*/CancellationToken cancellationToken)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(
            diagnostics,
            "RAW510",
            source,
            expectedStart,
            "CancellationToken cancellationToken");
    }

    [Fact]
    public async Task RAW511_discard代入は右辺Task生成式へ報告する()
    {
        const string markedSource = """
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public Task RunAsync(CancellationToken cancellationToken = default)
                {
                    _ = /*診断位置*/Task.Run(static () => Task.CompletedTask);
                    return Task.CompletedTask;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(
            diagnostics,
            "RAW511",
            source,
            expectedStart,
            "Task.Run(static () => Task.CompletedTask)");
    }
}
