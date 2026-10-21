using Microsoft.CodeAnalysis;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// UI型への参照の検出と診断位置を検証します。
/// </summary>
public sealed class RAWMVVM022ViewModelViewReferenceAnalyzerCoverageTests
{
    /// <summary>
    /// UIの基底型と画面型への別名による参照を検出します。
    /// </summary>
    /// <param name="typeName">参照するUI型の完全修飾名です。</param>
    /// <returns>検証完了を表すタスクです。</returns>
    [Theory]
    [InlineData("System.Windows.DependencyObject")]
    [InlineData("System.Windows.Window")]
    [InlineData("System.Windows.Controls.Control")]
    [InlineData("System.Windows.Controls.UserControl")]
    [InlineData("System.Windows.Controls.Page")]
    [InlineData("Microsoft.Maui.Controls.Element")]
    [InlineData("Microsoft.Maui.Controls.Page")]
    [InlineData("Microsoft.Maui.Controls.ContentPage")]
    [InlineData("Microsoft.Maui.Controls.View")]
    [InlineData("Avalonia.AvaloniaObject")]
    [InlineData("Avalonia.Controls.Control")]
    [InlineData("Avalonia.Controls.Window")]
    [InlineData("Avalonia.Controls.UserControl")]
    public async Task RAWMVVM022_UI型の別名参照を検出する(string typeName)
    {
        string source = $"using Target = global::{typeName};\n"
            + MvvmAnalyzerTestSources.WithToolkitStubs("""
                public sealed class OrdersViewModel : ObservableObject
                {
                    public void Show(/*診断位置*/Target target) { }
                }
                """);
        source = DiagnosticLocationAssert.RemoveMarker(source, out int expectedStart);

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source).ConfigureAwait(true);

        Diagnostic diagnostic = Assert.Single(diagnostics, static item => item.Id == "RAWMVVM022");
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAWMVVM022", source, expectedStart, "Target");
    }

    /// <summary>
    /// UIの基底型を間接継承する型を、名前や型引数の有無によらず検出します。
    /// </summary>
    /// <param name="baseType">継承するUIの基底型です。</param>
    /// <returns>検証完了を表すタスクです。</returns>
    [Theory]
    [InlineData("System.Windows.DependencyObject")]
    [InlineData("Microsoft.Maui.Controls.Element")]
    [InlineData("Avalonia.AvaloniaObject")]
    public async Task RAWMVVM022_UI型の間接継承とジェネリック型を検出する(string baseType)
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs($$"""
            namespace Components
            {
                public class ScreenBase<T> : {{baseType}} { }
                public sealed class Screen : ScreenBase<int> { }
                public sealed class Screen<T> : ScreenBase<T> { }
            }
            public sealed class OrdersViewModel : ObservableObject
            {
                public Components.Screen? GetScreen() => null;

                public Components./*診断位置*/Screen<int>? GetGenericScreen() => null;
            }
            """);
        source = DiagnosticLocationAssert.RemoveMarker(source, out int expectedStart);

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source).ConfigureAwait(true);

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM022"));
        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAWMVVM022", source, expectedStart, "Screen");
    }

    /// <summary>
    /// UIの基底型を継承しないWPFのMessageBoxも検出します。
    /// </summary>
    /// <returns>検証完了を表すタスクです。</returns>
    [Fact]
    public async Task RAWMVVM022_MessageBoxへの参照を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class OrdersViewModel : ObservableObject
            {
                public void Show() => global::System.Windows./*診断位置*/MessageBox.Show("message");
            }
            """);
        source = DiagnosticLocationAssert.RemoveMarker(source, out int expectedStart);

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source).ConfigureAwait(true);

        Assert.Single(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM022");
        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAWMVVM022", source, expectedStart, "MessageBox");
    }

    /// <summary>
    /// 別ファイルで定義された派生型をpartialなViewModelから参照しても検出します。
    /// </summary>
    /// <returns>検証完了を表すタスクです。</returns>
    [Fact]
    public async Task RAWMVVM022_別ファイルの派生型を検出する()
    {
        string viewModelSource = """
            namespace ReArtWord.App;
            public sealed partial class OrdersViewModel
            {
                public void Show(/*診断位置*/Screen target) { }
            }
            """;
        viewModelSource = DiagnosticLocationAssert.RemoveMarker(viewModelSource, out int expectedStart);
        IReadOnlyDictionary<string, string> sources = new Dictionary<string, string>
        {
            ["/repo/src/ReArtWord.App/Types.cs"] = MvvmAnalyzerTestSources.WithToolkitStubs("""
                public sealed class Screen : System.Windows.Controls.UserControl { }
                public sealed partial class OrdersViewModel : ObservableObject { }
                """),
            ["/repo/src/ReArtWord.App/OrdersViewModel.cs"] = viewModelSource,
        };

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            sources, new MvvmToolkitUsageAnalyzer(), "ReArtWord.App",
            AnalyzerTestHelper.CreateToolkitPackageFiles()).ConfigureAwait(true);

        Diagnostic diagnostic = Assert.Single(diagnostics, static item => item.Id == "RAWMVVM022");
        Assert.Equal("/repo/src/ReArtWord.App/OrdersViewModel.cs", diagnostic.Location.SourceTree?.FilePath);
        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAWMVVM022", viewModelSource, expectedStart, "Screen");
    }

    /// <summary>
    /// Application.Currentは専用の診断だけで報告します。
    /// </summary>
    /// <param name="typeName">Applicationの完全修飾名です。</param>
    /// <returns>検証完了を表すタスクです。</returns>
    [Theory]
    [InlineData("System.Windows.Application")]
    [InlineData("Microsoft.Maui.Controls.Application")]
    [InlineData("Avalonia.Application")]
    public async Task RAWMVVM022_ApplicationCurrentを重複報告しない(string typeName)
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs($$"""
            public sealed class OrdersViewModel : ObservableObject
            {
                public void Load() => _ = {{typeName}}.Current;
            }
            """);

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source).ConfigureAwait(true);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM022");
        Assert.Single(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM023");
    }

    /// <summary>
    /// MVVMアナライザーの診断を取得します。
    /// </summary>
    /// <param name="source">解析対象ソースです。</param>
    /// <returns>診断一覧を表すタスクです。</returns>
    private static Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(string source)
    {
        return AnalyzerTestHelper.AnalyzeAsync(
            source,
            new MvvmToolkitUsageAnalyzer(),
            "/repo/src/ReArtWord.App/Target.cs",
            "ReArtWord.App",
            AnalyzerTestHelper.CreateToolkitPackageFiles());
    }
}
