using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW460NullAndStringComparisonAnalyzerTests
{
    [Fact]
    public async Task RAW460_nullの等価比較を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(string? value)
                {
                    return value == null || null != value;
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAW460"));
    }

    [Fact]
    public async Task RAW460_isNullは検出しない()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(string? value)
                {
                    return value is null || value is not null;
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW460");
    }

    [Fact]
    public async Task RAW461_文字列演算子比較と比較方法なしAPIを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(string left, string right)
                {
                    return left == right
                        || left.Equals(right)
                        || string.Compare(left, right) == 0
                        || left.Contains("A");
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Equal(4, diagnostics.Count(static diagnostic => diagnostic.Id == "RAW461"));
    }

    [Fact]
    public async Task RAW461_比較方法明示済みなら検出しない()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public bool Run(string left, string right)
                {
                    return string.Equals(left, right, StringComparison.Ordinal)
                        || StringComparer.Ordinal.Equals(left, right)
                        || left.Contains("A", StringComparison.Ordinal);
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW461");
    }

    private static async Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(string source)
    {
        return await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new NullAndStringComparisonAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain").ConfigureAwait(false);
    }
}
