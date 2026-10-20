using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW150FileDeclarationCompositionAnalyzerTests
{
    [Fact]
    public async Task RAW150_同一ファイルにclass2件で検出する()
    {
        const string source = """
            public sealed class First
            {
            }

            public sealed class Second
            {
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new FileDeclarationCompositionAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW150");
    }

    [Fact]
    public async Task RAW150_同一ファイルにclassとstructで検出する()
    {
        const string source = """
            public sealed class First
            {
            }

            public struct Second
            {
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new FileDeclarationCompositionAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW150");
    }

    [Fact]
    public async Task RAW150_同一ファイルにclass1件のみは非検出()
    {
        const string source = """
            public sealed class Target
            {
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new FileDeclarationCompositionAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW150");
    }

    [Fact]
    public async Task RAW151_recordOnlyでrecord2件を検出する()
    {
        const string source = """
            public record First(int Value);
            public record Second(int Value);
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new FileDeclarationCompositionAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW151");
    }

    [Fact]
    public async Task RAW151_recordOnlyでrecord1件は非検出()
    {
        const string source = """
            public record Target(int Value);
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new FileDeclarationCompositionAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW151");
    }

    [Fact]
    public async Task RAW152_mixedFileのrecordを外部参照した場合に検出する()
    {
        IReadOnlyDictionary<string, string> sources = new Dictionary<string, string>
        {
            ["/repo/src/Mixed.cs"] = """
                public sealed class Holder
                {
                    public SharedRecord Create() => new SharedRecord(1);
                }

                public record SharedRecord(int Value);
                """,
            ["/repo/src/Consumer.cs"] = """
                public sealed class Consumer
                {
                    public SharedRecord Create() => new SharedRecord(2);
                }
                """,
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                sources,
                new FileDeclarationCompositionAnalyzer(),
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW152");
    }

    [Fact]
    public async Task RAW152_mixedFile内のみ参照は非検出()
    {
        const string source = """
            public sealed class Holder
            {
                public SharedRecord Create() => new SharedRecord(1);
            }

            public record SharedRecord(int Value);
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new FileDeclarationCompositionAnalyzer(),
                "/repo/src/Mixed.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW152");
    }

    [Fact]
    public async Task RAW152_record単独ファイルは外部参照しても非検出()
    {
        IReadOnlyDictionary<string, string> sources = new Dictionary<string, string>
        {
            ["/repo/src/Record.cs"] = """
                public record SharedRecord(int Value);
                """,
            ["/repo/src/Consumer.cs"] = """
                public sealed class Consumer
                {
                    public SharedRecord Create() => new SharedRecord(2);
                }
                """,
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                sources,
                new FileDeclarationCompositionAnalyzer(),
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW152");
    }

    [Fact]
    public async Task RAW150_RAW151_RAW152_tests配下と生成コードは検出しない()
    {
        const string source = """
            public sealed class First
            {
            }

            public sealed class Second
            {
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> testDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new FileDeclarationCompositionAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> generatedDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new FileDeclarationCompositionAnalyzer(),
                "/repo/src/Target.g.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(testDiagnostics, static diagnostic => diagnostic.Id is "RAW150" or "RAW151" or "RAW152");
        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id is "RAW150" or "RAW151" or "RAW152");
    }
}
