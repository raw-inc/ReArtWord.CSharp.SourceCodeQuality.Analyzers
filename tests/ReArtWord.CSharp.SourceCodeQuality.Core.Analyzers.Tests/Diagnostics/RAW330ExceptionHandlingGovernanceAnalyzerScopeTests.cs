using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed partial class RAW330ExceptionHandlingGovernanceAnalyzerTests
{
    private const string BareRethrowSource = """
        using System;

        public sealed class Target
        {
            public void Run()
            {
                try
                {
                    Work();
                }
                catch (InvalidOperationException)
                {
                    throw;
                }
            }

            private static void Work()
            {
            }
        }
        """;

    [Fact]
    public async Task RAW330_tests配下でも検出する()
    {
        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            BareRethrowSource,
            new ExceptionHandlingGovernanceAnalyzer(),
            "/repo/tests/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW330");
    }

    [Fact]
    public async Task RAW330_生成コードと自己テストは除外する()
    {
        IReadOnlyCollection<Diagnostic> generatedDiagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            BareRethrowSource,
            new ExceptionHandlingGovernanceAnalyzer(),
            "/repo/src/Target.g.cs",
            "ReArtWord.SourceCodeQuality.Domain");
        IReadOnlyCollection<Diagnostic> selfTestDiagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            BareRethrowSource,
            new ExceptionHandlingGovernanceAnalyzer(),
            "/repo/tests/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests/Target.cs",
            "ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests");

        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id == "RAW330");
        Assert.DoesNotContain(selfTestDiagnostics, static diagnostic => diagnostic.Id == "RAW330");
    }
}
