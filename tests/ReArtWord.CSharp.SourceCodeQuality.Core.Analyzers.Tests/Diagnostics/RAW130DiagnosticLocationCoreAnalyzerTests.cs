using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed partial class RAW130DiagnosticLocationCoreAnalyzerTests
{
    [Fact]
    public async Task RAW130_上限超過起点のpartial識別子へ報告する()
    {
        const string markedSource = """
            public sealed partial class Target
            {
            }
            public sealed partial class Target
            {
            }
            public sealed partial class Target
            {
            }
            public sealed partial class Target
            {
            }
            public sealed partial class /*診断位置*/Target
            {
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new PartialClassDeclarationLimitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW130", source, expectedStart, "Target");
    }

    [Fact]
    public async Task RAW140_Disposeメンバー名へ報告する()
    {
        const string markedSource = """
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
                    stream./*診断位置*/Dispose();
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DisposableUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW140", source, expectedStart, "Dispose");
    }

    [Fact]
    public async Task RAW201_summary欠落は宣言識別子へ報告する()
    {
        const string markedSource = """
            public sealed class /*診断位置*/Target
            {
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SummaryDocumentationAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW201", source, expectedStart, "Target");
    }

    [Fact]
    public async Task RAW202_summary日本語不足は本文へ報告する()
    {
        const string markedSource = """
            /// <summary>/*診断位置*/Target class summary.</summary>
            public sealed class Target
            {
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SummaryDocumentationAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW202", source, expectedStart, "Target class summary.");
    }

    [Fact]
    public async Task RAW310_Justification不足はSuppressMessage属性名へ報告する()
    {
        const string markedSource = """
            using System.Diagnostics.CodeAnalysis;

            // 抑制理由: 互換性維持のためです。
            [/*診断位置*/SuppressMessage("Reliability", "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください")]
            public sealed class Target
            {
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW310", source, expectedStart, "SuppressMessage");
    }

    [Fact]
    public async Task RAW310_Justification空値はJustification引数へ報告する()
    {
        const string markedSource = """
            using System.Diagnostics.CodeAnalysis;

            // 抑制理由: 互換性維持のためです。
            [SuppressMessage(
                "Reliability",
                "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
                /*診断位置*/Justification = "")]
            public sealed class Target
            {
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW310", source, expectedStart, "Justification = \"\"");
    }

    [Fact]
    public async Task RAW311_抑制理由コメント不足はSuppressMessage属性名へ報告する()
    {
        const string markedSource = """
            using System.Diagnostics.CodeAnalysis;

            [/*診断位置*/SuppressMessage(
                "Reliability",
                "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
                Justification = "互換性維持のためです。")]
            public sealed class Target
            {
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW311", source, expectedStart, "SuppressMessage");
    }

    [Fact]
    public async Task RAW321_NotImplementedException型参照へ報告する()
    {
        const string markedSource = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    throw new /*診断位置*/NotImplementedException();
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CommentDirectiveGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW321", source, expectedStart, "NotImplementedException");
    }

    [Fact]
    public async Task RAW410_constキーワードへ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                private /*診断位置*/const int MaxCount = 10;
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ConstAndAwaitAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW410", source, expectedStart, "const");
    }
}
