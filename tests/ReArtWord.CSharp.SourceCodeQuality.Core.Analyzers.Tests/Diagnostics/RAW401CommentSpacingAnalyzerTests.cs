using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW401CommentSpacingAnalyzerTests
{
    [Fact]
    public async Task RAW401_namespace直後のsummary空行不足を検出する()
    {
        const string source = """
            namespace Pattern1;
            /// <summary>説明です。</summary>
            public sealed class Target
            {
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static x => x.Id == "RAW401");
    }

    [Fact]
    public async Task RAW401_通常コメントの空行不足を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    var a = true;
                    // コメント
                    var b = true;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static x => x.Id == "RAW401");
    }

    [Fact]
    public async Task RAW402_インデント増加直後の空行を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {

                    // コメント
                    var a = true;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static x => x.Id == "RAW402");
    }

    [Fact]
    public async Task RAW401_空行2行でも検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    var a = true;


                    // コメント
                    var b = true;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static x => x.Id == "RAW401");
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    public async Task RAW401とRAW402_良い例は改行コード差分でも非検出(string newline)
    {
        const string template = """
            namespace Pattern1;

            /// <summary>
            /// これはコメントです
            /// </summary>
            public sealed class Target
            {
                /// <summary>
                /// これはコメントです
                /// </summary>
                public void Method()
                {
                    /*
                     * これはコメントです
                     */
                    var a = true;

                    // これはコメントです
                    var b = true;
                }
            }
            """;

        string source = template.Replace("\n", newline, StringComparison.Ordinal);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static x => x.Id == "RAW401");
        Assert.DoesNotContain(diagnostics, static x => x.Id == "RAW402");
    }

    [Fact]
    public async Task RAW401_tests配下でも通常コメント空行不足を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    int a = 1;
                    // コメント
                    int b = 2;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentSpacingAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static x => x.Id == "RAW401");
    }
}
