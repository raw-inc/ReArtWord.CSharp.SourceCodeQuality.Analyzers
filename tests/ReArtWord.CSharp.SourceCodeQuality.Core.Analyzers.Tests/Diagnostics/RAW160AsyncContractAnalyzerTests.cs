using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW160AsyncContractAnalyzerTests
{
    [Fact]
    public async Task RAW510_最終引数CancellationToken_defaultありは非検出()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public Task RunAsync(int value, CancellationToken cancellationToken = default)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW510");
    }

    [Fact]
    public async Task RAW510_CancellationTokenなしを検出する()
    {
        const string source = """
            using System.Threading.Tasks;

            public sealed class Target
            {
                public Task RunAsync()
                {
                    return Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW510");
    }

    [Fact]
    public async Task RAW510_CancellationTokenが最終引数でない場合を検出する()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public Task RunAsync(CancellationToken cancellationToken = default, int value = 0)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW510");
    }

    [Fact]
    public async Task RAW510_CancellationTokenにdefault指定がない場合を検出する()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public Task RunAsync(CancellationToken cancellationToken)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW510");
    }

    [Fact]
    public async Task RAW510_overrideメソッドは除外する()
    {
        const string source = """
            using System.Threading.Tasks;

            public abstract class Base
            {
                public abstract Task RunAsync();
            }

            public sealed class Target : Base
            {
                public override Task RunAsync()
                {
                    return Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW510");
    }

    [Fact]
    public async Task RAW510_明示インターフェイス実装は除外する()
    {
        const string source = """
            using System.Threading.Tasks;

            public interface ITarget
            {
                Task RunAsync();
            }

            public sealed class Target : ITarget
            {
                Task ITarget.RunAsync()
                {
                    return Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW510");
    }

    [Fact]
    public async Task RAW511_discard代入のfireAndForgetを検出する()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public Task RunAsync(CancellationToken cancellationToken = default)
                {
                    _ = Task.Run(static () => Task.CompletedTask);
                    return Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW511");
    }

    [Fact]
    public async Task RAW511_未await単独呼び出しを検出する()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async Task RunAsync(CancellationToken cancellationToken = default)
                {
                    SendAsync(cancellationToken);
                    await Task.CompletedTask;
                }

                private static Task SendAsync(CancellationToken cancellationToken = default)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW511");
    }

    [Fact]
    public async Task RAW511_awaitまたはreturnは非検出()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async Task RunAsync(CancellationToken cancellationToken = default)
                {
                    await SendAsync(cancellationToken);
                }

                public Task ForwardAsync(CancellationToken cancellationToken = default)
                {
                    return SendAsync(cancellationToken);
                }

                private static Task SendAsync(CancellationToken cancellationToken = default)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW511");
    }

    [Fact]
    public async Task RAW160_NET6以上でpublicAsyncEnumerable戻り値を検出する()
    {
        const string source = """
            using System.Collections.Generic;
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async Task<List<int>> LoadAsync(CancellationToken cancellationToken = default)
                {
                    await Task.Delay(1, cancellationToken);
                    return new List<int>();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW160");
    }

    [Fact]
    public async Task RAW160_NET5では検出しない()
    {
        const string source = """
            using System.Collections.Generic;
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async Task<List<int>> LoadAsync(CancellationToken cancellationToken = default)
                {
                    await Task.Delay(1, cancellationToken);
                    return new List<int>();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET5_0"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW160");
    }

    [Fact]
    public async Task RAW160_string戻り値は対象外()
    {
        const string source = """
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async Task<string> LoadAsync(CancellationToken cancellationToken = default)
                {
                    await Task.Delay(1, cancellationToken);
                    return string.Empty;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW160");
    }

    [Fact]
    public async Task RAW160_RAW510_RAW511_tests配下では検出しない()
    {
        const string source = """
            using System.Collections.Generic;
            using System.Threading;
            using System.Threading.Tasks;

            public sealed class Target
            {
                public async Task<List<int>> LoadAsync()
                {
                    SendAsync();
                    await Task.Delay(1);
                    return new List<int>();
                }

                private static Task SendAsync(CancellationToken cancellationToken = default)
                {
                    return Task.CompletedTask;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new AsyncContractAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW510");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW511");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW160");
    }
}
