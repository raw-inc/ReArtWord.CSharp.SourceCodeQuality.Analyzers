using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW203ReturnDocumentationAnalyzerTests
{
    [Fact]
    public async Task RAW203_戻り値のあるメソッドでreturns欠落を検出する()
    {
        const string source = """
            public sealed class Target
            {
                /// <summary>値を取得します。</summary>
                public int GetValue()
                {
                    return 1;
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW203");
    }

    [Fact]
    public async Task RAW203_Task戻り値でもreturns欠落を検出する()
    {
        const string source = """
            using System.Threading.Tasks;

            public sealed class Target
            {
                /// <summary>値を取得します。</summary>
                public Task<int> GetValueAsync()
                {
                    return Task.FromResult(1);
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW203");
    }

    [Fact]
    public async Task RAW203_returnsがあれば検出しない()
    {
        const string source = """
            public sealed class Target
            {
                /// <summary>値を取得します。</summary>
                /// <returns>値です。</returns>
                public int GetValue()
                {
                    return 1;
                }

                /// <summary>実行します。</summary>
                public void Run()
                {
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW203");
    }

    [Fact]
    public async Task RAW203_生成コードと自己テストは除外する()
    {
        const string source = """
            public sealed class Target
            {
                /// <summary>値を取得します。</summary>
                public int GetValue()
                {
                    return 1;
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> generatedDiagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ReturnDocumentationAnalyzer(),
            "/repo/src/Target.g.cs",
            "ReArtWord.SourceCodeQuality.Domain");
        IReadOnlyCollection<Diagnostic> selfTestDiagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ReturnDocumentationAnalyzer(),
            "/repo/tests/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests/Target.cs",
            "ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests");

        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id == "RAW203");
        Assert.DoesNotContain(selfTestDiagnostics, static diagnostic => diagnostic.Id == "RAW203");
    }

    private static async Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(string source)
    {
        return await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ReturnDocumentationAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain").ConfigureAwait(false);
    }
}
