using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

internal static class AnalyzerTestHelper
{
    public static async Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(
        string source,
        DiagnosticAnalyzer analyzer,
        string filePath,
        string assemblyName,
        IReadOnlyDictionary<string, string>? additionalFiles = null,
        IReadOnlyCollection<string>? preprocessorSymbols = null)
    {
        return await AnalyzeAsync(
            new Dictionary<string, string>
            {
                [filePath] = source,
            },
            analyzer,
            assemblyName,
            additionalFiles,
            preprocessorSymbols).ConfigureAwait(false);
    }

    public static async Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(
        IReadOnlyDictionary<string, string> sources,
        DiagnosticAnalyzer analyzer,
        string assemblyName,
        IReadOnlyDictionary<string, string>? additionalFiles = null,
        IReadOnlyCollection<string>? preprocessorSymbols = null)
    {
        if (sources.Count == 0)
        {
            throw new ArgumentException("解析対象ソースが1件以上必要です。", nameof(sources));
        }

        CSharpParseOptions parseOptions = CreateParseOptions(preprocessorSymbols);
        ImmutableArray<SyntaxTree> syntaxTrees = sources
            .Select(pair => CSharpSyntaxTree.ParseText(pair.Value, parseOptions, pair.Key))
            .ToImmutableArray<SyntaxTree>();

        IEnumerable<MetadataReference> references = GetMetadataReferences();
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees,
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        AnalyzerOptions analyzerOptions = CreateAnalyzerOptions(additionalFiles);
        CompilationWithAnalyzers analyzerDriver = compilation.WithAnalyzers(
            [analyzer],
            new CompilationWithAnalyzersOptions(analyzerOptions, onAnalyzerException: null, concurrentAnalysis: true, logAnalyzerExecutionTime: false));

        return await analyzerDriver.GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
    }

    private static CSharpParseOptions CreateParseOptions(IReadOnlyCollection<string>? preprocessorSymbols)
    {
        if (preprocessorSymbols is null || preprocessorSymbols.Count == 0)
        {
            return new CSharpParseOptions(documentationMode: DocumentationMode.Diagnose);
        }

        return new CSharpParseOptions(
            documentationMode: DocumentationMode.Diagnose,
            preprocessorSymbols: preprocessorSymbols);
    }

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

    private sealed class InMemoryAdditionalText(string path, string content) : AdditionalText
    {
        public override string Path { get; } = path;

        public override SourceText GetText(CancellationToken cancellationToken = default)
        {
            return SourceText.From(content, Encoding.UTF8);
        }
    }
}
