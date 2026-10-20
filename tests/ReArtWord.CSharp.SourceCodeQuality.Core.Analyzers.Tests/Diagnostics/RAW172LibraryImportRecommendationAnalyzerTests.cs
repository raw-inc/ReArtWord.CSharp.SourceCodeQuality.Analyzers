using System.Globalization;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// RAW172の検出条件、適用範囲および診断位置を検証します。
/// </summary>
public sealed class RAW172LibraryImportRecommendationAnalyzerTests
{
    private const string DllImportSource = """
        using System.Runtime.InteropServices;

        public static class NativeMethods
        {
            [DllImport("native")]
            public static extern int GetValue();
        }
        """;

    /// <summary>
    /// 診断ID、既定有効状態、重大度およびカテゴリを検証します。
    /// </summary>
    [Fact]
    public void RAW172_既定有効の設計警告として公開する()
    {
        DiagnosticDescriptor descriptor = Assert.Single(new LibraryImportRecommendationAnalyzer().SupportedDiagnostics);

        Assert.Equal("RAW172", descriptor.Id);
        Assert.Equal("設計", descriptor.Category);
        Assert.Equal(DiagnosticSeverity.Warning, descriptor.DefaultSeverity);
        Assert.True(descriptor.IsEnabledByDefault);
    }

