using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW140DisposableUsageAnalyzerTests
{
    [Fact]
    public async Task RAW140_通常メソッド内のDispose呼び出しを検出する()
    {
        const string source = """
            using System;
            using System.IO;

            public sealed class Target : IDisposable
            {
                public void Dispose()
                {
                }

                public void Run()
                {
                    MemoryStream stream = new MemoryStream();
                    stream.Dispose();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DisposableUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW140");
    }

    [Fact]
    public async Task RAW140_通常メソッド内のDisposeAsync呼び出しを検出する()
    {
        const string source = """
            using System;
            using System.IO;
            using System.Threading.Tasks;

            public sealed class Target : IAsyncDisposable
            {
                public ValueTask DisposeAsync()
                {
                    return ValueTask.CompletedTask;
                }

                public async Task RunAsync()
                {
                    MemoryStream stream = new MemoryStream();
                    await stream.DisposeAsync();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DisposableUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW140");
    }

    [Fact]
    public async Task RAW140_Dispose系メソッド内は非検出()
    {
        const string source = """
            using System;
            using System.IO;

            public sealed class Target : IDisposable
            {
                public void Dispose()
                {
                    MemoryStream stream = new MemoryStream();
                    stream.Dispose();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DisposableUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW140");
    }

    [Fact]
    public async Task RAW140とRAW141_明示実装Dispose内は非検出()
    {
        const string source = """
            using System;
            using System.IO;

            public sealed class Target : IDisposable
            {
                void IDisposable.Dispose()
                {
                    MemoryStream stream = new MemoryStream();
                    stream.Dispose();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DisposableUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW140");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW141");
    }

    [Fact]
    public async Task RAW141_nonUsingローカル宣言を検出する()
    {
        const string source = """
            using System;
            using System.IO;

            public sealed class Target : IDisposable
            {
                public void Dispose()
                {
                }

                public void Run()
                {
                    MemoryStream stream = new MemoryStream();
                    _ = stream.Length;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DisposableUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW141");
    }

    [Fact]
    public async Task RAW141_using句_using宣言_awaitUsingは非検出()
    {
        const string source = """
            using System;
            using System.IO;
            using System.Threading.Tasks;

            public sealed class Target : IDisposable, IAsyncDisposable
            {
                public void Dispose()
                {
                }

                public ValueTask DisposeAsync()
                {
                    return ValueTask.CompletedTask;
                }

                public async Task RunAsync()
                {
                    using MemoryStream usingDeclaration = new MemoryStream();
                    await using MemoryStream awaitUsingDeclaration = new MemoryStream();
                    using (MemoryStream scoped = new MemoryStream())
                    {
                        _ = scoped.Length;
                    }

                    _ = usingDeclaration.Length;
                    _ = awaitUsingDeclaration.Length;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DisposableUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW141");
    }

    [Fact]
    public async Task RAW140とRAW141_NET5では検出しない()
    {
        const string source = """
            using System;
            using System.IO;

            public sealed class Target : IDisposable
            {
                public void Dispose()
                {
                }

                public void Run()
                {
                    MemoryStream stream = new MemoryStream();
                    stream.Dispose();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DisposableUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET5_0"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW140");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW141");
    }

    [Fact]
    public async Task RAW140とRAW141_Disposable未実装クラスは対象外()
    {
        const string source = """
            using System.IO;

            public sealed class Target
            {
                public void Run()
                {
                    MemoryStream stream = new MemoryStream();
                    stream.Dispose();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DisposableUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW140");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW141");
    }

    [Fact]
    public async Task RAW140とRAW141_tests配下では検出しない()
    {
        const string source = """
            using System;
            using System.IO;

            public sealed class Target : IDisposable
            {
                public void Dispose()
                {
                }

                public void Run()
                {
                    MemoryStream stream = new MemoryStream();
                    stream.Dispose();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DisposableUsageAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW140");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW141");
    }
}
