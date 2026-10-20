using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW501ModernSynchronizationAndFieldAnalyzerTests
{
    [Fact]
    public async Task RAW501_NET8以上でlock文を検出する()
    {
        const string source = """
            public sealed class Target
            {
                private readonly object gate = new object();

                public void Run()
                {
                    lock (gate)
                    {
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ModernSynchronizationAndFieldAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET8_0_OR_GREATER"]);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW501");
    }

    [Fact]
    public async Task RAW501_NET9以上ではLock推奨文言を含む()
    {
        const string source = """
            public sealed class Target
            {
                private readonly object gate = new object();

                public void Run()
                {
                    lock (gate)
                    {
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ModernSynchronizationAndFieldAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET9_0_OR_GREATER"]);

        Assert.Contains(diagnostics, static diagnostic =>
            diagnostic.Id == "RAW501"
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Contains("System.Threading.Lock", StringComparison.Ordinal));
    }

    [Fact]
    public async Task RAW501_NET5では検出しない()
    {
        const string source = """
            public sealed class Target
            {
                private readonly object gate = new object();

                public void Run()
                {
                    lock (gate)
                    {
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ModernSynchronizationAndFieldAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET5_0"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW501");
    }

    [Fact]
    public async Task RAW102_NET6以上でpublicフィールドを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public int Count;
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ModernSynchronizationAndFieldAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW102");
    }

    [Fact]
    public async Task RAW102_NET6でpublicフィールドを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public int Count;
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ModernSynchronizationAndFieldAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0"]);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW102");
    }

    [Fact]
    public async Task RAW102_privateフィールドは非検出()
    {
        const string source = """
            public sealed class Target
            {
                private int count;
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ModernSynchronizationAndFieldAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW102");
    }

    [Fact]
    public async Task RAW102_constフィールドは例外として非検出()
    {
        const string source = """
            public sealed class Target
            {
                public const int MaxCount = 10;
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ModernSynchronizationAndFieldAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW102");
    }

    [Fact]
    public async Task RAW102_StructLayout型は例外として非検出()
    {
        const string source = """
            using System.Runtime.InteropServices;

            [StructLayout(LayoutKind.Sequential)]
            public struct NativeData
            {
                public int Value;
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ModernSynchronizationAndFieldAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET6_0_OR_GREATER"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW102");
    }

    [Fact]
    public async Task RAW102_NET5では検出しない()
    {
        const string source = """
            public sealed class Target
            {
                public int Count;
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ModernSynchronizationAndFieldAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                preprocessorSymbols: ["NET5_0"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW102");
    }

    [Fact]
    public async Task RAW102_シンボル未指定時は参照版数で判定する()
    {
        const string source = """
            public sealed class Target
            {
                public int Count;
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ModernSynchronizationAndFieldAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW102");
    }

    [Fact]
    public async Task RAW501とRAW102_tests配下では検出しない()
    {
        const string source = """
            public sealed class Target
            {
                public int Count;
                private readonly object gate = new object();

                public void Run()
                {
                    lock (gate)
                    {
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new ModernSynchronizationAndFieldAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests",
                preprocessorSymbols: ["NET8_0_OR_GREATER"]);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW501");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW102");
    }
}
