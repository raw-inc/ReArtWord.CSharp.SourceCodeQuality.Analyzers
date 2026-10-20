using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// 専用規則と一般命名規則を同時に適用して、利用側の診断結果を確認します。
/// </summary>
internal static class FieldNamingTestHelper
{
    /// <summary>
    /// 一つのソースへ命名規則とコンパイラー設定を適用します。
    /// </summary>
    /// <param name="source">解析対象です。</param>
    /// <param name="rules">コンパイル全体の診断設定です。</param>
    /// <param name="path">ソースの配置先です。</param>
    /// <returns>利用側へ報告される診断です。</returns>
    public static Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        string source,
        IReadOnlyDictionary<string, ReportDiagnostic>? rules = null,
        string path = "/repo/src/Target.cs")
    {
        return AnalyzeAsync(
            new Dictionary<string, string> { [path] = source },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithSpecificDiagnosticOptions(rules?.ToImmutableDictionary() ?? ImmutableDictionary<string, ReportDiagnostic>.Empty));
    }

    /// <summary>
    /// 複数ファイルへ同時に命名規則を適用します。
    /// </summary>
    /// <param name="sources">パスとソースの組です。</param>
    /// <param name="options">コンパイラーが使用する診断設定です。</param>
    /// <returns>利用側へ報告される診断です。</returns>
    public static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(
        IReadOnlyDictionary<string, string> sources,
        CSharpCompilationOptions options)
    {
        CSharpCompilation compilation = CSharpCompilation.Create(
            "Application",
            sources.Select(pair => CSharpSyntaxTree.ParseText(pair.Value, path: pair.Key)),
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            options);
        Assert.Empty(compilation.GetDiagnostics().Where(diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));
        return await compilation.WithAnalyzers([new PrivateFieldNamingAnalyzer(), new NamingConventionAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
    }
}
