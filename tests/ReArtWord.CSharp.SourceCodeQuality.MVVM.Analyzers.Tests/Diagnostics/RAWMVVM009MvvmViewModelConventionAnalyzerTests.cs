using System.Diagnostics.CodeAnalysis;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAWMVVM009MvvmViewModelConventionAnalyzerTests
{
    [Fact]
    public async Task RAWMVVM009_Observable継承クラスへViewModel接尾辞を要求する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public class Customer : ObservableObject
            {
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM009");
        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM024");
    }

    [Fact]
    public async Task RAWMVVM009_ObservableRecipientとObservableValidator継承もViewModel接尾辞を要求する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerRecipient : ObservableRecipient
            {
            }

            public sealed class CustomerValidator : ObservableValidator
            {
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM009"));
    }

    [Fact]
    public async Task RAWMVVM010_ViewModel命名クラスへObservable継承を要求する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerViewModel
            {
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM010");
    }

    [Fact]
    public async Task RAWMVVM010_Toolkit外の同名基底型はObservable継承扱いにしない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            namespace OtherToolkit
            {
                public class ObservableObject
                {
                }
            }

            public sealed class CustomerViewModel : OtherToolkit.ObservableObject
            {
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM010");
    }

    [Fact]
    public async Task RAWMVVM009とRAWMVVM010_名前と継承が揃えば検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerViewModel : ObservableRecipient
            {
            }

            public sealed class PlainModel
            {
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM009");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM010");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM024");
    }

    [Fact]
    public async Task RAWMVVM011_INotifyPropertyChanged直接実装を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class ManualNotification : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM011");
    }

    [Fact]
    public async Task RAWMVVM011_生成コードではINotifyPropertyChanged直接実装を検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class ManualNotification : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler? PropertyChanged;
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MvvmToolkitUsageAnalyzer(),
                "/repo/src/ReArtWord.App/Target.g.cs",
                "ReArtWord.App",
                AnalyzerTestHelper.CreateToolkitPackageFiles());

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM011");
    }

    [Fact]
    public async Task RAWMVVM024_sealedまたはabstractのViewModelは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class SealedCustomerViewModel : ObservableObject
            {
            }

            public abstract class AbstractCustomerViewModel : ObservableObject
            {
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM024");
    }

    [Fact]
    public async Task RAWMVVM026_ViewModelの通知なしpublic可変プロパティを検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerViewModel : ObservableObject
            {
                public string Name { get; set; } = string.Empty;

                public string DisplayName
                {
                    get => string.Empty;
                    set => OnPropertyChanged(nameof(DisplayName));
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Single(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM026");
    }

    [Fact]
    public async Task RAWMVVM026_public以外または読み取り専用プロパティは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerViewModel : ObservableObject
            {
                internal string InternalName { get; set; } = string.Empty;

                public string DisplayName { get; } = string.Empty;

                public static string SharedName { get; set; } = string.Empty;
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM026");
    }

    [Fact]
    public async Task RAWMVVM026_SetProperty通知付きsetterは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerViewModel : ObservableObject
            {
                private string name = string.Empty;

                public string Name
                {
                    get => name;
                    set => SetProperty(ref name, value);
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM026");
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
