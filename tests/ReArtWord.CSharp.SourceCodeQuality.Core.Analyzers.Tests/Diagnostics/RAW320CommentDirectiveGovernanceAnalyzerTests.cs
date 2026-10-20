using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW320CommentDirectiveGovernanceAnalyzerTests
{
    [Fact]
    public async Task RAW320_TODO日本語理由ありは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    // TODO: 仕様確定後に実装手順を更新する
                    int value = 1;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW320");
    }

    [Fact]
    public async Task RAW320_TODO理由なしを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    // TODO:
                    int value = 1;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW320");
    }

    [Fact]
    public async Task RAW320_TODOプレフィックスなしを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    // TODO 実装方針を詰める
                    int value = 1;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW320");
    }

    [Fact]
    public async Task RAW320_TODO英語のみを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    // TODO: update later
                    int value = 1;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW320");
    }

    [Fact]
    public async Task RAW320_tests配下でも検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                    // FIXME:
                    int value = 1;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentDirectiveGovernanceAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW320");
    }

    [Fact]
    public async Task RAW321_NotImplementedExceptionにTODOコメントがない場合を検出する()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    throw new NotImplementedException();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW321");
    }

    [Fact]
    public async Task RAW321_NotImplementedExceptionにTODOコメントがある場合は非検出()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    // TODO: 実装方針が確定したら処理を追加する
                    throw new NotImplementedException();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW321");
    }

    [Fact]
    public async Task RAW321_throw式でも検出する()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public int Run() => throw new NotImplementedException();
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW321");
    }

    [Fact]
    public async Task RAW321_tests配下でも検出する()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    throw new NotImplementedException();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentDirectiveGovernanceAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW321");
    }
}
