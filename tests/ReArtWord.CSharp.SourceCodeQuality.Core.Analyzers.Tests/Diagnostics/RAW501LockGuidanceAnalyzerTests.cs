using System.Globalization;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW501の型に応じた案内、診断位置および適用範囲を検証します。
/// </summary>
public sealed class RAW501LockGuidanceAnalyzerTests
{
    private const string ScopedLockMessage = "lock文は禁止です。System.Threading.LockのEnterScope()をusing文で使用してください（例: using (gate.EnterScope())）。";
    private const string Net9LockMessage = "lock文は禁止です。.NET9以上ではSystem.Threading.Lockの利用を優先し、非同期要件がある場合はSemaphoreSlimなど用途に合う同期機構を使用してください。";
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
    /// .NET9以降でLock型のフィールドにEnterScopeの案内を報告します。
    /// </summary>
    /// <param name="symbol">対象フレームワークのシンボルです。nullの場合は参照版数で判定します。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("NET9_0")]
    [InlineData("NET9_0_OR_GREATER")]
    [InlineData("NET10_0")]
    [InlineData("NET10_0_OR_GREATER")]
    [InlineData(null)]
    public async Task RAW501_Lock型にはEnterScopeを案内する(string? symbol)
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
        Assert.Equal(ScopedLockMessage, diagnostic.GetMessage(CultureInfo.InvariantCulture));
        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW501", source, expectedStart, "lock");
    }

    /// <summary>
    /// 型の別名や式の書き方にかかわらず、Lock型を意味情報から判定します。
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
    public async Task RAW501_Lock型の別名と式にもEnterScopeを案内する(string expression)
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
        Assert.Equal(ScopedLockMessage, diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Lock型以外や解決できない式では.NET9以降の一般的な案内を維持します。
    /// </summary>
    /// <param name="expression">lock文の対象となる式です。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("new object()")]
    [InlineData("(object)gate")]
    [InlineData("monitorGate")]
    [InlineData("new Custom.Lock()")]
    [InlineData("missingGate")]
    public async Task RAW501_Lock型以外には一般的な案内を維持する(string expression)
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
        Assert.Equal(Net9LockMessage, diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// .NET8では従来の同期機構の案内を維持します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public async Task RAW501_NET8では既存の案内を維持する()
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
            preprocessorSymbols: ["NET8_0"]).ConfigureAwait(true);

        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("RAW501", diagnostic.Id);
        Assert.Equal(
            "lock文は禁止です。.NET8ではSemaphoreSlimなどのモダンな同期機構を使用し、.NET9以上へ移行後はSystem.Threading.Lockを検討してください。",
            diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// EnterScopeをusingで使用するコードには診断を報告しません。
    /// </summary>
    /// <param name="statement">EnterScopeを使用する文です。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("using (gate.EnterScope()) { }")]
    [InlineData("using var scope = gate.EnterScope();")]
    public async Task RAW501_EnterScopeのusingには報告しない(string statement)
    {
        string source = $$"""
            public sealed class Target
            {
                private readonly System.Threading.Lock gate = new();

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
