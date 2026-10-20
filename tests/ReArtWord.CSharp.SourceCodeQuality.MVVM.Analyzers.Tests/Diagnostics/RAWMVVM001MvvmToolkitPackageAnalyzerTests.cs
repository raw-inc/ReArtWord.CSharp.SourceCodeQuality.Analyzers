using System.Diagnostics.CodeAnalysis;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAWMVVM001MvvmToolkitPackageAnalyzerTests
{
    [Fact]
    public async Task RAWMVVM001_PackageReference無しを検出する()
    {
        const string source = """
            public sealed class Target
            {
            }
            """;
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/src/ReArtWord.App/ReArtWord.App.csproj"] = "<Project />",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM001");
    }

    [Fact]
    public async Task RAWMVVM001_PackageReference有りは検出しない()
    {
        const string source = """
            public sealed class Target
            {
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                AnalyzerTestHelper.CreateToolkitPackageFiles());

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM001");
    }

    [Fact]
    public async Task RAWMVVM001_属性順序と引用符差分があってもPackageReferenceを認識する()
    {
        const string source = """
            public sealed class Target
            {
            }
            """;
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/src/ReArtWord.App/ReArtWord.App.csproj"] =
                "<Project><ItemGroup><PackageReference Version='8.4.0' Include='CommunityToolkit.Mvvm' /></ItemGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                additionalFiles);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM001");
    }

    [Fact]
    public async Task RAWMVVM001_csproj以外のPackageReferenceは導入済み扱いにしない()
    {
        const string source = """
            public sealed class Target
            {
            }
            """;
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/packages.props"] =
                "<Project><ItemGroup><PackageReference Include=\"CommunityToolkit.Mvvm\" /></ItemGroup></Project>",
            ["/repo/src/ReArtWord.App/ReArtWord.App.csproj"] = "<Project />",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM001");
    }

    [Fact]
    public async Task RAWMVVM001_PackageVersion単独では導入済み扱いにしない()
    {
        const string source = """
            public sealed class Target
            {
            }
            """;
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Packages.props"] = "<Project><ItemGroup><PackageVersion Include=\"CommunityToolkit.Mvvm\" Version=\"8.4.0\" /></ItemGroup></Project>",
            ["/repo/src/ReArtWord.App/ReArtWord.App.csproj"] = "<Project />",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.cs",
                "ReArtWord.App",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM001");
    }
}
