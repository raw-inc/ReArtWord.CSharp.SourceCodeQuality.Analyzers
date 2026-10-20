using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW410ConstAndAwaitAnalyzerTests
{
    [Fact]
    public async Task RAW410_フィールドconstを検出する()
    {
        const string source = """
            public sealed class Target
            {
                private const int MaxCount = 10;
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConstAndAwaitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic =>
            diagnostic.Id == "RAW410"
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("readonly", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RAW410_ローカルconstを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    const int value = 1;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConstAndAwaitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic =>
            diagnostic.Id == "RAW410"
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("通常のローカル変数", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RAW410_tests配下では検出しない()
    {
        const string source = """
            public sealed class Target
            {
                private const int MaxCount = 10;
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConstAndAwaitAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW410");
    }

    [Fact]
    public async Task RAW411_awaitTaskで検出する()
    {
        const string source = """
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async Task RunAsync()
                {
                    await Task.Delay(1);
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConstAndAwaitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW411");
    }

    [Fact]
    public async Task RAW411_awaitValueTaskで検出する()
    {
        const string source = """
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async Task RunAsync()
                {
                    ValueTask valueTask = ValueTask.CompletedTask;
                    await valueTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConstAndAwaitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW411");
    }

    [Fact]
    public async Task RAW411_ConfigureAwait指定ありは検出しない()
    {
        const string source = """
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async Task RunAsync()
                {
                    await Task.Delay(1).ConfigureAwait(false);

                    ValueTask valueTask = ValueTask.CompletedTask;
                    await valueTask.ConfigureAwait(true);
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConstAndAwaitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW411");
    }

    [Fact]
    public async Task RAW411_tests配下では検出しない()
    {
        const string source = """
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async Task RunAsync()
                {
                    await Task.Delay(1);
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConstAndAwaitAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW411");
    }
}
