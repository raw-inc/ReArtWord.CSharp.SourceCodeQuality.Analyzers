using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW457OperatorSeparatorSpacingAnalyzerTests
{
    [Fact]
    public async Task RAW457_代入周辺と宣言内の連続Spaceを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(int b)
                {
                    int a =  b;
                    int c   = b;
                    int  value = c;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.True(diagnostics.Count(static diagnostic => diagnostic.Id == "RAW457") >= 3);
    }

    [Fact]
    public async Task RAW457_行頭インデント改行文字列コメント内Spaceは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public string Run()
                {
                    string value = "A  B";
                    // A  B
                    return
                        value;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW457");
    }

    [Fact]
    public async Task RAW457_Tabは連続Spaceとして扱わない()
    {
        string source = """
            public sealed class Target
            {
                public void Run()
                {
                    int{tab}{tab}value = 1;
                }
            }
            """.Replace("{tab}", "\t", StringComparison.Ordinal);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW457");
    }

    [Fact]
    public async Task RAW458_代入二項ラムダ矢印のSpace不足を検出する()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public int Value=>1;

                public void Run(int b, object value)
                {
                    int a=b;
                    a+=b;
                    int sum = a+b;
                    Func<int, int> convert = x=>x + 1;
                    bool pattern = value is(null);
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.True(diagnostics.Count(static diagnostic => diagnostic.Id == "RAW458") >= 5);
    }

    [Fact]
    public async Task RAW458_演算子左右Spaceがある場合は非検出()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public int Value => 1;

                public void Run(int b, object value)
                {
                    int a = b;
                    a += b;
                    int sum = a + b;
                    Func<int, int> convert = x => x + 1;
                    bool pattern = value is null;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW458");
    }

    [Fact]
    public async Task RAW458_改行を挟む演算子は同一行側だけ検査する()
    {
        const string source = """
            public sealed class Target
            {
                public int Valid(int left, int right)
                {
                    return left +
                        right;
                }

                public int Invalid(int left, int right)
                {
                    return left+
                        right;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW458");
    }

    [Fact]
    public async Task RAW459_呼び出し宣言recordのカンマ違反を検出する()
    {
        const string source = """
            public sealed record TargetRecord(int A,int B);

            public sealed class Target
            {
                public void Execute()
                {
                    Run(1,2);
                    Run(1 , 2);
                }

                private void Run(int a,int b)
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

        Assert.True(diagnostics.Count(static diagnostic => diagnostic.Id == "RAW459") >= 4);
    }

    [Fact]
    public async Task RAW459_カンマ区切りがAカンマSpaceB形式の場合は非検出()
    {
        const string source = """
            public sealed record TargetRecord(int A, int B);

            public sealed class Target
            {
                public void Execute()
                {
                    Run(1, 2);
                    int[] values = { 1, 2, };
                }

                private void Run(int a, int b)
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
    public async Task RAW459_複数行の行末と次行先頭インデントは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Execute()
                {
                    Run(
                        1,
                        2);
                }

                private void Run(int a, int b)
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
    public async Task RAW459_複数行でもカンマ直前Spaceは検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Execute()
                {
                    Run(
                        1 ,
                        2);
                }

                private void Run(int a, int b)
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
    public async Task RAW459_文字列コメント補間アラインメントのカンマは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public string Run(int value)
                {
                    string text = "a,b";
                    string aligned = $"|{value,10}|";
                    // a,b
                    return text + aligned;
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
