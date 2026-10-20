using System.Diagnostics.CodeAnalysis;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAWMVVM015MvvmServiceAndRelayCommandAdditionalAnalyzerTests
{
    [Fact]
    public async Task RAWMVVM015_ViewModel内のService直接生成だけを検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public interface IUserService
            {
            }

            public sealed class UserService : IUserService
            {
            }

            public sealed class CustomerViewModel : ObservableObject
            {
                public void Create()
                {
                    object service = new UserService();
                    UserService typed = new();
                    object names = new List<string>();
                }
            }

            public sealed class PlainFactory
            {
                public object Create()
                {
                    return new UserService();
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM015"));
    }

    [Fact]
    public async Task RAWMVVM015_実装インターフェース名がServiceなら具象型名が違っても検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public interface ICustomerService
            {
            }

            public sealed class CustomerRepository : ICustomerService
            {
            }

            public sealed class CustomerViewModel : ObservableObject
            {
                public void Create()
                {
                    object service = new CustomerRepository();
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM015");
    }

    [Fact]
    public async Task RAWMVVM015_targetTypedService生成を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class UserService
            {
            }

            public sealed class CustomerViewModel : ObservableObject
            {
                public void Create()
                {
                    UserService service = new();
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM015");
    }

    [Fact]
    public async Task RAWMVVM015_Service名を持たない通常型はViewModel内でも検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed class CustomerRepository
            {
            }

            public sealed class CustomerViewModel : ObservableObject
            {
                public void Create()
                {
                    object repository = new CustomerRepository();
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM015");
    }

    [Fact]
    public async Task RAWMVVM018から020_RelayCommand対象メソッド規約違反を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                [RelayCommand]
                public int SaveCommand()
                {
                    return 0;
                }

                [RelayCommand]
                private Task Load()
                {
                    return Task.CompletedTask;
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM018");
        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM019");
        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM020");
    }

    [Fact]
    public async Task RAWMVVM018から020_適正なRelayCommand対象メソッドは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                [RelayCommand]
                private void Save()
                {
                }

                [RelayCommand]
                private Task Load()
                {
                    return Task.CompletedTask;
                }

                [RelayCommand]
                private ValueTask Sync()
                {
                    return ValueTask.CompletedTask;
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM018");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM019");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM020");
    }

    [Fact]
    public async Task RAWMVVM020_TaskOfTとValueTaskOfTは戻り値違反として検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                [RelayCommand]
                private Task<int> Load()
                {
                    return Task.FromResult(0);
                }

                [RelayCommand]
                private ValueTask<int> Save()
                {
                    return new ValueTask<int>(0);
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAWMVVM020"));
    }

    [Fact]
    public async Task RAWMVVM027_AllowConcurrentExecutionsの日本語コメント不足を検出する()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                [RelayCommand(AllowConcurrentExecutions = true)]
                private async Task Load()
                {
                    await Task.CompletedTask;
                }

                // ユーザー操作の連打を個別処理する必要があるため許可します。
                [RelayCommand(AllowConcurrentExecutions = true)]
                private async Task Save()
                {
                    await Task.CompletedTask;
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Single(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM027");
    }

    [Fact]
    public async Task RAWMVVM027_false指定と同期Commandは検出しない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                [RelayCommand(AllowConcurrentExecutions = false)]
                private async Task Load()
                {
                    await Task.CompletedTask;
                }

                [RelayCommand(AllowConcurrentExecutions = true)]
                private void Save()
                {
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM027");
    }

    [Fact]
    public async Task RAWMVVM027_XMLコメントは理由コメントとして扱わない()
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs("""
            public sealed partial class CustomerViewModel : ObservableObject
            {
                /// <summary>
                /// 並列実行します。
                /// </summary>
                [RelayCommand(AllowConcurrentExecutions = true)]
                private async Task Load()
                {
                    await Task.CompletedTask;
                }
            }
            """);

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM027");
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
