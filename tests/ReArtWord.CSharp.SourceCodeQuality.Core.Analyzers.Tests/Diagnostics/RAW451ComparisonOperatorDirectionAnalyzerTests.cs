using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW451ComparisonOperatorDirectionAnalyzerTests
{
    [Fact]
    public async Task RAW451_大なり演算子を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(int left, int right)
                {
                    return left > right;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Diagnostic diagnostic = Assert.Single(diagnostics, static diagnostic => diagnostic.Id == "RAW451");
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
    }

    [Fact]
    public async Task RAW451_大なりイコール演算子を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(int left, int right)
                {
                    return left >= right;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW451");
    }

    [Fact]
    public async Task RAW451_許可演算子は非検出()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(int left, int right)
                {
                    return left < right
                        || left <= right
                        || left == right
                        || left != right;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW451");
    }

    [Fact]
    public async Task RAW451_tests配下でも検出する()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(int left, int right)
                {
                    return left > right;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW451");
    }

    [Fact]
    public async Task RAW451_Analyzer自己テストは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(int left, int right)
                {
                    return left > right;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/tests/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests/Target.cs",
                "ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW451");
    }

    [Fact]
    public async Task RAW451_関係パターンは対象外()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(int value)
                {
                    return value is > 0 or >= 10;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW451");
    }
}
