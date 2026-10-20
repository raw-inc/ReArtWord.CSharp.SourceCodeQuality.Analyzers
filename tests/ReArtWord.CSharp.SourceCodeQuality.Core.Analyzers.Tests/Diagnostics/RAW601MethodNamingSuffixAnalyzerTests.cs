using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW601MethodNamingSuffixAnalyzerTests
{
    [Fact]
    public async Task RAW601_asyncメソッドでAsync接尾辞なしを検出する()
    {
        const string source = """
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async Task Run()
                {
                    await Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MethodNamingSuffixAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW601");
    }

    [Fact]
    public async Task RAW601_asyncメソッドでAsync接尾辞ありは非検出()
    {
        const string source = """
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async Task RunAsync()
                {
                    await Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MethodNamingSuffixAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW601");
    }

    [Fact]
    public async Task RAW610_unsafeメソッドでUnsafe接尾辞なしを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public unsafe void Run()
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MethodNamingSuffixAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW610");
    }

    [Fact]
    public async Task RAW610_unsafeメソッドでUnsafe接尾辞ありは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public unsafe void RunUnsafe()
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MethodNamingSuffixAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW610");
    }

    [Fact]
    public async Task RAW610_unsafeブロック含有メソッドでUnsafe接尾辞なしを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    unsafe
                    {
                        int* pointer = null;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MethodNamingSuffixAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW610");
    }

    [Fact]
    public async Task RAW610_unsafe型内メソッドでUnsafe接尾辞なしを検出する()
    {
        const string source = """
            public unsafe class Target
            {
                public void Run()
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MethodNamingSuffixAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW610");
    }

    [Fact]
    public async Task RAW610_メッセージにSuppressMessage案内を含む()
    {
        const string source = """
            public sealed class Target
            {
                public unsafe void Run()
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MethodNamingSuffixAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic =>
            diagnostic.Id == "RAW610"
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("SuppressMessage", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RAW611_asyncかつunsafe対象でUnsafeAsync接尾辞なしを検出する()
    {
        const string source = """
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async unsafe Task RunAsync()
                {
                    await Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MethodNamingSuffixAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW611");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW601");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW610");
    }

    [Fact]
    public async Task RAW611_asyncかつunsafe対象でUnsafeAsync接尾辞ありは非検出()
    {
        const string source = """
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async unsafe Task RunUnsafeAsync()
                {
                    await Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MethodNamingSuffixAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id is "RAW601" or "RAW610" or "RAW611");
    }

    [Fact]
    public async Task RAW601_RAW610_RAW611_tests配下と生成コードは検出しない()
    {
        const string source = """
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async unsafe Task Run()
                {
                    await Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> testDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MethodNamingSuffixAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> generatedDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MethodNamingSuffixAnalyzer(),
                "/repo/src/Target.g.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(testDiagnostics, static diagnostic => diagnostic.Id is "RAW601" or "RAW610" or "RAW611");
        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id is "RAW601" or "RAW610" or "RAW611");
    }
}
