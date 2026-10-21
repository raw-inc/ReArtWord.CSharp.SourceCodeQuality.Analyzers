using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW111DataTypeToStringAnalyzerTests
{
    [Theory]
    [InlineData("class", 13)]
    [InlineData("sealed class", 20)]
    [InlineData("struct", 14)]
    public async Task RAW111_データ型でToString未実装を検出する(string typeDeclaration, int typeNameColumn)
    {
        string source = $$"""
            public {{typeDeclaration}} UserData
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

        Microsoft.CodeAnalysis.Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("RAW111", diagnostic.Id);
        Assert.Equal(Microsoft.CodeAnalysis.DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(0, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
        Assert.Equal(typeNameColumn, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
    }

    [Theory]
    [InlineData("public const string DefaultName = \"sample\";")]
    [InlineData("public static readonly string DefaultName = \"sample\";")]
    [InlineData("public static string DefaultName { get; } = \"sample\";")]
    public async Task RAW111_データメンバーだけを持つstaticクラスは対象外(string memberDeclaration)
    {
        string source = $$"""
            public static class SettingNames
            {
                {{memberDeclaration}}
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DataTypeToStringAnalyzer(),
                "/repo/src/SettingNames.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RAW111_static修飾子が別ファイルにあるpartialクラスは対象外()
    {
        Dictionary<string, string> sources = new()
        {
            ["/repo/src/SettingNames.cs"] = """
                public static partial class SettingNames
                {
                }
                """,
            ["/repo/src/SettingNames.Values.cs"] = """
                public partial class SettingNames
                {
                    public const string DefaultName = "sample";
                }
                """,
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                sources,
                new DataTypeToStringAnalyzer(),
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Empty(diagnostics);
    }

    [Fact]
    public async Task RAW111_staticクラス内の非staticデータ型は検出する()
    {
        const string source = """
            public static class SettingNames
            {
                public const string DefaultName = "sample";

                public sealed class UserData
                {
                    public int Id { get; set; }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DataTypeToStringAnalyzer(),
                "/repo/src/SettingNames.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Microsoft.CodeAnalysis.Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("RAW111", diagnostic.Id);
        Assert.Equal(Microsoft.CodeAnalysis.DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal(4, diagnostic.Location.GetLineSpan().StartLinePosition.Line);
        Assert.Equal(24, diagnostic.Location.GetLineSpan().StartLinePosition.Character);
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
