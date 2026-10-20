using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW111DataTypeToStringAnalyzerTests
{
    [Fact]
    public async Task RAW111_データ型でToString未実装を検出する()
    {
        const string source = """
            public sealed class UserData
            {
                public int Id { get; set; }

                public string Name { get; set; } = string.Empty;
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DataTypeToStringAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW111");
    }

    [Fact]
    public async Task RAW111_ToString実装済みは非検出()
    {
        const string source = """
            public sealed class UserData
            {
                public int Id { get; set; }

                public string Name { get; set; } = string.Empty;

                public override string ToString()
                {
                    return $"{Id}:{Name}";
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DataTypeToStringAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW111");
    }

    [Fact]
    public async Task RAW111_業務ロジックメソッドを持つ型は対象外()
    {
        const string source = """
            public sealed class UserData
            {
                public int Id { get; set; }

                public int CalcScore()
                {
                    return Id * 10;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DataTypeToStringAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW111");
    }

    [Fact]
    public async Task RAW111_record型の既定ToStringは非検出()
    {
        const string source = """
            public record UserData(int Id, string Name);
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DataTypeToStringAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW111");
    }

    [Fact]
    public async Task RAW111_tests配下は検出しない()
    {
        const string source = """
            public sealed class UserData
            {
                public int Id { get; set; }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DataTypeToStringAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW111");
    }
}
