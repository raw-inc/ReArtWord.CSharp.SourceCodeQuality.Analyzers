using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW454WhitespaceTabAnalyzerTests
{
    [Fact]
    public async Task RAW454_行頭インデントTabを検出する()
    {
        string source = """
            public sealed class Target
            {
            {tab}public void Run()
                {
                }
            }
            """.Replace("{tab}", "\t", StringComparison.Ordinal);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new WhitespaceTabAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW454");
    }

    [Fact]
    public async Task RAW454_トークン間空白Tabを検出する()
    {
        string source = """
            public{tab}sealed class Target
            {
                public void Run()
                {
                }
            }
            """.Replace("{tab}", "\t", StringComparison.Ordinal);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new WhitespaceTabAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW454");
    }

    [Fact]
    public async Task RAW454_文字列とコメント内Tabは非検出()
    {
        string source = """
            public sealed class Target
            {
                public string Run()
                {
                    string value = "A{tab}B";
                    // A{tab}B
                    return value;
                }
            }
            """.Replace("{tab}", "\t", StringComparison.Ordinal);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new WhitespaceTabAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW454");
    }

    [Fact]
    public async Task RAW454_Spaceインデントは非検出()
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
                new WhitespaceTabAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW454");
    }

    [Fact]
    public async Task RAW454_tests配下でも検出する()
    {
        string source = """
            public{tab}sealed class Target
            {
            }
            """.Replace("{tab}", "\t", StringComparison.Ordinal);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new WhitespaceTabAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW454");
    }

    [Fact]
    public async Task RAW454_Analyzer自己テストと生成コードは非検出()
    {
        string source = """
            public{tab}sealed class Target
            {
            }
            """.Replace("{tab}", "\t", StringComparison.Ordinal);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> selfDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new WhitespaceTabAnalyzer(),
                "/repo/tests/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests/Target.cs",
                "ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests");
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> generatedDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new WhitespaceTabAnalyzer(),
                "/repo/src/Target.g.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(selfDiagnostics, static diagnostic => diagnostic.Id == "RAW454");
        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id == "RAW454");
    }
}
