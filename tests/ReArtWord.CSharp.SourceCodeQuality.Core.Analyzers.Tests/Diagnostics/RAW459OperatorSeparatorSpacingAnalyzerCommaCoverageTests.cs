using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW459OperatorSeparatorSpacingAnalyzerCommaCoverageTests
{
    [Fact]
    public async Task RAW459_三項以上の単一行カンマ不足を検出する()
    {
        const string source = """
            using System.Collections.Generic;

            public sealed record TripleRecord(int A,int B,int C);

            public sealed class Target
            {
                public void Execute()
                {
                    Run(1,2,3);
                    (int first,int second,int third) tuple = (1,2,3);
                    Dictionary<int,string> map = new();
                }

                private void Run(int a,int b,int c)
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.True(diagnostics.Count(static diagnostic => diagnostic.Id == "RAW459") >= 10);
    }

    [Fact]
    public async Task RAW459_三項以上の単一行カンマが正しい場合は非検出()
    {
        const string source = """
            using System.Collections.Generic;

            public sealed record TripleRecord(int A, int B, int C);

            public sealed class Target
            {
                public void Execute()
                {
                    Run(1, 2, 3);
                    (int first, int second, int third) tuple = (1, 2, 3);
                    Dictionary<int, string> map = new();
                }

                private void Run(int a, int b, int c)
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW459");
    }

    [Fact]
    public async Task RAW459_三項以上の複数行カンマは行末と次行インデントを非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Execute()
                {
                    Run(
                        1,
                        2,
                        3);

                    (int first, int second, int third) tuple = (
                        1,
                        2,
                        3);

                    int[] values =
                    {
                        1,
                        2,
                        3,
                    };
                }

                private void Run(
                    int a,
                    int b,
                    int c)
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW459");
    }

    [Fact]
    public async Task RAW459_複数行内で同一行に続く次項目のSpace不足を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Execute()
                {
                    Run(
                        1,2,
                        3);
                }

                private void Run(int a, int b, int c)
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW459");
    }

    [Fact]
    public async Task RAW459_複数行でも各行のカンマ直前Spaceを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Execute()
                {
                    Run(
                        1 ,
                        2 ,
                        3);
                }

                private void Run(int a, int b, int c)
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.True(diagnostics.Count(static diagnostic => diagnostic.Id == "RAW459") >= 2);
    }

    [Fact]
    public async Task RAW459_カンマ直後Space過剰を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Execute()
                {
                    Run(1,  2,  3);
                }

                private void Run(int a, int b, int c)
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.True(diagnostics.Count(static diagnostic => diagnostic.Id == "RAW459") >= 2);
        Assert.True(diagnostics.Count(static diagnostic => diagnostic.Id == "RAW457") >= 2);
    }

    [Fact]
    public async Task RAW459_配列作成サイズのカンマ不足を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Execute()
                {
                    int[,,] matrix = new int[1,2,3];
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.True(diagnostics.Count(static diagnostic => diagnostic.Id == "RAW459") >= 2);
    }

    [Fact]
    public async Task RAW459_末尾カンマと配列ランク指定は非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Execute()
                {
                    int[,,] matrix = new int[1, 2, 3];
                    int[] values = { 1, 2, 3, };
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW459");
    }
}
