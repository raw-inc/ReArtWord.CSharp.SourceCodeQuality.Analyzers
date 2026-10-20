using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW455TernaryExpressionStyleAnalyzerTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task RAW456_整列済み三項演算子は条件数4個まで非検出(int conditionCount)
    {
        string source = TernaryExpressionTestSourceFactory.BuildFalseNestedTernarySource(conditionCount);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW455");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW456");
    }

    [Fact]
    public async Task RAW456_真側ネストと偽側ネストの整列済み三項演算子は非検出()
    {
        string source = TernaryExpressionTestSourceFactory.BuildRequestedExamplesSource();

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW455");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW456");
    }

    [Fact]
    public async Task RAW456_単一行三項演算子を検出する()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(bool condition)
                {
                    return condition ? true : false;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW456");
    }

    [Fact]
    public async Task RAW456_コロン列違いを検出する()
    {
        string source = TernaryExpressionTestSourceFactory.BuildMisalignedColonSource();

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW456");
    }

    [Fact]
    public async Task RAW456_コロン行前方の式を検出する()
    {
        string source = TernaryExpressionTestSourceFactory.BuildColonLinePrefixSource();

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW456");
    }

    [Fact]
    public async Task RAW456_インデントなしの整列済み三項演算子は非検出()
    {
        string firstLine = "public bool Value = true ? true";
        int questionColumn = firstLine.IndexOf('?', StringComparison.Ordinal);
        string source = string.Join(
            Environment.NewLine,
            "public sealed class Target",
            "{",
            firstLine,
            new string(' ', questionColumn) + ": false;",
            "}");

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW456");
    }

    [Fact]
    public async Task RAW455_三項条件数5個以上を検出する()
    {
        string source = TernaryExpressionTestSourceFactory.BuildFalseNestedTernarySource(5);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW455");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW456");
    }

    [Fact]
    public async Task RAW455_props設定で閾値を変更できる()
    {
        string source = TernaryExpressionTestSourceFactory.BuildFalseNestedTernarySource(3);
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW455TernaryConditionCountThreshold>3</RAW455TernaryConditionCountThreshold></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW455");
    }

    [Fact]
    public async Task RAW455_csproj設定はprops設定を上書きする()
    {
        string source = TernaryExpressionTestSourceFactory.BuildFalseNestedTernarySource(3);
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW455TernaryConditionCountThreshold>10</RAW455TernaryConditionCountThreshold></PropertyGroup></Project>",
            ["/repo/src/ReArtWord.SourceCodeQuality.Domain/ReArtWord.SourceCodeQuality.Domain.csproj"] = "<Project><PropertyGroup><RAW455TernaryConditionCountThreshold>3</RAW455TernaryConditionCountThreshold></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW455");
    }

    [Fact]
    public async Task RAW455_不正設定値は既定値5へフォールバックする()
    {
        string source = TernaryExpressionTestSourceFactory.BuildFalseNestedTernarySource(4);
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW455TernaryConditionCountThreshold>invalid</RAW455TernaryConditionCountThreshold></PropertyGroup></Project>",
            ["/repo/src/ReArtWord.SourceCodeQuality.Domain/ReArtWord.SourceCodeQuality.Domain.csproj"] = "<Project><PropertyGroup><RAW455TernaryConditionCountThreshold>0</RAW455TernaryConditionCountThreshold></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW455");
    }

    [Fact]
    public async Task RAW455_メッセージに設定後閾値を含む()
    {
        string source = TernaryExpressionTestSourceFactory.BuildFalseNestedTernarySource(3);
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW455TernaryConditionCountThreshold>3</RAW455TernaryConditionCountThreshold></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic =>
            diagnostic.Id == "RAW455"
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("3 条件未満", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RAW456_tests配下でも検出する()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(bool condition)
                {
                    return condition ? true : false;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW456");
    }

    [Fact]
    public async Task RAW456_Analyzer自己テストと生成コードは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(bool condition)
                {
                    return condition ? true : false;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> selfDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/tests/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests/Target.cs",
                "ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests");
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> generatedDiagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new TernaryExpressionStyleAnalyzer(),
                "/repo/src/Target.g.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(selfDiagnostics, static diagnostic => diagnostic.Id == "RAW456");
        Assert.DoesNotContain(generatedDiagnostics, static diagnostic => diagnostic.Id == "RAW456");
    }

}
