using System.Diagnostics.CodeAnalysis;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAWMVVM012MvvmNotificationAnalyzerTests
{
    [Fact]
    public async Task RAWMVVM012_ViewModel内のPropertyChanged購読を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerViewModel : ObservableObject
            {
                public void Watch(ObservableObject source)
                {
                    source.PropertyChanged += Handle;
                }

                private void Handle(object? sender, PropertyChangedEventArgs e)
                {
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source, "/repo/src/ReArtWord.App/Target.cs");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM012");
    }

    [Fact]
    public async Task RAWMVVM012_ViewModel外のPropertyChanged購読は検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class PlainWatcher
            {
                public void Watch(ObservableObject source)
                {
                    source.PropertyChanged += Handle;
                    source.PropertyChanged -= Handle;
                }

                private void Handle(object? sender, PropertyChangedEventArgs e)
                {
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source, "/repo/src/ReArtWord.App/Target.cs");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM012");
    }

    [Fact]
    public async Task RAWMVVM012_購読解除もViewModel内では検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerViewModel : ObservableObject
            {
                public void Unwatch(ObservableObject source)
                {
                    source.PropertyChanged -= Handle;
                }

                private void Handle(object? sender, PropertyChangedEventArgs e)
                {
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source, "/repo/src/ReArtWord.App/Target.cs");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM012");
    }

    [Fact]
    public async Task RAWMVVM016_PropertyChangedとPropertyChanging手動発火を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class ManualNotification : INotifyPropertyChanged, INotifyPropertyChanging
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public event PropertyChangingEventHandler? PropertyChanging;

                public void Raise()
                {
                    PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(Raise)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Raise)));
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source, "/repo/src/ReArtWord.App/Target.cs");

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM016"));
    }

    [Fact]
    public async Task RAWMVVM016_直接Invokeでも通知イベント手動発火を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class ManualNotification : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;

                public void Raise()
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(nameof(Raise)));
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source, "/repo/src/ReArtWord.App/Target.cs");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM016");
    }

    [Fact]
    public async Task RAWMVVM016_通知イベント以外のInvokeは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class ManualNotification
            {
                private event Action? Saved;

                public void Raise()
                {
                    Saved?.Invoke();
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source, "/repo/src/ReArtWord.App/Target.cs");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM016");
    }

    [Fact]
    public async Task RAWMVVM017_OnPropertyChangedのnameof依存通知を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerViewModel : ObservableObject
            {
                public string FullName { get; } = string.Empty;

                public void Refresh()
                {
                    OnPropertyChanged(nameof(FullName));
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source, "/repo/src/ReArtWord.App/Target.cs");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM017");
    }

    [Fact]
    public async Task RAWMVVM017_privateプロパティや文字列指定は依存通知として検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerViewModel : ObservableObject
            {
                private string SecretName { get; } = string.Empty;

                public void Refresh()
                {
                    OnPropertyChanged(nameof(SecretName));
                    OnPropertyChanged("FullName");
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source, "/repo/src/ReArtWord.App/Target.cs");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM017");
    }

    [Fact]
    public async Task RAWMVVM017_ViewModel外のOnPropertyChangedは依存通知として検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class PlainNotifier
            {
                public string FullName { get; } = string.Empty;

                public void OnPropertyChanged(string propertyName)
                {
                }

                public void Refresh()
                {
                    OnPropertyChanged(nameof(FullName));
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source, "/repo/src/ReArtWord.App/Target.cs");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM017");
    }

    [Fact]
    public async Task 通知系新規診断_生成コードは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerViewModel : ObservableObject
            {
                public string FullName { get; } = string.Empty;

                public void Watch(ObservableObject source)
                {
                    source.PropertyChanged += Handle;
                    OnPropertyChanged(nameof(FullName));
                }

                private void Handle(object? sender, PropertyChangedEventArgs e)
                {
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source, "/repo/src/ReArtWord.App/Target.g.cs");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM012");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM017");
    }

    private static async Task<IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic>> AnalyzeAsync(
        string source,
        string filePath)
    {
        return await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new MvvmToolkitUsageAnalyzer(),
            filePath,
            "ReArtWord.App",
            AnalyzerTestHelper.CreateToolkitPackageFiles());
    }
}
