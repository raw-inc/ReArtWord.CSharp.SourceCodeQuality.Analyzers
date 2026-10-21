using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// XMLから読み取る行数上限と、設定が無効な場合の動作を検証します。
/// </summary>
// 抑制理由: xUnitの同期コンテキストを維持し、xUnit1030違反を避けるためです。
[SuppressMessage("Reliability", "CA2007", Justification = "xUnitテストではConfigureAwaitを付与しません。")]
public sealed class RAW101ConfigurationXmlAnalyzerTests
{
    /// <summary>
    /// コメント内の設定を無視し、既定の350行を境界として検査します。
    /// </summary>
    /// <param name="extension">設定ファイルの拡張子です。</param>
    /// <param name="lineCount">クラスの行数です。</param>
    /// <returns>検証処理です。</returns>
    [Theory]
    [InlineData(".props", 350)]
    [InlineData(".props", 351)]
    [InlineData(".csproj", 350)]
    [InlineData(".csproj", 351)]
    public async Task RAW101_コメント内の設定は既定上限を変更しない(string extension, int lineCount)
    {
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Settings" + extension] = """
                <Project>
                  <PropertyGroup>
                    <!-- <RAW101MaxClassLineCount>5</RAW101MaxClassLineCount> -->
                  </PropertyGroup>
                  <!--
                  <PropertyGroup>
                    <RAW101MaxClassLineCount>1000</RAW101MaxClassLineCount>
                  </PropertyGroup>
                  -->
                </Project>
                """,
        };

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(lineCount, additionalFiles);

        if (lineCount == 350)
        {
            Assert.Empty(diagnostics);
        }
        else
        {
            Assert.Equal("RAW101", Assert.Single(diagnostics).Id);
        }
    }

    /// <summary>
    /// コメントやCDATA内の見かけ上の要素を除外し、有効な設定値を読み取ります。
    /// </summary>
    /// <param name="propertyXml">プロパティのXMLです。</param>
    /// <param name="expectedDiagnostic">診断の有無です。</param>
    /// <returns>検証処理です。</returns>
    [Theory]
    [InlineData("<RAW101MaxClassLineCount>5</RAW101MaxClassLineCount><!-- <RAW101MaxClassLineCount>350</RAW101MaxClassLineCount> -->", true)]
    [InlineData("<!-- <RAW101MaxClassLineCount>350</RAW101MaxClassLineCount> --><RAW101MaxClassLineCount>5</RAW101MaxClassLineCount>", true)]
    [InlineData("<RAW101MaxClassLineCount>5<!-- 350 --></RAW101MaxClassLineCount>", true)]
    [InlineData("<RAW101MaxClassLineCount>5</RAW101MaxClassLineCount><RAW101MaxClassLineCount>350</RAW101MaxClassLineCount>", false)]
    [InlineData("<RAW101MaxClassLineCount>5</RAW101MaxClassLineCount><RAW101MaxClassLineCount />", true)]
    [InlineData("<raw101maxclasslinecount>5</raw101maxclasslinecount>", true)]
    [InlineData("<RAW101MaxClassLineCount><![CDATA[5]]></RAW101MaxClassLineCount>", true)]
    [InlineData("<![CDATA[<RAW101MaxClassLineCount>5</RAW101MaxClassLineCount>]]>", false)]
    [InlineData("<RAW101MaxClassLineCount>&#53;</RAW101MaxClassLineCount>", true)]
    [InlineData("<RAW101MaxClassLineCount Condition=\"'5' &gt; '0'\">5</RAW101MaxClassLineCount>", true)]
    public async Task RAW101_XML内の有効な要素だけを適用する(string propertyXml, bool expectedDiagnostic)
    {
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Target.csproj"] = $"<Project xmlns=\"http://schemas.microsoft.com/developer/msbuild/2003\"><PropertyGroup>{propertyXml}</PropertyGroup></Project>",
        };

        IReadOnlyCollection<Diagnostic> boundaryDiagnostics = await AnalyzeAsync(5, additionalFiles);
        IReadOnlyCollection<Diagnostic> exceededDiagnostics = await AnalyzeAsync(6, additionalFiles);

        Assert.Empty(boundaryDiagnostics);
        if (expectedDiagnostic)
        {
            Assert.Equal("RAW101", Assert.Single(exceededDiagnostics).Id);
        }
        else
        {
            Assert.Empty(exceededDiagnostics);
        }
    }

    /// <summary>
    /// csprojに有効な上限がなければpropsの上限を使います。
    /// </summary>
    /// <param name="propertyXml">csprojのプロパティです。</param>
    /// <returns>検証処理です。</returns>
    [Theory]
    [InlineData("<!-- <RAW101MaxClassLineCount>350</RAW101MaxClassLineCount> -->")]
    [InlineData("<RAW101MaxClassLineCount>invalid</RAW101MaxClassLineCount>")]
    [InlineData("<RAW101MaxClassLineCount>0</RAW101MaxClassLineCount>")]
    [InlineData("<RAW101MaxClassLineCount>-1</RAW101MaxClassLineCount>")]
    public async Task RAW101_csprojに有効な設定がなければpropsを使う(string propertyXml)
    {
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW101MaxClassLineCount>5</RAW101MaxClassLineCount></PropertyGroup></Project>",
            ["/repo/Target.csproj"] = $"<Project><PropertyGroup>{propertyXml}</PropertyGroup></Project>",
        };

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(6, additionalFiles);

        Assert.Equal("RAW101", Assert.Single(diagnostics).Id);
    }

    /// <summary>
    /// 読み込めないXMLやDTDを無視し、他の設定または既定値を使います。
    /// </summary>
    /// <param name="projectXml">読み込めない設定ファイルです。</param>
    /// <returns>検証処理です。</returns>
    [Theory]
    [InlineData("")]
    [InlineData("<Project><PropertyGroup><RAW101MaxClassLineCount>5</RAW101MaxClassLineCount>")]
    [InlineData("<Project><PropertyGroup><RAW101MaxClassLineCount>350</RAW101MaxClassLineCount>")]
    [InlineData("<Project><!-- <RAW101MaxClassLineCount>5</RAW101MaxClassLineCount></Project>")]
    [InlineData("<!DOCTYPE Project [<!ENTITY limit '5'>]><Project><RAW101MaxClassLineCount>&limit;</RAW101MaxClassLineCount></Project>")]
    [InlineData("<!DOCTYPE Project SYSTEM 'file:///nonexistent-issue-3.dtd'><Project><RAW101MaxClassLineCount>5</RAW101MaxClassLineCount></Project>")]
    public async Task RAW101_読み込めないXMLの設定は適用しない(string projectXml)
    {
        Dictionary<string, string> additionalFiles = new()
        {
            ["/repo/Target.csproj"] = projectXml,
        };

        IReadOnlyCollection<Diagnostic> defaultDiagnostics = await AnalyzeAsync(6, additionalFiles);
        Assert.Empty(defaultDiagnostics);

        additionalFiles["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW101MaxClassLineCount>5</RAW101MaxClassLineCount></PropertyGroup></Project>";
        IReadOnlyCollection<Diagnostic> propsDiagnostics = await AnalyzeAsync(6, additionalFiles);
        Assert.Equal("RAW101", Assert.Single(propsDiagnostics).Id);
    }

    /// <summary>
    /// 指定行数のクラスに行数規則を適用します。
    /// </summary>
    /// <param name="lineCount">クラスの行数です。</param>
    /// <param name="additionalFiles">設定ファイルです。</param>
    /// <returns>報告された診断です。</returns>
    private static Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(int lineCount, IReadOnlyDictionary<string, string> additionalFiles)
    {
        string source = "public sealed class Target\n{\n" + new string('\n', lineCount - 3) + "}";
        return AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ClassLengthAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain",
            additionalFiles);
    }
}
