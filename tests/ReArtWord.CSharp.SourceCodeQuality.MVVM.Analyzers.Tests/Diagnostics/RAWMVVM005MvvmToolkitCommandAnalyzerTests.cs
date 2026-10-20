using System.Diagnostics.CodeAnalysis;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAWMVVM005MvvmToolkitCommandAnalyzerTests
{
    [Fact]
    public async Task RAWMVVM005_Command型fieldとpropertyを検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class Target
            {
                private IRelayCommand? saveCommand;

                public ICommand? OpenCommand { get; private set; }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                AnalyzerTestHelper.CreateToolkitPackageFiles());

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM005"));
    }

    [Fact]
    public async Task RAWMVVM005_RelayCommand手動生成を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class Target
            {
                public void Initialize()
                {
                    object command = new RelayCommand(Save);
                }

                private static void Save()
                {
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

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM005");
    }

    [Fact]
    public async Task RAWMVVM005_targetTypedRelayCommand手動生成を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class Target
            {
                public void Initialize()
                {
                    RelayCommand command = new(Save);
                }

                private static void Save()
                {
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

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM005");
    }

    [Fact]
    public async Task RAWMVVM005_Command型メンバー初期化子内のnewはメンバー診断へ集約する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class Target
            {
                public IRelayCommand SaveCommand { get; } = new RelayCommand(Save);

                private static void Save()
                {
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

        Assert.Single(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM005");
    }

    [Fact]
    public async Task RAWMVVM005_RelayCommand属性と型非該当Command名は検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class Target
            {
                public string SaveCommand { get; } = string.Empty;

                [RelayCommand]
                private void Save()
                {
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

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM005");
    }
}
