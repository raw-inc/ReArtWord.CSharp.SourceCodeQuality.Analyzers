using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW334WarningSuppressionDirectiveAnalyzerTests
{
    [Fact]
    public async Task RAW334_pragmaWarningDisableを検出する()
    {
        const string source = """
            #pragma warning disable
            public sealed class Target
            {
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW334");
    }

    [Fact]
    public async Task RAW334_ID指定付きpragmaWarningDisableを検出する()
    {
        const string source = """
            #pragma warning disable CA2007, RAW330
            public sealed class Target
            {
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW334");
    }

    [Fact]
    public async Task RAW334_restoreとnullableとSuppressMessageは検出しない()
    {
        const string source = """
            #nullable disable
            #pragma warning restore CA2007
            using System.Diagnostics.CodeAnalysis;

            [SuppressMessage("Reliability", "CA2007", Justification = "テスト用です。")]
            public sealed class Target
            {
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW334");
    }

    private static async Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(string source)
    {
        return await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new WarningSuppressionDirectiveAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain").ConfigureAwait(false);
    }
}
