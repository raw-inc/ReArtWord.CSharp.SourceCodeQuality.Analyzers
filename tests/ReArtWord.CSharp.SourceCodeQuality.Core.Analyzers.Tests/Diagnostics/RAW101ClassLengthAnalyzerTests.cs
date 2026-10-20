using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW101ClassLengthAnalyzerTests
{
    [Fact]
    public async Task RAW101_351行超えクラスで検出される()
    {
        string source = BuildClassSource(360);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ClassLengthAnalyzer(),
                "/repo/src/LargeClass.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW101");
    }

    [Fact]
    public async Task RAW101_既定上限ちょうどは非検出で1行超過は検出する()
    {
        string boundarySource = BuildClassSource(347);
        string exceededSource = BuildClassSource(348);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> boundaryDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                boundarySource,
                new ClassLengthAnalyzer(),
                "/repo/src/BoundaryClass.cs",
                "ReArtWord.SourceCodeQuality.Domain");
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> exceededDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                exceededSource,
                new ClassLengthAnalyzer(),
                "/repo/src/ExceededClass.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(boundaryDiagnostics, static diagnostic => diagnostic.Id == "RAW101");
        Assert.Contains(exceededDiagnostics, static diagnostic => diagnostic.Id == "RAW101");
    }

    [Fact]
    public async Task RAW101_メッセージに責務分離文言を含む()
    {
        string source = BuildClassSource(360);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ClassLengthAnalyzer(),
                "/repo/src/LargeClass.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic =>
            diagnostic.Id == "RAW101"
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("責務分離が適切に行われていない可能性が高いです", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RAW101_テストコードは検出対象外()
    {
        string source = BuildClassSource(360);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ClassLengthAnalyzer(),
                "/repo/tests/LargeClass.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW101");
    }

    [Fact]
    public async Task RAW101_props設定で上限を変更できる()
    {
        string source = BuildClassSource(130);
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW101MaxClassLineCount>120</RAW101MaxClassLineCount></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ClassLengthAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW101");
    }

    [Fact]
    public async Task RAW101_旧props設定キーは読み取らない()
    {
        string source = BuildClassSource(130);
        string oldPropertyName = "RAW" + "001MaxClassLineCount";
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = $"<Project><PropertyGroup><{oldPropertyName}>120</{oldPropertyName}></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ClassLengthAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW101");
    }

    [Fact]
    public async Task RAW101_csproj設定はprops設定を上書きする()
    {
        string source = BuildClassSource(130);
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW101MaxClassLineCount>500</RAW101MaxClassLineCount></PropertyGroup></Project>",
            ["/repo/src/ReArtWord.SourceCodeQuality.Domain/ReArtWord.SourceCodeQuality.Domain.csproj"] = "<Project><PropertyGroup><RAW101MaxClassLineCount>120</RAW101MaxClassLineCount></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ClassLengthAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW101");
    }

    [Fact]
    public async Task RAW101_不正設定値は既定値350へフォールバックする()
    {
        string source = BuildClassSource(340);
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW101MaxClassLineCount>invalid</RAW101MaxClassLineCount></PropertyGroup></Project>",
            ["/repo/src/ReArtWord.SourceCodeQuality.Domain/ReArtWord.SourceCodeQuality.Domain.csproj"] = "<Project><PropertyGroup><RAW101MaxClassLineCount>-10</RAW101MaxClassLineCount></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ClassLengthAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW101");
    }

    private static string BuildClassSource(int fieldCount)
    {
        StringBuilder builder = new();
        builder.AppendLine("/// <summary>日本語の説明です。</summary>");
        builder.AppendLine("public sealed class LargeClass");
        builder.AppendLine("{");

        for (int i = 0; i < fieldCount; i++)
        {
            builder.AppendLine(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "    private readonly int field{0} = {0};",
                    i));
        }

        builder.AppendLine("}");
        return builder.ToString();
    }
}
