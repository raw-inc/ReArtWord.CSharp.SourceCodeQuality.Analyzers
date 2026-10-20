using System.Diagnostics.CodeAnalysis;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAWMVVM013MvvmObservablePropertyAdditionalAnalyzerTests
{
    [Fact]
    public async Task RAWMVVM013_ObservableProperty対象fieldのprivate以外を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                [ObservableProperty]
                public string name = string.Empty;
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM013");
    }

    [Fact]
    public async Task RAWMVVM013_privateFieldは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                [ObservableProperty]
                private string name = string.Empty;
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM013");
    }

    [Fact]
    public async Task RAWMVVM014_ObservableProperty対象fieldのstaticとreadonlyを検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                [ObservableProperty]
                private static readonly string name = string.Empty;
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM014"));
    }

    [Fact]
    public async Task RAWMVVM014_通常のインスタンス可変fieldは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                [ObservableProperty]
                private string name = string.Empty;
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM014");
    }

    [Fact]
    public async Task RAWMVVM025_IEnumerableとIReadOnlyListのObservablePropertyを警告する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                [ObservableProperty]
                private IEnumerable<string> names = [];

                [ObservableProperty]
                private partial IReadOnlyList<string> Items { get; set; }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM025"));
    }

    [Fact]
    public async Task RAWMVVM025_List実装型も列挙型として警告する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                [ObservableProperty]
                private List<string> names = [];
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM025");
    }

    [Fact]
    public async Task RAWMVVM025_ObservableCollection系は検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                [ObservableProperty]
                private ObservableCollection<string> names = [];

                [ObservableProperty]
                private ReadOnlyObservableCollection<string>? readonlyNames;
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM025");
    }

    private static async Task<IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic>> AnalyzeAsync(string source)
    {
        return await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new MvvmToolkitUsageAnalyzer(),
            "/repo/src/ReArtWord.App/Target.cs",
            "ReArtWord.App",
            AnalyzerTestHelper.CreateToolkitPackageFiles());
    }
}
