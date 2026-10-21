using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed partial class RAW440ControlFlowStyleAnalyzerTests
{
    [Fact]
    public async Task RAW440_whileTrueを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    while (true)
                    {
                        break;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW440");
    }

    [Fact]
    public async Task RAW441_ループ文の単文本体を検出する()
    {
        const string source = """
            using System.Collections.Generic;

            public sealed class Target
            {
                public void Run(IEnumerable<int> values)
                {
                    for (int i = 0; i < 1; i++) i++;
                    foreach (int value in values) value.ToString();
                    while (values is not null) break;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW441");
    }

    [Fact]
    public async Task RAW442_ifElseの単文本体を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(bool a, bool b)
                {
                    if (a) b = false;
                    else if (b) a = false;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW442");
    }

    [Fact]
    public async Task RAW450_elseIf3連鎖以上を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public int Convert(int value)
                {
                    if (value == 0)
                    {
                        return 0;
                    }
                    else if (value == 1)
                    {
                        return 1;
                    }
                    else if (value == 2)
                    {
                        return 2;
                    }
                    else if (value == 3)
                    {
                        return 3;
                    }

                    return -1;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Equal("RAW450", Assert.Single(diagnostics).Id);
    }

    [Fact]
    public async Task RAW450_elseIf2連鎖以下は非検出()
    {
        const string source = """
            public sealed class Target
            {
                public int Convert(int value)
                {
                    if (value == 0)
                    {
                        return 0;
                    }
                    else if (value == 1)
                    {
                        return 1;
                    }
                    else if (value == 2)
                    {
                        return 2;
                    }

                    return -1;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RAW440_019_020_波括弧ありは非検出()
    {
        const string source = """
            using System.Collections.Generic;

            public sealed class Target
            {
                public void Run(IEnumerable<int> values, bool condition)
                {
                    while (condition)
                    {
                        condition = false;
                    }

                    for (int i = 0; i < 1; i++)
                    {
                        condition = !condition;
                    }

                    foreach (int value in values)
                    {
                        condition = 0 < value;
                    }

                    if (condition)
                    {
                        condition = false;
                    }
                    else
                    {
                        condition = true;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW440");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW441");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW442");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW443");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW450");
    }

    [Fact]
    public async Task RAW441_doWhileは対象外()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    do
                        break;
                    while (false);
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW441");
    }

    [Fact]
    public async Task RAW440_019_020_tests配下でも検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(bool condition)
                {
                    while (true) break;

                    if (condition) condition = false;

                    for (int i = 0; i < 1; i++) i++;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW440");
        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW441");
        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW442");
    }

    [Fact]
    public async Task RAW450_tests配下でも検出する()
    {
        const string source = """
            public sealed class Target
            {
                public int Convert(int value)
                {
                    if (value == 0)
                    {
                        return 0;
                    }
                    else if (value == 1)
                    {
                        return 1;
                    }
                    else if (value == 2)
                    {
                        return 2;
                    }
                    else if (value == 3)
                    {
                        return 3;
                    }

                    return -1;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ControlFlowStyleAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Equal("RAW450", Assert.Single(diagnostics).Id);
    }
}
