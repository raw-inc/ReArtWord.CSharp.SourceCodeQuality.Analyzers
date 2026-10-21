using System.Globalization;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW501の用途に応じた方法選択の案内、診断位置および適用範囲を検証します。
/// </summary>
public sealed class RAW501LockGuidanceAnalyzerTests
{
    private const string ExpectedMessage = "lock文を用いずに、処理の性質や共有資源の扱いに合った排他制御・借用方法を採用することを検討してください（例: Interlockedによる更新、SemaphoreSlimによる同時実行数の制御など）。";
    private const string LockSource = """
        public sealed class Target
        {
            private readonly System.Threading.Lock gate = new();

            public void Run()
            {
                /*診断位置*/lock (gate)
                {
                }
            }
        }
        """;

    /// <summary>
    /// .NET9以降でLock型を使っていても用途に応じた方法の検討を促します。
    /// </summary>
    /// <param name="symbol">対象フレームワークのシンボルです。nullの場合は参照版数で判定します。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("NET9_0")]
    [InlineData("NET9_0_OR_GREATER")]
    [InlineData("NET10_0")]
    [InlineData("NET10_0_OR_GREATER")]
    [InlineData(null)]
    public async Task RAW501_Lock型にも用途に応じた方法の検討を促す(string? symbol)
    {
        string source = DiagnosticLocationAssert.RemoveMarker(LockSource, out int expectedStart);
        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ModernSynchronizationAndFieldAnalyzer(),
            "/repo/src/Target.cs",
            "Sample.Application",
            preprocessorSymbols: symbol is null ? null : [symbol]).ConfigureAwait(true);

        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("RAW501", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
        Assert.Equal("用途に応じた排他制御・借用方法の検討", diagnostic.Descriptor.Title.ToString(CultureInfo.InvariantCulture));
        Assert.Equal(ExpectedMessage, diagnostic.GetMessage(CultureInfo.InvariantCulture));
        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW501", source, expectedStart, "lock");
    }

    /// <summary>
    /// 型の別名や式の書き方によらず、同じ検討を促します。
    /// </summary>
    /// <param name="expression">lock文の対象となる式です。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("gate")]
    [InlineData("this.gate")]
    [InlineData("(gate)")]
    [InlineData("localGate")]
    [InlineData("Gate")]
    [InlineData("GetGate()")]
    public async Task RAW501_対象の式によらず同じ検討を促す(string expression)
    {
        string source = $$"""
            using LockAlias = global::System.Threading.Lock;

            public sealed class Target
            {
                private readonly LockAlias gate = new();
                private LockAlias Gate => gate;

                private LockAlias GetGate() => gate;

                public void Run()
                {
                    var localGate = gate;
                    lock ({{expression}})
                    {
                    }
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ModernSynchronizationAndFieldAnalyzer(),
            "/repo/src/Target.cs",
            "Sample.Application",
            preprocessorSymbols: ["NET9_0"]).ConfigureAwait(true);

        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("RAW501", diagnostic.Id);
        Assert.Equal(ExpectedMessage, diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Lock型以外や解決できない式にも、用途に応じた方法の検討を促します。
    /// </summary>
    /// <param name="expression">lock文の対象となる式です。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("new object()")]
    [InlineData("(object)gate")]
    [InlineData("monitorGate")]
    [InlineData("new Custom.Lock()")]
    [InlineData("missingGate")]
    public async Task RAW501_Lock型以外にも用途に応じた方法の検討を促す(string expression)
    {
        string source = $$"""
            public sealed class Target
            {
                private readonly System.Threading.Lock gate = new();
                private readonly object monitorGate = new System.Threading.Lock();

                public void Run()
                {
                    lock ({{expression}})
                    {
                    }
                }
            }

            namespace Custom
            {
                public sealed class Lock { }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ModernSynchronizationAndFieldAnalyzer(),
            "/repo/src/Target.cs",
            "Sample.Application",
            preprocessorSymbols: ["NET10_0"]).ConfigureAwait(true);

        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("RAW501", diagnostic.Id);
        Assert.Equal(ExpectedMessage, diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// .NET8でも特定APIへの移行を要求せず、用途に応じた方法の検討を促します。
    /// </summary>
    /// <param name="symbol">対象フレームワークのシンボルです。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("NET8_0")]
    [InlineData("NET8_0_OR_GREATER")]
    public async Task RAW501_NET8でも用途に応じた方法の検討を促す(string symbol)
    {
        const string source = """
            public sealed class Target
            {
                private readonly object gate = new();

                public void Run()
                {
                    lock (gate)
                    {
                    }
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ModernSynchronizationAndFieldAnalyzer(),
            "/repo/src/Target.cs",
            "Sample.Application",
            preprocessorSymbols: [symbol]).ConfigureAwait(true);

        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("RAW501", diagnostic.Id);
        Assert.Equal(ExpectedMessage, diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// lock文を含まない処理には診断を報告しません。
    /// </summary>
    /// <param name="statement">lock文を含まない処理です。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("using (gate.EnterScope()) { }")]
    [InlineData("using var scope = gate.EnterScope();")]
    [InlineData("System.Threading.Interlocked.Increment(ref count);")]
    public async Task RAW501_lock文を含まない処理には報告しない(string statement)
    {
        string source = $$"""
            public sealed class Target
            {
                private readonly System.Threading.Lock gate = new();
                private int count;

                public void Run()
                {
                    {{statement}}
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ModernSynchronizationAndFieldAnalyzer(),
            "/repo/src/Target.cs",
            "Sample.Application",
            preprocessorSymbols: ["NET10_0"]).ConfigureAwait(true);

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// テストコードと生成コードではLock型でも診断を報告しません。
    /// </summary>
    /// <param name="filePath">解析ファイルのパスです。</param>
    /// <param name="assemblyName">解析対象のアセンブリ名です。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("/repo/tests/Target.cs", "Sample.Application")]
    [InlineData("/repo/src/Target.cs", "Sample.Tests")]
    [InlineData("/repo/src/Target.g.cs", "Sample.Application")]
    public async Task RAW501_適用範囲外のコードには報告しない(string filePath, string assemblyName)
    {
        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            LockSource,
            new ModernSynchronizationAndFieldAnalyzer(),
            filePath,
            assemblyName,
            preprocessorSymbols: ["NET10_0"]).ConfigureAwait(true);

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// .NET8未満は参照先にLock型があっても診断を報告しません。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public async Task RAW501_NET8未満ではLock型にも報告しない()
    {
        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            LockSource,
            new ModernSynchronizationAndFieldAnalyzer(),
            "/repo/src/Target.cs",
            "Sample.Application",
            preprocessorSymbols: ["NET7_0"]).ConfigureAwait(true);

        Assert.Empty(diagnostics);
    }
}
