using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers.Tests.Diagnostics;

/// <summary>
/// 実際の C# コンパイルへ Razor Analyzer を適用するテスト補助です。
/// </summary>
internal static class AnalyzerTestHelper
{
    /// <summary>
    /// 一つのソースから診断を取得します。
    /// </summary>
    /// <param name="source">解析対象の C# ソースです。</param>
    /// <param name="filePath">ソースのパスです。</param>
    /// <param name="cancellationToken">テストの取消トークンです。</param>
    /// <returns>Analyzer が報告した診断です。</returns>
    public static Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        string source,
        string filePath,
        CancellationToken cancellationToken = default)
    {
        return AnalyzeAsync(new Dictionary<string, string> { [filePath] = source }, cancellationToken);
    }

    /// <summary>
    /// 複数のソースを同じコンパイルで解析します。
    /// </summary>
    /// <param name="sources">パスと C# ソースの組です。</param>
    /// <param name="cancellationToken">テストの取消トークンです。</param>
    /// <returns>Analyzer が報告した診断です。</returns>
    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        IReadOnlyDictionary<string, string> sources,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(sources);
        CSharpParseOptions options = new(LanguageVersion.Preview);
        IEnumerable<SyntaxTree> trees = sources.Select(pair =>
            CSharpSyntaxTree.ParseText(pair.Value, options, pair.Key, cancellationToken: cancellationToken));
        CSharpCompilation compilation = CSharpCompilation.Create(
            "RazorApplication",
            trees,
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, nullableContextOptions: NullableContextOptions.Enable));
        Assert.Empty(compilation.GetDiagnostics(cancellationToken).Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        return await compilation.WithAnalyzers([new RazorComponentAnalyzer()])
            .GetAnalyzerDiagnosticsAsync(cancellationToken).ConfigureAwait(false);
    }
}
