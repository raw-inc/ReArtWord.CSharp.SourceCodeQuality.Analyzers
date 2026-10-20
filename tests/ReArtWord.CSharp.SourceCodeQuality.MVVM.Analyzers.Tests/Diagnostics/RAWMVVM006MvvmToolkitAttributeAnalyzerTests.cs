using System.Diagnostics.CodeAnalysis;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAWMVVM006MvvmToolkitAttributeAnalyzerTests
{
    [Fact]
    public async Task RAWMVVM006_RelayCommand属性の文字列を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class Target
            {
                [RelayCommand(CanExecute = "CanSave")]
                private void Save()
                {
                }

                private bool CanSave()
                {
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

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM006");
    }

    [Fact]
    public async Task RAWMVVM006_RelayCommand属性の入れ子文字列を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class Target
            {
                [RelayCommand(CanExecute = "CanSave" + "Now")]
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

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM006"));
    }

    [Fact]
    public async Task RAWMVVM006_RelayCommand属性のnameofは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class Target
            {
                [RelayCommand(CanExecute = nameof(CanSave))]
                private void Save()
                {
                }

                private bool CanSave()
                {
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

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM006");
    }

    [Fact]
    public async Task RAWMVVM007_NotifyPropertyChangedFor属性の文字列を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class Target : ObservableObject
            {
                public string FullName => string.Empty;

                [ObservableProperty]
                [NotifyPropertyChangedFor("FullName")]
                public partial string Name { get; set; }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                AnalyzerTestHelper.CreateToolkitPackageFiles());

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM007");
    }

    [Fact]
    public async Task RAWMVVM007_params文字列を複数件検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class Target : ObservableObject
            {
                public string FullName => string.Empty;

                public string SortName => string.Empty;

                [ObservableProperty]
                [NotifyPropertyChangedFor("FullName", "SortName")]
                public partial string Name { get; set; }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                AnalyzerTestHelper.CreateToolkitPackageFiles());

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM007"));
    }

    [Fact]
    public async Task RAWMVVM007_NotifyPropertyChangedFor属性のnameofは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class Target : ObservableObject
            {
                public string FullName => string.Empty;

                [ObservableProperty]
                [NotifyPropertyChangedFor(nameof(FullName))]
                public partial string Name { get; set; }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                AnalyzerTestHelper.CreateToolkitPackageFiles());

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM007");
    }

    [Fact]
    public async Task RAWMVVM008_field上のObservablePropertyを検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class Target : ObservableObject
            {
                [ObservableProperty]
                private string name = string.Empty;
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                AnalyzerTestHelper.CreateToolkitPackageFiles());

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM008");
    }

    [Fact]
    public async Task RAWMVVM008_生成コードでは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class Target : ObservableObject
            {
                [ObservableProperty]
                private string name = string.Empty;
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.g.cs",
                "ReArtWord.App",
                AnalyzerTestHelper.CreateToolkitPackageFiles());

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM008");
    }

    [Fact]
    public async Task RAWMVVM008_partial_property上のObservablePropertyは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class Target : ObservableObject
            {
                [ObservableProperty]
                public partial string Name { get; set; }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                AnalyzerTestHelper.CreateToolkitPackageFiles());

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM008");
    }
}
