using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW201SummaryDocumentationAnalyzerTests
{
    [Fact]
    public async Task RAW201_summary欠落で検出される()
    {
        const string source = """
            public sealed class Target
            {
                public Target()
                {
                }

                public void Run()
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SummaryDocumentationAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static x => x.Id == "RAW201");
    }

    [Fact]
    public async Task RAW202_英語のみsummaryで検出される()
    {
        const string source = """
            /// <summary>Target class summary.</summary>
            public sealed class Target
            {
                /// <summary>Constructor summary.</summary>
                public Target()
                {
                }

                /// <summary>Run summary.</summary>
                public void Run()
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SummaryDocumentationAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static x => x.Id == "RAW202");
    }

    [Fact]
    public async Task RAW201とRAW202_日本語summaryでは非検出()
    {
        const string source = """
            /// <summary>対象クラスの日本語説明です。</summary>
            public sealed class Target
            {
                /// <summary>コンストラクタの日本語説明です。</summary>
                public Target()
                {
                }

                /// <summary>実行処理の日本語説明です。</summary>
                public void Run()
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SummaryDocumentationAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static x => x.Id == "RAW201");
        Assert.DoesNotContain(diagnostics, static x => x.Id == "RAW202");
    }

    [Fact]
    public async Task RAW201_tests配下でもsummary欠落を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SummaryDocumentationAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static x => x.Id == "RAW201");
    }
}
