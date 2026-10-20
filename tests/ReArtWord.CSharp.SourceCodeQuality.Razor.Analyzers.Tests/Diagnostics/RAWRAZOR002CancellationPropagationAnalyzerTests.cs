using System.Globalization;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers.Tests.Diagnostics;

/// <summary>
/// コンポーネント内の取消トークン伝搬と対象外の境界を確認します。
/// </summary>
public sealed class RAWRAZOR002CancellationPropagationAnalyzerTests
{
    /// <summary>
    /// 省略・default・None と明示トークンを区別します。
    /// </summary>
    /// <param name="invocation">コンポーネント内の呼出しです。</param>
    /// <param name="invalid">診断を期待するかです。</param>
    /// <returns>診断の有無と発火位置の検証です。</returns>
    [Theory]
    [InlineData("Api.ReadAsync()", true)]
    [InlineData("Api.ReadAsync(default)", true)]
    [InlineData("Api.ReadAsync((CancellationToken)default)", true)]
    [InlineData("Api.ReadAsync(default(CancellationToken))", true)]
    [InlineData("Api.ReadAsync(CancellationToken.None)", true)]
    [InlineData("Api.ReadAsync(cancellationToken: default)", true)]
    [InlineData("Api.ReadAsync(Token.None)", true)]
    [InlineData("Api.ReadAsync<int>()", true)]
    [InlineData("Api.ReadAsync(cancellationToken)", false)]
    [InlineData("Api.ReadAsync(cancellationToken: cancellationToken)", false)]
    [InlineData("Api.ReadWithoutTokenAsync()", false)]
    public async Task RAWRAZOR002_取消不能な指定を報告する(string invocation, bool invalid)
    {
        string source = WithComponent("class View : ComponentBase { public Task LoadAsync(CancellationToken cancellationToken) => " + invocation + "; }");
        ImmutableArray<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(source, "/repo/src/View.razor.cs").ConfigureAwait(true);
        if (!invalid)
        {
            Assert.Empty(diagnostics);
            return;
        }

        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("RAWRAZOR002", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal("Razor", diagnostic.Descriptor.Category);
        Assert.Contains("ReadAsync", diagnostic.GetMessage(CultureInfo.InvariantCulture), StringComparison.Ordinal);
        Assert.Equal("ReadAsync", source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }

    /// <summary>
    /// 間接継承と名前の別名でも同じコンポーネント型を識別します。
    /// </summary>
    /// <returns>複数ソースを使う意味解析の検証です。</returns>
    [Fact]
    public async Task RAWRAZOR002_別ファイルの基底型を介して検出する()
    {
        Dictionary<string, string> sources = new()
        {
            ["/repo/src/Component.cs"] = WithComponent("abstract class Screen : ComponentBase { }"),
            ["/repo/src/View.razor.cs"] = "using Base = Screen; class View : Base { public System.Threading.Tasks.Task LoadAsync() => Api.ReadAsync(); }",
        };
        Diagnostic diagnostic = Assert.Single(await AnalyzerTestHelper.AnalyzeAsync(sources).ConfigureAwait(true));
        Assert.Equal("RAWRAZOR002", diagnostic.Id);
        Assert.Equal("/repo/src/View.razor.cs", diagnostic.Location.GetLineSpan().Path);
    }

    /// <summary>
    /// Razor に記述した処理は生成コード内でも検出します。
    /// </summary>
    /// <returns>Razor の元位置への診断の確認です。</returns>
    [Fact]
    public async Task RAWRAZOR002_生成コードでも利用者の呼出しを検出する()
    {
        string source = WithComponent("class View : ComponentBase { public Task LoadAsync() {\n#line 15 \"View.razor\"\nreturn Api.ReadAsync();\n#line default\n} }");
        Diagnostic diagnostic = Assert.Single(await AnalyzerTestHelper.AnalyzeAsync(source, "/repo/obj/View.razor.g.cs").ConfigureAwait(true));
        Assert.Equal("RAWRAZOR002", diagnostic.Id);
        Assert.Equal("View.razor", diagnostic.Location.GetMappedLineSpan().Path);
        Assert.Equal(14, diagnostic.Location.GetMappedLineSpan().StartLinePosition.Line);
    }

    /// <summary>
    /// 通常の型、別の同名型、解放境界へ UI の取消しを要求しません。
    /// </summary>
    /// <param name="body">型宣言です。</param>
    /// <returns>対象外の呼出しに対する検証です。</returns>
    [Theory]
    [InlineData("class Service { public Task LoadAsync() => Api.ReadAsync(); }")]
    [InlineData("class View : ComponentBase { public Task DisposeAsync() => Api.ReadAsync(); }")]
    [InlineData("class View : ComponentBase { public void Dispose() { Api.ReadAsync(); } }")]
    [InlineData("namespace Other { class ComponentBase { } class View : ComponentBase { public Task LoadAsync() => Api.ReadAsync(); } }")]
    public async Task RAWRAZOR002_対象外の呼出しを検出しない(string body)
    {
        Assert.Empty(await AnalyzerTestHelper.AnalyzeAsync(WithComponent(body), "/repo/src/View.razor.cs").ConfigureAwait(true));
    }

    /// <summary>
    /// 同じコンポーネントのローカル関数にも明示トークンを要求します。
    /// </summary>
    /// <returns>単純名で呼ぶ処理の診断位置の検証です。</returns>
    [Fact]
    public async Task RAWRAZOR002_ローカル関数への取消不能トークンを検出する()
    {
        string source = WithComponent("class View : ComponentBase { public void Start() { void Local(CancellationToken token) { } Local(CancellationToken.None); } }");
        Diagnostic diagnostic = Assert.Single(await AnalyzerTestHelper.AnalyzeAsync(source, "/repo/src/View.razor.cs").ConfigureAwait(true));
        Assert.Equal("RAWRAZOR002", diagnostic.Id);
        Assert.Equal("Local", source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }

    /// <summary>
    /// null 条件付き呼出しでも対象メソッドの名前を報告します。
    /// </summary>
    /// <returns>メンバー束縛式の意味解析と診断位置の検証です。</returns>
    [Fact]
    public async Task RAWRAZOR002_null条件付き呼出しの取消省略を検出する()
    {
        string source = WithComponent("""
            class Client { public Task ReadAsync(CancellationToken token = default) => Task.CompletedTask; }
            class View : ComponentBase { public Task? LoadAsync(Client? client) => client?.ReadAsync(); }
            """);
        Diagnostic diagnostic = Assert.Single(await AnalyzerTestHelper.AnalyzeAsync(source, "/repo/src/View.razor.cs").ConfigureAwait(true));
        Assert.Equal("RAWRAZOR002", diagnostic.Id);
        Assert.Equal("ReadAsync", source.Substring(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.Length));
    }

    /// <summary>
    /// Razor 型がないプロジェクトにも適用できます。
    /// </summary>
    /// <returns>Razor 参照のないコンパイルの検証です。</returns>
    [Fact]
    public async Task RAWRAZOR002_ComponentBaseが存在しないコンパイルを処理する()
    {
        Assert.Empty(await AnalyzerTestHelper.AnalyzeAsync("class OrdinaryType { }", "/repo/src/OrdinaryType.cs").ConfigureAwait(true));
    }

    /// <summary>
    /// 同じコンパイルに必要な基底型と取消可能な API を定義します。
    /// </summary>
    /// <param name="body">テスト対象の型宣言です。</param>
    /// <returns>コンパイル可能な C# ソースです。</returns>
    private static string WithComponent(string body)
    {
        return """
            using System.Threading;
            using System.Threading.Tasks;
            using Microsoft.AspNetCore.Components;
            using Token = System.Threading.CancellationToken;
            namespace Microsoft.AspNetCore.Components { public class ComponentBase { } }
            static class Api
            {
                public static Task ReadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
                public static Task ReadAsync<T>(CancellationToken cancellationToken = default) => Task.CompletedTask;
                public static Task ReadWithoutTokenAsync() => Task.CompletedTask;
            }
            """ + "\n" + body;
    }
}
