using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// 相反する private フィールド命名規則の設定を確認します。
/// </summary>
public sealed class RAW120CFieldNamingConfigurationTests
{
    /// <summary>
    /// 相反する名前変更の代わりに設定の競合を報告します。
    /// </summary>
    /// <param name="name">フィールド名です。</param>
    /// <returns>診断IDと案内内容の確認です。</returns>
    [Theory]
    [InlineData("count")]
    [InlineData("_count")]
    [InlineData("Count")]
    [InlineData("count_value")]
    public async Task RAW120C_両方の規則が有効なら競合だけを報告する(string name)
    {
        Diagnostic diagnostic = Assert.Single(await FieldNamingTestHelper.AnalyzeAsync(
            $"class Target {{ private int {name}; }}",
            new Dictionary<string, ReportDiagnostic> { ["RAW120A"] = ReportDiagnostic.Error, ["RAW120B"] = ReportDiagnostic.Error }).ConfigureAwait(true));
        Assert.Equal("RAW120C", diagnostic.Id);
        Assert.Contains("RAW120A", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Contains("RAW120B", diagnostic.GetMessage(System.Globalization.CultureInfo.InvariantCulture), StringComparison.Ordinal);
    }

    /// <summary>
    /// 有効な規則が一つなら一般規則との競合も起こしません。
    /// </summary>
    /// <param name="severity">有効化に使用する重大度です。</param>
    /// <returns>error 以外の設定でも同じ命名方式が適用される確認です。</returns>
    [Theory]
    [InlineData(ReportDiagnostic.Error)]
    [InlineData(ReportDiagnostic.Warn)]
    [InlineData(ReportDiagnostic.Info)]
    [InlineData(ReportDiagnostic.Hidden)]
    public async Task RAW120C_片方が無効なら競合しない(ReportDiagnostic severity)
    {
        Assert.Empty(await FieldNamingTestHelper.AnalyzeAsync(
            "class Target { private int _count; }",
            new Dictionary<string, ReportDiagnostic> { ["RAW120A"] = ReportDiagnostic.Suppress, ["RAW120B"] = severity }).ConfigureAwait(true));
    }

    /// <summary>
    /// ファイルごとに選択した規則を使い、全体設定を誤って優先しません。
    /// </summary>
    /// <returns>同じコンパイル内で異なる方式を指定する確認です。</returns>
    [Fact]
    public async Task RAW120C_ファイルごとの選択を混同しない()
    {
        Dictionary<string, string> sources = new()
        {
            ["/repo/src/Without.cs"] = "class Without { private int _count; }",
            ["/repo/src/With.cs"] = "class With { private int count; }",
        };
        FieldNamingSyntaxTreeOptions provider = new(
            new Dictionary<string, IReadOnlyDictionary<string, ReportDiagnostic>>
            {
                ["/repo/src/Without.cs"] = new Dictionary<string, ReportDiagnostic>
                {
                    ["RAW120A"] = ReportDiagnostic.Error,
                    ["RAW120B"] = ReportDiagnostic.Suppress,
                },
            },
            new Dictionary<string, ReportDiagnostic> { ["RAW120B"] = ReportDiagnostic.Error });
        ImmutableArray<Diagnostic> diagnostics = await FieldNamingTestHelper.AnalyzeAsync(
            sources,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithSyntaxTreeOptionsProvider(provider)).ConfigureAwait(true);

        Assert.Equal(2, diagnostics.Length);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "RAW120A" && diagnostic.Location.SourceTree?.FilePath == "/repo/src/Without.cs");
        Assert.Contains(diagnostics, diagnostic => diagnostic.Id == "RAW120B" && diagnostic.Location.SourceTree?.FilePath == "/repo/src/With.cs");
    }

    /// <summary>
    /// ファイル別に none または default を指定すると選択式の規則が無効になります。
    /// </summary>
    /// <param name="severity">ファイルに指定する重大度です。</param>
    /// <returns>無効化された専用規則へ一般規則を委譲しないことの確認です。</returns>
    [Theory]
    [InlineData(ReportDiagnostic.Suppress)]
    [InlineData(ReportDiagnostic.Default)]
    public async Task RAW120C_ファイルで専用規則を無効化したら一般規則を使う(ReportDiagnostic severity)
    {
        FieldNamingSyntaxTreeOptions provider = new(
            new Dictionary<string, IReadOnlyDictionary<string, ReportDiagnostic>>
            {
                ["/repo/src/Target.cs"] = new Dictionary<string, ReportDiagnostic> { ["RAW120B"] = severity },
            },
            new Dictionary<string, ReportDiagnostic> { ["RAW120B"] = ReportDiagnostic.Error });
        Diagnostic diagnostic = Assert.Single(await FieldNamingTestHelper.AnalyzeAsync(
            new Dictionary<string, string> { ["/repo/src/Target.cs"] = "class Target { private int _count; }" },
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary).WithSyntaxTreeOptionsProvider(provider)).ConfigureAwait(true));
        Assert.Equal("RAW612", diagnostic.Id);
    }

    /// <summary>
    /// コンパイラーの個別抑制をファイル設定より優先します。
    /// </summary>
    /// <returns>NoWarn 相当の指定で無効になった規則へ委譲しない確認です。</returns>
    [Fact]
    public async Task RAW120C_個別のコンパイラー設定を優先する()
    {
        FieldNamingSyntaxTreeOptions provider = new(
            new Dictionary<string, IReadOnlyDictionary<string, ReportDiagnostic>>
            {
                ["/repo/src/Target.cs"] = new Dictionary<string, ReportDiagnostic> { ["RAW120B"] = ReportDiagnostic.Error },
            },
            ImmutableDictionary<string, ReportDiagnostic>.Empty);
        CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithSyntaxTreeOptionsProvider(provider)
            .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic> { ["RAW120B"] = ReportDiagnostic.Suppress });
        Diagnostic diagnostic = Assert.Single(await FieldNamingTestHelper.AnalyzeAsync(
            new Dictionary<string, string> { ["/repo/src/Target.cs"] = "class Target { private int _count; }" },
            options).ConfigureAwait(true));
        Assert.Equal("RAW612", diagnostic.Id);
    }

    /// <summary>
    /// 一括指定より個別に指定した重大度を優先します。
    /// </summary>
    /// <returns>実際に有効な重大度に従う確認です。</returns>
    [Fact]
    public async Task RAW120C_警告の一括抑制より個別の有効化を優先する()
    {
        CSharpCompilationOptions options = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithGeneralDiagnosticOption(ReportDiagnostic.Suppress)
            .WithSpecificDiagnosticOptions(new Dictionary<string, ReportDiagnostic>
            {
                ["RAW120A"] = ReportDiagnostic.Error,
                ["RAW120B"] = ReportDiagnostic.Warn,
            });
        Diagnostic diagnostic = Assert.Single(await FieldNamingTestHelper.AnalyzeAsync(
            new Dictionary<string, string> { ["/repo/src/Target.cs"] = "class Target { private int _count; }" },
            options).ConfigureAwait(true));
        Assert.Equal("RAW120C", diagnostic.Id);
    }
}
