using Microsoft.CodeAnalysis;
using ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers.Tests.Diagnostics;

/// <summary>
/// Razor の行マッピングと利用者のメンバー宣言を照合します。
/// </summary>
public sealed class RAWRAZOR001CodeBehindMemberAnalyzerTests
{
    /// <summary>
    /// Razor に記述した宣言の識別子へ診断します。
    /// </summary>
    /// <param name="member">メンバー宣言です。</param>
    /// <param name="identifier">診断対象の識別子です。</param>
    /// <returns>元位置と診断内容の検証です。</returns>
    [Theory]
    [InlineData("private int value;", "value")]
    [InlineData("private int first, second;", "first")]
    [InlineData("public string? Value { get; set; }", "Value")]
    [InlineData("private void Click() { }", "Click")]
    [InlineData("private View() { }", "View")]
    [InlineData("public event System.Action? Changed;", "Changed")]
    [InlineData("public event System.Action Changed { add { } remove { } }", "Changed")]
    [InlineData("private sealed class State { }", "State")]
    [InlineData("private struct State { }", "State")]
    [InlineData("private record State;", "State")]
    public async Task RAWRAZOR001_Razorに対応するメンバーを識別子位置へ報告する(string member, string identifier)
    {
        string source = "class View\n{\n#line 20 \"Pages/View.razor\"\n" + member + "\n#line default\n}";
        Diagnostic diagnostic = Assert.Single(await AnalyzerTestHelper.AnalyzeAsync(source, "/repo/obj/View.razor.g.cs").ConfigureAwait(true));
        Assert.Equal("RAWRAZOR001", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("Razor", diagnostic.Descriptor.Category);
        Assert.Equal("Pages/View.razor", diagnostic.Location.GetMappedLineSpan().Path);
        Assert.Equal(19, diagnostic.Location.GetMappedLineSpan().StartLinePosition.Line);
        Assert.Equal(identifier, source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }

    /// <summary>
    /// 描画式と通常のコードビハインドはメンバー分離の対象外です。
    /// </summary>
    /// <param name="source">検査するソースです。</param>
    /// <param name="path">ソースパスです。</param>
    /// <returns>過剰な検出がないことの確認です。</returns>
    [Theory]
    [InlineData("class View { private void Click() { } }", "/repo/src/View.razor.cs")]
    [InlineData("class View { private void Click() { } }", "/repo/obj/View.razor.g.cs")]
    [InlineData("class View { void Render() {\n#line 12 \"View.razor\"\nvar text = \"@code { private int value; }\";\n#line default\n} }", "/repo/obj/View.razor.g.cs")]
    [InlineData("class View {\n#line 12 \"View.cshtml\"\nprivate int value;\n#line default\n}", "/repo/obj/View.g.cs")]
    public async Task RAWRAZOR001_描画式や通常のCSharp宣言を検出しない(string source, string path)
    {
        Assert.Empty(await AnalyzerTestHelper.AnalyzeAsync(source, path).ConfigureAwait(true));
    }

    /// <summary>
    /// 宣言の属性ではなく識別子の元位置を使用します。
    /// </summary>
    /// <returns>生成コードの属性をまたぐ行マッピングの検証です。</returns>
    [Fact]
    public async Task RAWRAZOR001_属性の後から始まる行マッピングを使用する()
    {
        string source = "class View {\n[System.Obsolete]\n#line 30 \"View.RAZOR\"\nprivate int value;\n#line default\n}";
        Diagnostic diagnostic = Assert.Single(await AnalyzerTestHelper.AnalyzeAsync(source, "/repo/obj/View.razor.g.cs").ConfigureAwait(true));
        Assert.Equal("View.RAZOR", diagnostic.Location.GetMappedLineSpan().Path);
        Assert.Equal(29, diagnostic.Location.GetMappedLineSpan().StartLinePosition.Line);
        Assert.Equal("value", source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }

    /// <summary>
    /// inject ディレクティブで型名とプロパティ名だけが元画面へ対応する生成宣言は対象外です。
    /// </summary>
    /// <returns>Razor SDK が生成する注入プロパティの回帰確認です。</returns>
    [Fact]
    public async Task RAWRAZOR001_注入ディレクティブの生成プロパティを検出しない()
    {
        string source = """
            namespace Microsoft.AspNetCore.Components
            {
                public sealed class InjectAttribute : System.Attribute { }
            }
            class View
            {
                [global::Microsoft.AspNetCore.Components.InjectAttribute] private
            #line (1,9)-(1,22) "View.razor"
                System.String
            #line default
            #line hidden
            #line (1,23)-(1,30) "View.razor"
                Service
            #line default
            #line hidden
                { get; set; } = default!;
            }
            """;
        Assert.Empty(await AnalyzerTestHelper.AnalyzeAsync(source, "/repo/obj/View.razor.g.cs").ConfigureAwait(true));
    }

    /// <summary>
    /// 注入属性があっても Razor に手書きしたプロパティ宣言は対象です。
    /// </summary>
    /// <returns>生成コードの除外が手書き宣言へ広がらないことの確認です。</returns>
    [Fact]
    public async Task RAWRAZOR001_注入属性を持つ手書きプロパティを検出する()
    {
        string source = """
            namespace Microsoft.AspNetCore.Components
            {
                public sealed class InjectAttribute : System.Attribute { }
            }
            class View
            {
            #line 10 "View.razor"
                [global::Microsoft.AspNetCore.Components.InjectAttribute]
                private string Service { get; set; } = default!;
            #line default
            }
            """;
        Diagnostic diagnostic = Assert.Single(await AnalyzerTestHelper.AnalyzeAsync(source, "/repo/obj/View.razor.g.cs").ConfigureAwait(true));
        Assert.Equal("RAWRAZOR001", diagnostic.Id);
        Assert.Equal("Service", source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }

    /// <summary>
    /// Analyzer 初期化の引数検証を維持します。
    /// </summary>
    [Fact]
    public void RAWRAZOR001_初期化コンテキストのnullを拒否する()
    {
        Assert.Throws<ArgumentNullException>(() => new RazorComponentAnalyzer().Initialize(null!));
    }
}
