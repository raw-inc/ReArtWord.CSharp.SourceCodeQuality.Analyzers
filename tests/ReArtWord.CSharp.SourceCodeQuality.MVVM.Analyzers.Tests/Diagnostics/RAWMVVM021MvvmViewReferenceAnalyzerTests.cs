using System.Diagnostics.CodeAnalysis;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAWMVVM021MvvmViewReferenceAnalyzerTests
{
    [Fact]
    public async Task RAWMVVM021_OnChanged内の同一プロパティ再代入を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                public string Name { get; set; } = string.Empty;

                private void OnNameChanged(string value)
                {
                    Name = value;
                    this.Name += value;
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM021"));
    }

    [Fact]
    public async Task RAWMVVM021_複合代入と増減演算も再代入として検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CounterViewModel : ObservableObject
            {
                public int Count { get; set; }

                private void OnCountChanged(int value)
                {
                    Count += value;
                    Count++;
                    --this.Count;
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Equal(3, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM021"));
    }

    [Fact]
    public async Task RAWMVVM021_別プロパティ代入とChangingHookは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                public string Name { get; set; } = string.Empty;

                public string DisplayName { get; set; } = string.Empty;

                private void OnNameChanged(string value)
                {
                    DisplayName = value;
                }

                private void OnNameChanging(string value)
                {
                    Name = value;
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM021");
    }

    [Fact]
    public async Task RAWMVVM022_ViewModelからView型参照を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            namespace Views
            {
                public sealed class MainView
                {
                }
            }

            public sealed class CustomerViewModel : ObservableObject
            {
                private System.Windows.Window? window;

                private Views.MainView? view;

                public void Show()
                {
                    System.Windows.MessageBox.Show("message");
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.True(2 <= diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM022"));
    }

    [Fact]
    public async Task RAWMVVM022_WPF_MAUI_AvaloniaのView型参照を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerViewModel : ObservableObject
            {
                private System.Windows.Controls.Page? wpfPage;

                private Microsoft.Maui.Controls.ContentPage? mauiPage;

                private Avalonia.Controls.Window? avaloniaWindow;
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.True(3 <= diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM022"));
    }

    [Fact]
    public async Task RAWMVVM022_View接尾辞でもView名前空間でもない型は検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            namespace Models
            {
                public sealed class CustomerScreen
                {
                }
            }

            public sealed class CustomerViewModel : ObservableObject
            {
                private Models.CustomerScreen? screen;
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM022");
    }

    [Fact]
    public async Task RAWMVVM023_ViewModelからApplicationCurrent直接参照を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerViewModel : ObservableObject
            {
                public void Load()
                {
                    _ = System.Windows.Application.Current;
                    _ = Microsoft.Maui.Controls.Application.Current;
                    _ = Avalonia.Application.Current;
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Equal(3, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM023"));
    }

    [Fact]
    public async Task RAWMVVM023_Application型のCurrent以外と独自Applicationは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            namespace Framework
            {
                public sealed class Application
                {
                    public static Application? Current { get; set; }
                }
            }

            public sealed class CustomerViewModel : ObservableObject
            {
                public void Load()
                {
                    _ = System.Windows.Application.Current?.ToString();
                    _ = Framework.Application.Current;
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Single(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM023");
    }

    [Fact]
    public async Task RAWMVVM022とRAWMVVM023_ViewModel外は検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class ViewFactory
            {
                public void Load()
                {
                    _ = System.Windows.Application.Current;
                    _ = new System.Windows.Window();
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM022");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM023");
    }

    private static async Task<IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic>> AnalyzeAsync(string source)
    {
        return await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new MvvmToolkitUsageAnalyzer(),
            "/repo/src/ReArtWord.App/Target.cs",
            "ReArtWord.App",
            AnalyzerTestHelper.CreateToolkitPackageFiles());
    }
}
