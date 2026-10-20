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
    public async Task RAW170_コレクションフィールド識別子へ報告する()
    {
        const string markedSource = """
            using System.Collections.Generic;

            public sealed class Target
            {
                private List<int> /*診断位置*/values = new List<int>();
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new CollectionReadonlyAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW170", source, expectedStart, "values");
    }

    [Fact]
    public async Task RAW171_DateTime型名へ報告する()
    {
        const string markedSource = """
            using System;

            public sealed class Target
            {
                public /*診断位置*/DateTime GetNow()
                {
                    return DateTime.Now;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new DateTimeUsageAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET8_0_OR_GREATER"]);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW171", source, expectedStart, "DateTime");
    }

    [Fact]
    public async Task RAW203_メソッド識別子へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                /// <summary>値を取得します。</summary>
                public int /*診断位置*/GetValue()
                {
                    return 1;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ReturnDocumentationAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW203", source, expectedStart, "GetValue");
    }

    [Fact]
    public async Task RAW340_ArgumentNullException型名へ報告する()
    {
        const string markedSource = """
            using System;

            public sealed class Target
            {
                public void Run(string? value)
                {
                    if (value is null)
                    {
                        throw new /*診断位置*/ArgumentNullException(nameof(value));
                    }
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeGuardAsync(source, ["NET6_0_OR_GREATER"]);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW340", source, expectedStart, "ArgumentNullException");
    }

    [Fact]
    public async Task RAW341_ArgumentException型名へ報告する()
    {
        const string markedSource = """
            using System;

            public sealed class Target
            {
                public void Run(string? value)
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        throw new /*診断位置*/ArgumentException("値が必要です。", nameof(value));
                    }
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeGuardAsync(source, ["NET8_0_OR_GREATER"]);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW341", source, expectedStart, "ArgumentException");
    }

    [Fact]
    public async Task RAW342_ArgumentOutOfRangeException型名へ報告する()
    {
        const string markedSource = """
            using System;

            public sealed class Target
            {
                public void Run(int count)
                {
                    if (count < 0)
                    {
                        throw new /*診断位置*/ArgumentOutOfRangeException(nameof(count));
                    }
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeGuardAsync(source, ["NET9_0_OR_GREATER"]);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW342", source, expectedStart, "ArgumentOutOfRangeException");
    }

    [Fact]
    public async Task RAW460_null比較演算子へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public bool Run(string? value)
                {
                    return value /*診断位置*/== null;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeComparisonAsync(source);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW460", source, expectedStart, "==");
    }

    [Fact]
    public async Task RAW461_文字列比較演算子へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public bool Run(string left, string right)
                {
                    return left /*診断位置*/== right;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzeComparisonAsync(source);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW461", source, expectedStart, "==");
    }

    [Fact]
    public async Task RAW612_変数識別子へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public void Run()
                {
                    int /*診断位置*/local_value = 1;
                    _ = local_value;
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics = await AnalyzeNamingAsync(source);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW612", source, expectedStart, "local_value");
    }

    [Fact]
    public async Task RAW613_メソッド識別子へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public void /*診断位置*/Run_test()
                {
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics = await AnalyzeNamingAsync(source);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW613", source, expectedStart, "Run_test");
    }

    [Fact]
    public async Task RAW614_引数識別子へ報告する()
    {
        const string markedSource = """
            public sealed class Target
            {
                public void Run(string /*診断位置*/input_value)
                {
                }
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics = await AnalyzeNamingAsync(source);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW614", source, expectedStart, "input_value");
    }

    [Fact]
    public async Task RAW615_enum識別子へ報告する()
    {
        const string markedSource = """
            public enum /*診断位置*/Status
            {
                None
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics = await AnalyzeNamingAsync(source);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW615", source, expectedStart, "Status");
    }

    [Fact]
    public async Task RAW616_interface識別子へ報告する()
    {
        const string markedSource = """
            public interface /*診断位置*/Worker
            {
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics = await AnalyzeNamingAsync(source);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW616", source, expectedStart, "Worker");
    }

    [Fact]
    public async Task RAW617_型識別子へ報告する()
    {
        const string markedSource = """
            public sealed class /*診断位置*/target_type
            {
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics = await AnalyzeNamingAsync(source);

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW617", source, expectedStart, "target_type");
    }

    private static async Task<IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic>> AnalyzeGuardAsync(
        string source,
        IReadOnlyCollection<string> preprocessorSymbols)
    {
        return await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ArgumentGuardClauseAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain",
            preprocessorSymbols: preprocessorSymbols).ConfigureAwait(false);
    }

    private static async Task<IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic>> AnalyzeComparisonAsync(string source)
    {
        return await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new NullAndStringComparisonAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain").ConfigureAwait(false);
    }

    private static async Task<IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic>> AnalyzeNamingAsync(string source)
    {
        return await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new NamingConventionAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain").ConfigureAwait(false);
    }
}
