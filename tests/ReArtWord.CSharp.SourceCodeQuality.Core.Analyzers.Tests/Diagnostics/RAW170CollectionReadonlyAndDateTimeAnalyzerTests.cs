using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW170CollectionReadonlyAndDateTimeAnalyzerTests
{
    [Fact]
    public async Task RAW170_コレクションフィールドとsetプロパティを検出する()
    {
        const string source = """
            using System.Collections.Generic;

            public sealed class Target
            {
                private List<int> values = new List<int>();

                public IReadOnlyList<int> Items { get; set; } = new List<int>();
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new CollectionReadonlyAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain");

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAW170"));
    }

    [Fact]
    public async Task RAW170_readonlyとinitは検出しない()
    {
        const string source = """
            using System.Collections.Generic;

            public sealed class Target
            {
                private readonly List<int> values = new List<int>();

                public IReadOnlyList<int> Items { get; init; } = new List<int>();

                public IReadOnlyList<int> Values => values;
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new CollectionReadonlyAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW170");
    }

    [Fact]
    public async Task RAW171_NET8以上でDateTime利用を検出する()
    {
        const string source = """
            using System;
            using System.Collections.Generic;

            public sealed class Target
            {
                public List<DateTime> Values { get; } = new List<DateTime>();

                public DateTime GetNow()
                {
                    return DateTime.Now;
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new DateTimeUsageAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain",
            preprocessorSymbols: ["NET8_0_OR_GREATER"]);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW171");
    }

    [Fact]
    public async Task RAW171_NET6とDateTimeOffsetは検出しない()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public DateTimeOffset GetNow()
                {
                    return DateTimeOffset.Now;
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new DateTimeUsageAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain",
            preprocessorSymbols: ["NET6_0"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW171");
    }
}
