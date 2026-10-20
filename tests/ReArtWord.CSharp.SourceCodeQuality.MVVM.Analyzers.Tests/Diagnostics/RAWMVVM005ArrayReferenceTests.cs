using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>名前空間を持たない型の解析でも例外を発生させません。</summary>
public sealed class RAWMVVM005ArrayReferenceTests
{
    /// <summary>配列と型引数を解析し、通常のCommand診断だけを維持します。</summary>
    /// <returns>意味解析の検証完了です。</returns>
    [Fact]
    public async Task RAWMVVM005_配列と型引数で解析例外を発生させない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class Target<T, ICommand>
            {
                private readonly string[] languages = new string[0];
                private int[,] matrix = new int[0, 0];
                private T? item;
                private ICommand? unrelatedTypeParameter;
                public IRelayCommand[] Commands { get; } = new IRelayCommand[0];
                private IRelayCommand? actualCommand;
            }
            """);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source, new MvvmToolkitUsageAnalyzer(), "/repo/src/ReArtWord.App/Target.cs", "ReArtWord.App",
            AnalyzerTestHelper.CreateToolkitPackageFiles()).ConfigureAwait(true);
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "AD0001");
        Assert.Single(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM005");
    }
}