    /// <summary>
    /// .NET10以降のシンボルで移行検討の警告を検証します。
    /// </summary>
    /// <param name="symbol">対象フレームワークのシンボルです。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("NET10_0")]
    [InlineData("NET10_0_OR_GREATER")]
    [InlineData("NET11_0")]
    [InlineData("NET11_0_OR_GREATER")]
    public async Task RAW172_NET10以降で検出する(string symbol)
    {
        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(DllImportSource, [symbol]).ConfigureAwait(true);

        Diagnostic diagnostic = Assert.Single(diagnostics);
        Assert.Equal("RAW172", diagnostic.Id);
        Assert.Equal(DiagnosticSeverity.Warning, diagnostic.Severity);
        Assert.Equal(".NET10以降ではDllImportの利用を避け、LibraryImportへの移行を検討してください。", diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// 実行環境が.NET10でも、対象シンボルが旧フレームワークなら診断しません。
    /// </summary>
    /// <param name="symbol">対象フレームワークのシンボルです。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("NET9_0")]
    [InlineData("NET9_0_OR_GREATER")]
    [InlineData("NET8_0")]
    [InlineData("NET7_0")]
    [InlineData("NET6_0")]
    [InlineData("NETCOREAPP3_1")]
    [InlineData("NET40")]
    [InlineData("NET472")]
    [InlineData("NET48")]
    [InlineData("NET48_OR_GREATER")]
    [InlineData("NET481")]
    [InlineData("NETFRAMEWORK")]
    [InlineData("NETSTANDARD2_0")]
    public async Task RAW172_NET10未満は検出しない(string symbol)
    {
        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(DllImportSource, [symbol]).ConfigureAwait(true);

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// SDKが定義する互換性シンボルやOSシンボルが併存しても検出します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public async Task RAW172_複数シンボルの最大バージョンで判定する()
    {
        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(
            DllImportSource,
            ["NET", "NETCOREAPP", "NET9_0_OR_GREATER", "NET10_0", "NET10_0_OR_GREATER", "WINDOWS"])
            .ConfigureAwait(true);

        Assert.Equal("RAW172", Assert.Single(diagnostics).Id);
    }

    /// <summary>
    /// シンボルがない場合は参照System.Runtimeのバージョンで検出します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public async Task RAW172_NETシンボルがなければ参照アセンブリで判定する()
    {
        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(DllImportSource, []).ConfigureAwait(true);

        Assert.Equal("RAW172", Assert.Single(diagnostics).Id);
    }

    /// <summary>
    /// 属性の表記差にかかわらず使用箇所の末尾識別子だけを診断します。
    /// </summary>
    /// <param name="attributeName">診断位置マーカー付きの属性名です。</param>
    /// <param name="expectedText">診断対象となる識別子です。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("/*診断位置*/DllImport", "DllImport")]
    [InlineData("/*診断位置*/DllImportAttribute", "DllImportAttribute")]
    [InlineData("System.Runtime.InteropServices./*診断位置*/DllImport", "DllImport")]
    [InlineData("global::System.Runtime.InteropServices./*診断位置*/DllImportAttribute", "DllImportAttribute")]
    [InlineData("/*診断位置*/NativeImport", "NativeImport")]
    [InlineData("Interop./*診断位置*/DllImport", "DllImport")]
    [InlineData("Interop::/*診断位置*/DllImport", "DllImport")]
    public async Task RAW172_属性名の表記差と診断位置を検証する(string attributeName, string expectedText)
    {
        ArgumentNullException.ThrowIfNull(expectedText);

        string markedSource = $$"""
            using System.Runtime.InteropServices;
            using NativeImport = System.Runtime.InteropServices.DllImportAttribute;
            using Interop = System.Runtime.InteropServices;

            public static class NativeMethods
            {
                [{{attributeName}}("native")]
                public static extern int GetValue();
            }
            """;

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source, ["NET10_0"]).ConfigureAwait(true);

        Assert.Single(diagnostics);
        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW172", source, expectedStart, expectedText);
    }

    /// <summary>
    /// メソッドとローカル関数のDllImportをそれぞれ一度だけ診断します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public async Task RAW172_複数のPInvoke宣言を個別に検出する()
    {
        const string source = """
            using System;
            using System.Runtime.InteropServices;

            public static class NativeMethods
            {
                [Obsolete, DllImport("native")]
                public static extern int First();

                public static int GetValue()
                {
                    [DllImport("native")]
                    static extern int Second();

                    return Second();
                }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source, ["NET10_0"]).ConfigureAwait(true);

        Assert.Equal(2, diagnostics.Count);
        Assert.All(diagnostics, static diagnostic => Assert.Equal("RAW172", diagnostic.Id));
    }

    /// <summary>
    /// LibraryImport宣言とその生成実装内のDllImportは診断しません。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public async Task RAW172_LibraryImportと生成されたDllImportは検出しない()
    {
        Dictionary<string, string> sources = new()
        {
            ["/repo/src/NativeMethods.cs"] = """
                using System.Runtime.InteropServices;

                public static partial class NativeMethods
                {
                    [LibraryImport("native")]
                    public static partial int GetValue();
                }
                """,
            ["/repo/obj/NativeMethods.g.cs"] = """
                // <auto-generated/>
                using System.Runtime.InteropServices;

                public static partial class NativeMethods
                {
                    public static partial int GetValue()
                    {
                        [DllImport("native")]
                        static extern int NativeGetValue();

                        return NativeGetValue();
                    }
                }
                """,
        };

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            sources,
            new LibraryImportRecommendationAnalyzer(),
            "Sample.Application",
            preprocessorSymbols: ["NET10_0"]).ConfigureAwait(true);

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// 同名の独自属性を名前だけで誤検出しません。
    /// </summary>
    /// <param name="attributeNamespace">独自属性の名前空間です。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("Custom")]
    [InlineData("System.Runtime.InteropServices")]
    public async Task RAW172_同名の独自属性は検出しない(string attributeNamespace)
    {
        string source = $$"""
            namespace {{attributeNamespace}}
            {
                public sealed class DllImportAttribute : System.Attribute
                {
                    public DllImportAttribute(string library) { }
                }
            }

            public static class NativeMethods
            {
                [{{attributeNamespace}}.DllImport("native")]
                public static void Run() { }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source, ["NET10_0"]).ConfigureAwait(true);

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// 編集途中の解決不能な属性を誤検出しません。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public async Task RAW172_解決できない属性は検出しない()
    {
        const string source = """
            public static class NativeMethods
            {
                [DllImport("native")]
                public static extern int GetValue();
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source, ["NET10_0"]).ConfigureAwait(true);

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// テストコードおよび生成コードを適用範囲から除外します。
    /// </summary>
    /// <param name="filePath">解析ファイルのパスです。</param>
    /// <param name="assemblyName">解析対象のアセンブリ名です。</param>
    /// <param name="prefix">生成コードを示す先頭コメントです。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("/repo/tests/NativeMethods.cs", "Sample.Application", "")]
    [InlineData("/repo/src/NativeMethods.cs", "Sample.Tests", "")]
    [InlineData("/repo/src/NativeMethods.cs", "ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests", "")]
    [InlineData("/repo/obj/NativeMethods.cs", "Sample.Application", "")]
    [InlineData("/repo/src/NativeMethods.g.cs", "Sample.Application", "")]
    [InlineData("/repo/src/NativeMethods.g.i.cs", "Sample.Application", "")]
    [InlineData("/repo/src/NativeMethods.designer.cs", "Sample.Application", "")]
    [InlineData("/repo/src/NativeMethods.generated.cs", "Sample.Application", "")]
    [InlineData("/repo/src/NativeMethods.cs", "Sample.Application", "// <auto-generated/>\n")]
    public async Task RAW172_適用範囲外のコードは検出しない(string filePath, string assemblyName, string prefix)
    {
        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            prefix + DllImportSource,
            new LibraryImportRecommendationAnalyzer(),
            filePath,
            assemblyName,
            preprocessorSymbols: ["NET10_0"]).ConfigureAwait(true);

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// 指定フレームワークの本番コードとして解析します。
    /// </summary>
    /// <param name="source">解析対象のソースです。</param>
    /// <param name="symbols">対象フレームワークのシンボルです。</param>
    /// <returns>Analyzer診断一覧です。</returns>
    private static Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(string source, IReadOnlyCollection<string> symbols)
    {
        return AnalyzerTestHelper.AnalyzeAsync(
            source,
            new LibraryImportRecommendationAnalyzer(),
            "/repo/src/NativeMethods.cs",
            "Sample.Application",
            preprocessorSymbols: symbols);
    }
}
