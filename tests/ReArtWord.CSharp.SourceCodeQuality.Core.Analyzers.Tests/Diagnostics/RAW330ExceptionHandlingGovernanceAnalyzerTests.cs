using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed partial class RAW330ExceptionHandlingGovernanceAnalyzerTests
{
    [Fact]
    public async Task RAW330_throw再スローを警告として検出する()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    try
                    {
                        Work();
                    }
                    catch (InvalidOperationException)
                    {
                        throw;
                    }
                }

                private static void Work()
                {
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Diagnostic diagnostic = Assert.Single(diagnostics, static diagnostic => diagnostic.Id == "RAW330");
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        string message = diagnostic.GetMessage(CultureInfo.InvariantCulture);
        Assert.Contains("SuppressMessage", message, StringComparison.Ordinal);
        Assert.Contains("本警告", message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RAW331_汎用Exception直接throwを検出する()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public string Run(string? value)
                {
                    if (value is null)
                    {
                        throw new Exception("invalid");
                    }

                    return value ?? throw new System.Exception("invalid");
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAW331"));
    }

    [Fact]
    public async Task RAW331_具体例外型は検出しない()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public string Run(string? value)
                {
                    if (value is null)
                    {
                        throw new InvalidOperationException("invalid");
                    }

                    return value ?? throw new ArgumentException("invalid", nameof(value));
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW331");
    }

    [Fact]
    public async Task RAW332_catch変数の再スローを検出する()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    try
                    {
                        Work();
                    }
                    catch (InvalidOperationException ex)
                    {
                        throw ex;
                    }
                }

                private static void Work()
                {
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW332");
    }

    [Fact]
    public async Task RAW332_catch変数以外は再スローとして検出しない()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    InvalidOperationException other = new InvalidOperationException("other");
                    try
                    {
                        Work();
                    }
                    catch (InvalidOperationException ex)
                    {
                        _ = ex;
                        throw other;
                    }
                }

                private static void Work()
                {
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW332");
    }

    [Fact]
    public async Task RAW333_catch内の式付きthrowを検出する()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    try
                    {
                        Work();
                    }
                    catch (InvalidOperationException)
                    {
                        throw new InvalidOperationException("wrapped");
                    }
                }

                public string Convert()
                {
                    try
                    {
                        Work();
                    }
                    catch (InvalidOperationException)
                    {
                        return GetValue() ?? throw new InvalidOperationException("fallback");
                    }

                    return string.Empty;
                }

                private static string? GetValue() => null;

                private static void Work()
                {
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Equal(2, diagnostics.Count(static diagnostic => diagnostic.Id == "RAW333"));
    }

    [Fact]
    public async Task RAW333_throw再スローとthrowExには重複しない()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    try
                    {
                        Work();
                    }
                    catch (InvalidOperationException ex)
                    {
                        if (ex.Message.Length == 0)
                        {
                            throw;
                        }

                        throw ex;
                    }
                }

                private static void Work()
                {
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW330");
        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW332");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW333");
    }

    [Fact]
    public async Task RAW333_LocalFunctionとLambda内のthrowはcatch直下として扱わない()
    {
        const string source = """
            using System;

            public sealed class Target
            {
                public void Run()
                {
                    try
                    {
                        Work();
                    }
                    catch (InvalidOperationException)
                    {
                        void Local() => throw new InvalidOperationException("local");
                        Func<string> factory = () => throw new InvalidOperationException("lambda");
                        Local();
                        _ = factory();
                    }
                }

                private static void Work()
                {
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW333");
    }

    private static async Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(string source)
    {
        return await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new ExceptionHandlingGovernanceAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain").ConfigureAwait(false);
    }
}
