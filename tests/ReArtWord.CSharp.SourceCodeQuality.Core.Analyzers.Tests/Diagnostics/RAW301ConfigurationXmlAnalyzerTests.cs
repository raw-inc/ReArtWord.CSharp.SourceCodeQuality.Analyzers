using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// XMLコメント内の許可IDがNoWarnの検査に影響しないことを検証します。
/// </summary>
// 抑制理由: xUnitの同期コンテキストを維持し、xUnit1030違反を避けるためです。
[SuppressMessage("Reliability", "CA2007", Justification = "xUnitテストではConfigureAwaitを付与しません。")]
public sealed class RAW301ConfigurationXmlAnalyzerTests
{
    /// <summary>
    /// コメント内の許可IDを除外し、有効な設定と既定の許可IDを使います。
    /// </summary>
    /// <param name="extension">設定ファイルの拡張子です。</param>
    /// <returns>検証処理です。</returns>
    [Theory]
    [InlineData(".props")]
    [InlineData(".csproj")]
    public async Task RAW301_コメント内の許可IDは追加しない(string extension)
    {
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Settings" + extension] = """
                <Project>
                  <PropertyGroup>
                    <!-- <RAW301AllowedNoWarnIds>CA2007</RAW301AllowedNoWarnIds> -->
                    <RAW301AllowedNoWarnIds>CA1305</RAW301AllowedNoWarnIds>
                    <!--
                    <RAW301AllowedNoWarnIds>CA2007</RAW301AllowedNoWarnIds>
                    -->
                    <NoWarn>$(NoWarn);CS1591;CA1707;CA1305;CA2007</NoWarn>
                  </PropertyGroup>
                </Project>
                """,
        };

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            "public sealed class Target { }",
            new SuppressionGovernanceAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain",
            additionalFiles);

        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("RAW301", diagnostic.Id);
        Assert.Contains("CA2007", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Equal("/repo/Settings" + extension, diagnostic.Location.GetLineSpan().Path);
    }
}
