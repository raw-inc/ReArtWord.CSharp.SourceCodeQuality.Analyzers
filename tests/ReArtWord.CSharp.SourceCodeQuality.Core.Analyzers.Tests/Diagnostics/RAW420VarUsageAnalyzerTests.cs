using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW420VarUsageAnalyzerTests
{
    [Fact]
    public async Task RAW420とRAW421_許可例は非検出()
    {
        const string source = """
            using System.Collections.Generic;
            using System.Linq;

            public sealed class Target
            {
                public void Run(List<int> items)
                {
                    var a = new List<int>();
                    var b = new { Name = "John", Age = 30 };
                    var c = true;
                    var d = string.Empty;
                    var e = items.Select(x => new { Value = x });
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new VarUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static x => x.Id == "RAW420");
        Assert.DoesNotContain(diagnostics, static x => x.Id == "RAW421");
    }

    [Fact]
    public async Task RAW420_型非自明なメソッド戻り値を検出する()
    {
        const string source = """
            using System.Collections.Generic;

            public sealed class Target
            {
                public void Run()
                {
                    var a = GetList();
                    var b = HogeMethod<int>();
                }

                private static List<int> GetList() => new List<int>();

                private static List<T> HogeMethod<T>() => new List<T>();
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new VarUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static x => x.Id == "RAW420");
    }

    [Fact]
    public async Task RAW421_プリミティブリテラル利用は検出し_RAW420を抑止する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    var a = 123;
                    var b = 3.14;
                    var c = "A";
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new VarUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static x => x.Id == "RAW421");
        Assert.DoesNotContain(diagnostics, static x => x.Id == "RAW420");
    }

    [Fact]
    public async Task RAW420とRAW421_非自明プリミティブでは併発する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    var a = GetNumber();
                }

                private static int GetNumber() => 1;
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new VarUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static x => x.Id == "RAW420");
        Assert.Contains(diagnostics, static x => x.Id == "RAW421");
    }

    [Fact]
    public async Task RAW420とRAW421_for_foreach_out_varを評価する()
    {
        const string source = """
            using System.Collections.Generic;

            public sealed class Target
            {
                public void Run(IEnumerable<int> values, string text)
                {
                    for (var i = 0; i < 1; i++)
                    {
                    }

                    foreach (var value in values)
                    {
                    }

                    int.TryParse(text, out var parsed);
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new VarUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static x => x.Id == "RAW420");
        Assert.Contains(diagnostics, static x => x.Id == "RAW421");
    }

    [Fact]
    public async Task RAW421_型付き静的メンバー例外は非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    var a = string.Empty;
                    var b = int.MaxValue;
                    var c = char.MinValue;
                    var d = double.NaN;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new VarUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static x => x.Id == "RAW420");
        Assert.DoesNotContain(diagnostics, static x => x.Id == "RAW421");
    }

    [Fact]
    public async Task RAW420とRAW421_tests配下でもvar違反を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    var a = GetNumber();
                }

                private static int GetNumber() => 1;
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new VarUsageAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static x => x.Id == "RAW420");
        Assert.Contains(diagnostics, static x => x.Id == "RAW421");
    }
}
