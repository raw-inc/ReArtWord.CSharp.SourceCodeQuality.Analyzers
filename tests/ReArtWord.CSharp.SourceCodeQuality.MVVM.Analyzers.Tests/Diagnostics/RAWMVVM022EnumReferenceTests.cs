using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>画面名に似た列挙型の参照と実View参照を区別します。</summary>
public sealed class RAWMVVM022EnumReferenceTests
{
    /// <summary>列挙値、戻り値型、配列と条件式をView参照と誤認しません。</summary>
    /// <returns>意味解析による検証完了です。</returns>
    [Theory]
    [InlineData("Models")]
    [InlineData("Controls")]
    public async Task RAWMVVM022_画面接尾辞と画面名前空間の列挙型を検出しない(string modelNamespace)
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs($$"""
            namespace {{modelNamespace}}
            {
                public enum EWizardPage { Welcome, Confirm }
            }
            public sealed class InstallerViewModel : ObservableObject
            {
                public {{modelNamespace}}.EWizardPage Page => {{modelNamespace}}.EWizardPage.Welcome;
                public bool IsWelcome => Page == {{modelNamespace}}.EWizardPage.Welcome;
                public {{modelNamespace}}.EWizardPage[] Route() => new[] { {{modelNamespace}}.EWizardPage.Welcome, {{modelNamespace}}.EWizardPage.Confirm };
            }
            """);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source, new MvvmToolkitUsageAnalyzer(), "/repo/src/ReArtWord.App/Target.cs", "ReArtWord.App",
            AnalyzerTestHelper.CreateToolkitPackageFiles()).ConfigureAwait(true);
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM022");
    }

    /// <summary>列挙型と同じ接尾辞を持つViewクラスは引き続き検出します。</summary>
    /// <returns>意味解析による検証完了です。</returns>
    [Fact]
    public async Task RAWMVVM022_Pageクラスの検出を維持する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class SetupPage { }
            public sealed class InstallerViewModel : ObservableObject
            {
                private SetupPage? page;
            }
            """);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source, new MvvmToolkitUsageAnalyzer(), "/repo/src/ReArtWord.App/Target.cs", "ReArtWord.App",
            AnalyzerTestHelper.CreateToolkitPackageFiles()).ConfigureAwait(true);
        Assert.Single(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM022");
    }
}
