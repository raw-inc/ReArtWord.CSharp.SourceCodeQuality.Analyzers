using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW457OperatorSeparatorSpacingAnalyzerScopeTests
{
    [Fact]
    public async Task RAW457_RAW458_RAW459_tests配下でも検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(int b)
                {
                    int a=  b;
                    Consume(1,2);
                }

                private void Consume(int a, int b)
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW457");
        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW458");
        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW459");
    }

    [Fact]
    public async Task RAW457_RAW458_RAW459_Analyzer自己テストと生成コードは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(int b)
                {
                    int a=  b;
                    Consume(1,2);
                }

                private void Consume(int a, int b)
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> selfDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/tests/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests/Target.cs",
                "ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests");
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> generatedDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new OperatorSeparatorSpacingAnalyzer(),
                "/repo/src/Target.g.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(selfDiagnostics, static diagnostic => diagnostic.Id == "RAW457");
        Assert.DoesNotContain(selfDiagnostics, static diagnostic => diagnostic.Id == "RAW458");
        Assert.DoesNotContain(selfDiagnostics, static diagnostic => diagnostic.Id == "RAW459");
        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id == "RAW457");
        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id == "RAW458");
        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id == "RAW459");
    }
}
