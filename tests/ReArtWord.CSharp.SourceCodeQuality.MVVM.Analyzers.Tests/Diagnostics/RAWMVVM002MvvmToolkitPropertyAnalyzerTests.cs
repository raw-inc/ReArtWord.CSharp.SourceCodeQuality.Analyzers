using System.Diagnostics.CodeAnalysis;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAWMVVM002MvvmToolkitPropertyAnalyzerTests
{
    [Fact]
    public async Task RAWMVVM002とRAWMVVM003_SetPropertyを同じ位置へ検出する()
    {
        const string markedTarget = """
            public sealed class Target : ObservableObject
            {
                private string name = string.Empty;

                public string Name
                {
                    get => name;
                    set => /*診断位置*/SetProperty(ref name, value);
                }
            }
            """;
        string markedSource = MvvmAnalyzerTestSources.WithToolkitStubs(markedTarget);
        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                AnalyzerTestHelper.CreateToolkitPackageFiles());

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAWMVVM002", source, expectedStart, "SetProperty");
        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAWMVVM003", source, expectedStart, "SetProperty");
    }

    [Fact]
    public async Task RAWMVVM002とRAWMVVM003_無関係なSetPropertyは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class Target
            {
                private string name = string.Empty;

                public string Name
                {
                    get => name;
                    set => SetProperty(ref name, value);
                }

                private static bool SetProperty(ref string field, string value)
                {
                    field = value;
                    return true;
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                AnalyzerTestHelper.CreateToolkitPackageFiles());

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM002");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM003");
    }

    [Fact]
    public async Task RAWMVVM004_OnPropertyChangedを検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class Target : ObservableObject
            {
                public void Refresh()
                {
                    OnPropertyChanged(nameof(Refresh));
                    OnPropertyChanged("FullName");
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                AnalyzerTestHelper.CreateToolkitPackageFiles());

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM004"));
    }

    [Fact]
    public async Task RAWMVVM004_生成コードとAnalyzer自己テストは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class Target : ObservableObject
            {
                public void Refresh()
                {
                    OnPropertyChanged(nameof(Refresh));
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> generatedDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.g.cs",
                "ReArtWord.App",
                AnalyzerTestHelper.CreateToolkitPackageFiles());
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> selfDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/tests/ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests/Target.cs",
                "ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests",
                AnalyzerTestHelper.CreateToolkitPackageFiles());

        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id == "RAWMVVM004");
        Assert.DoesNotContain(selfDiagnostics, static diagnostic => diagnostic.Id == "RAWMVVM004");
    }
}
