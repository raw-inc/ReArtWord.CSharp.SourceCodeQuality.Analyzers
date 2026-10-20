using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// Analyzerテスト用のRoslyn実行ヘルパーです。
/// </summary>
internal static class AnalyzerTestHelper
{
    /// <summary>
    /// 文字列ソースを解析し、Analyzer診断を取得します。
    /// </summary>
    /// <param name="source">解析対象ソースです。</param>
    /// <param name="analyzer">実行するAnalyzerです。</param>
    /// <param name="filePath">ソースファイルパスです。</param>
    /// <param name="assemblyName">アセンブリ名です。</param>
    /// <param name="additionalFiles">追加ファイルです。</param>
    /// <returns>Analyzer診断一覧です。</returns>
    public static async Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(
        string source,
        DiagnosticAnalyzer analyzer,
        string filePath,
        string assemblyName,
        IReadOnlyDictionary<string, string>? additionalFiles = null)
    {
        return await AnalyzeAsync(
            new Dictionary<string, string>
            {
                [filePath] = source,
            },
            analyzer,
            assemblyName,
            additionalFiles).ConfigureAwait(false);
    }

    /// <summary>
    /// 複数ソースを解析し、Analyzer診断を取得します。
    /// </summary>
    /// <param name="sources">解析対象ソースです。</param>
    /// <param name="analyzer">実行するAnalyzerです。</param>
    /// <param name="assemblyName">アセンブリ名です。</param>
    /// <param name="additionalFiles">追加ファイルです。</param>
    /// <returns>Analyzer診断一覧です。</returns>
    public static async Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(
        IReadOnlyDictionary<string, string> sources,
        DiagnosticAnalyzer analyzer,
        string assemblyName,
        IReadOnlyDictionary<string, string>? additionalFiles = null)
    {
        if (sources.Count == 0)
        {
            throw new ArgumentException("解析対象ソースが1件以上必要です。", nameof(sources));
        }

        CSharpParseOptions parseOptions = new(languageVersion: LanguageVersion.Preview, documentationMode: DocumentationMode.Diagnose);
        ImmutableArray<SyntaxTree> syntaxTrees = sources
            .Select(pair => CSharpSyntaxTree.ParseText(pair.Value, parseOptions, pair.Key))
            .ToImmutableArray<SyntaxTree>();

        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            GetMetadataReferences(),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        CompilationWithAnalyzers analyzerDriver = compilation.WithAnalyzers(
            [analyzer],
            new CompilationWithAnalyzersOptions(
                CreateAnalyzerOptions(additionalFiles),
                onAnalyzerException: null,
                concurrentAnalysis: true,
                logAnalyzerExecutionTime: false));

        return await analyzerDriver.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Toolkit PackageReferenceを含むAdditionalFilesを作成します。
    /// </summary>
    /// <returns>AdditionalFilesの内容です。</returns>
    public static IReadOnlyDictionary<string, string> CreateToolkitPackageFiles()
    {
        return new Dictionary<string, string>
        {
            ["/repo/src/ReArtWord.App/ReArtWord.App.csproj"] =
                "<Project><ItemGroup><PackageReference Include=\"CommunityToolkit.Mvvm\" /></ItemGroup></Project>",
        };
    }

    /// <summary>
    /// 実行環境の参照アセンブリを取得します。
    /// </summary>
    /// <returns>参照アセンブリ一覧です。</returns>
    private static IEnumerable<MetadataReference> GetMetadataReferences()
    {
        string? platformAssemblies = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (string.IsNullOrWhiteSpace(platformAssemblies))
        {
            throw new InvalidOperationException("プラットフォーム参照アセンブリの取得に失敗しました。");
        }

        return platformAssemblies
            .Split(Path.PathSeparator)
            .Select(path => MetadataReference.CreateFromFile(path));
    }

    /// <summary>
    /// AnalyzerOptionsを作成します。
    /// </summary>
    /// <param name="additionalFiles">追加ファイルです。</param>
    /// <returns>AnalyzerOptionsです。</returns>
    private static AnalyzerOptions CreateAnalyzerOptions(IReadOnlyDictionary<string, string>? additionalFiles)
    {
        if (additionalFiles is null || additionalFiles.Count == 0)
        {
            return new AnalyzerOptions([]);
        }

        ImmutableArray<AdditionalText> files = additionalFiles
            .Select(pair => (AdditionalText)new InMemoryAdditionalText(pair.Key, pair.Value))
            .ToImmutableArray();

        return new AnalyzerOptions(files);
    }

    /// <summary>
    /// メモリ上のAdditionalTextです。
    /// </summary>
    private sealed class InMemoryAdditionalText(string path, string content)
        : AdditionalText
    {
        /// <summary>
        /// ファイルパスを取得します。
        /// </summary>
        public override string Path { get; } = path;

        /// <summary>
        /// テキストを取得します。
        /// </summary>
        /// <param name="cancellationToken">キャンセル制御です。</param>
        /// <returns>ソーステキストです。</returns>
        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            return SourceText.From(content, Encoding.UTF8);
        }
    }
}
