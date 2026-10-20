using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW612NamingConventionAnalyzerTests
{
    [Fact]
    public async Task RAW612_RAW613_RAW614_アンダースコア付き命名を検出する()
    {
        const string source = """
            public sealed class Target
            {
                private int field_value;

                public void Run_test(string input_value)
                {
                    int local_value = field_value;
                    _ = local_value;
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW612");
        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW613");
        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW614");
    }

    [Fact]
    public async Task RAW612_単独discardは検出しない()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(string value)
                {
                    _ = value;
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW612");
    }

    [Fact]
    public async Task RAW615_RAW616_RAW617_型命名違反を検出する()
    {
        const string source = """
            public enum Status
            {
                None
            }

            public interface Worker
            {
            }

            public sealed class bad_type
            {
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW615");
        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW616");
        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW617");
    }

    [Fact]
    public async Task RAW615_RAW616_RAW617_準拠名は検出しない()
    {
        const string source = """
            public enum EStatus
            {
                None
            }

            public interface IWorker
            {
            }

            public sealed class Target
            {
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW615");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW616");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW617");
    }

    private static async Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(string source)
    {
        return await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new NamingConventionAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain").ConfigureAwait(false);
    }
}
