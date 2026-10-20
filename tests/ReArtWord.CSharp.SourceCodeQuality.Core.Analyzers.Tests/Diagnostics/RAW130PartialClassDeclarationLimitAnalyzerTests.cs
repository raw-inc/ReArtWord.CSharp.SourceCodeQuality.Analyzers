using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW130PartialClassDeclarationLimitAnalyzerTests
{
    [Fact]
    public async Task RAW130_partial宣言5個で検出する()
    {
        string source = BuildPartialClassSource(5);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PartialClassDeclarationLimitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW130");
    }

    [Fact]
    public async Task RAW130_partial宣言4個は非検出()
    {
        string source = BuildPartialClassSource(4);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PartialClassDeclarationLimitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW130");
    }

    [Fact]
    public async Task RAW130_メッセージに責務分離文言を含む()
    {
        string source = BuildPartialClassSource(5);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PartialClassDeclarationLimitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic =>
            diagnostic.Id == "RAW130"
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("責務分離が適切に行われていない可能性が高いです", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RAW130_props設定で上限を変更できる()
    {
        string source = BuildPartialClassSource(4);
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW130MaxPartialClassDeclarationCount>3</RAW130MaxPartialClassDeclarationCount></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PartialClassDeclarationLimitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW130");
    }

    [Fact]
    public async Task RAW130_csproj設定はprops設定を上書きする()
    {
        string source = BuildPartialClassSource(4);
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW130MaxPartialClassDeclarationCount>10</RAW130MaxPartialClassDeclarationCount></PropertyGroup></Project>",
            ["/repo/src/ReArtWord.SourceCodeQuality.Domain/ReArtWord.SourceCodeQuality.Domain.csproj"] = "<Project><PropertyGroup><RAW130MaxPartialClassDeclarationCount>3</RAW130MaxPartialClassDeclarationCount></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PartialClassDeclarationLimitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW130");
    }

    [Fact]
    public async Task RAW130_不正設定値は既定値4へフォールバックする()
    {
        string source = BuildPartialClassSource(4);
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW130MaxPartialClassDeclarationCount>invalid</RAW130MaxPartialClassDeclarationCount></PropertyGroup></Project>",
            ["/repo/src/ReArtWord.SourceCodeQuality.Domain/ReArtWord.SourceCodeQuality.Domain.csproj"] = "<Project><PropertyGroup><RAW130MaxPartialClassDeclarationCount>0</RAW130MaxPartialClassDeclarationCount></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PartialClassDeclarationLimitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW130");
    }

    [Fact]
    public async Task RAW130_メッセージに設定後上限を含む()
    {
        string source = BuildPartialClassSource(4);
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW130MaxPartialClassDeclarationCount>3</RAW130MaxPartialClassDeclarationCount></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PartialClassDeclarationLimitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic =>
            diagnostic.Id == "RAW130"
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("partial classは3つまで", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RAW130_tests配下と生成コードは検出しない()
    {
        string source = BuildPartialClassSource(5);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> testDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PartialClassDeclarationLimitAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> generatedDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PartialClassDeclarationLimitAnalyzer(),
                "/repo/src/Target.g.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(testDiagnostics, static diagnostic => diagnostic.Id == "RAW130");
        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id == "RAW130");
    }

    private static string BuildPartialClassSource(int declarationCount)
    {
        StringBuilder builder = new();
        for (int index = 0; index < declarationCount; index++)
        {
            builder.AppendLine("public sealed partial class Target");
            builder.AppendLine("{");
            builder.Append("    public int Value");
            builder.Append(index.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine(" => 0;");
            builder.AppendLine("}");
        }

        return builder.ToString();
    }
}
