using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW340ArgumentGuardClauseAnalyzerTests
{
    [Fact]
    public async Task RAW340_NET6以上でArgumentNullException直接throwを検出する()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run(string? value)
                {
                    if (value is null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source, ["NET6_0_OR_GREATER"]);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW340");
    }

    [Fact]
    public async Task RAW340_NET5では検出しない()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run(string? value)
                {
                    if (value is null)
                    {
                        throw new ArgumentNullException(nameof(value));
                    }
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source, ["NET5_0"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW340");
    }

    [Fact]
    public async Task RAW341_NET8以上で文字列guardのArgumentException直接throwを検出する()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run(string? value)
                {
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        throw new ArgumentException("値が必要です。", nameof(value));
                    }
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source, ["NET8_0_OR_GREATER"]);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW341");
    }

    [Fact]
    public async Task RAW341_NET6では文字列ThrowIf未提供として検出しない()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run(string? value)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new ArgumentException("値が必要です。", nameof(value));
                    }
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source, ["NET6_0"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW341");
    }

    [Fact]
    public async Task RAW342_NET9以上で範囲guardのArgumentOutOfRangeException直接throwを検出する()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run(int count)
                {
                    if (count <= 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(count));
                    }
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source, ["NET9_0_OR_GREATER"]);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW342");
    }

    [Fact]
    public async Task RAW342_NET6では範囲ThrowIf未提供として検出しない()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run(int count)
                {
                    if (count < 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(count));
                    }
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source, ["NET6_0"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW342");
    }

    private static async Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(
        string source,
        IReadOnlyCollection<string> preprocessorSymbols)
    {
        return await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ArgumentGuardClauseAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain",
            preprocessorSymbols: preprocessorSymbols).ConfigureAwait(false);
    }
}
