using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// 参照先のAPIに応じた文字列比較の診断を検証します。
/// </summary>
// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW461StringComparisonCultureAnalyzerTests
{
    private const string ComparisonOverload = "public bool Contains(string value, StringComparison comparisonType) => false;";

    /// <summary>
    /// 比較方法を指定するAPIの有無と、名前付き引数の診断位置を確認します。
    /// </summary>
    /// <param name="supportsComparison">比較方法を指定するAPIの有無です。</param>
    /// <param name="usesNamedArgument">名前付き引数を使用するかどうかです。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task RAW461_Containsは比較用APIがある場合のみ検出する(bool supportsComparison, bool usesNamedArgument)
    {
        string argument = usesNamedArgument ? "value: \"A\"" : "\"A\"";
        string source = $$"""
            public sealed class Target
            {
                public bool Run(string value) => value./*診断位置*/Contains({{argument}});
            }
            """;
        source = DiagnosticLocationAssert.RemoveMarker(source, out int expectedStart);

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source, supportsComparison ? ComparisonOverload : string.Empty);

        if (supportsComparison)
        {
            Diagnostic diagnostic = Assert.Single(diagnostics);
            Assert.Equal(DiagnosticSeverity.Error, diagnostic.Severity);
            DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW461", source, expectedStart, "Contains");
        }
        else
        {
            Assert.Empty(diagnostics);
        }
    }

    /// <summary>
    /// 引数の型やアクセス範囲が異なるAPIを比較用APIと誤認しないことを確認します。
    /// </summary>
    /// <param name="overload">追加するオーバーロードです。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("public bool Contains(char value, StringComparison comparisonType) => false;")]
    [InlineData("public bool Contains(string value, int comparisonType) => false;")]
    [InlineData("private bool Contains(string value, StringComparison comparisonType) => false;")]
    public async Task RAW461_利用できないオーバーロードは比較用APIとみなさない(string overload)
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(string value) => value.Contains("A");
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source, overload);

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// 比較方法を指定したContainsを診断しないことを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public async Task RAW461_比較方法を指定したContainsは検出しない()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(string value) => value.Contains("A", System.StringComparison.Ordinal);
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source, ComparisonOverload);

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// Containsの比較用APIがなくても、ほかの文字列比較とnull比較は検査することを確認します。
    /// </summary>
    /// <param name="expression">検査する比較式です。</param>
    /// <param name="diagnosticId">期待する診断IDです。</param>
    /// <returns>非同期テスト処理です。</returns>
    [Theory]
    [InlineData("value.StartsWith(\"A\")", "RAW461")]
    [InlineData("value.EndsWith(\"A\")", "RAW461")]
    [InlineData("value.IndexOf(\"A\") >= 0", "RAW461")]
    [InlineData("value.LastIndexOf(\"A\") >= 0", "RAW461")]
    [InlineData("value.Equals(\"A\")", "RAW461")]
    [InlineData("string.Compare(value, \"A\") == 0", "RAW461")]
    [InlineData("value == \"A\"", "RAW461")]
    [InlineData("value != \"A\"", "RAW461")]
    [InlineData("value == null", "RAW460")]
    public async Task RAW461_Containsの比較用APIがなくてもほかの比較を検出する(string expression, string diagnosticId)
    {
        string source = $$"""
            public sealed class Target
            {
                public bool Run(string value) => {{expression}};
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Equal(diagnosticId, Assert.Single(diagnostics).Id);
    }

    /// <summary>
    /// 文字を検索する呼び出しと、別の型のContainsを診断しないことを確認します。
    /// </summary>
    /// <returns>非同期テスト処理です。</returns>
    [Fact]
    public async Task RAW461_文字引数と別の型のContainsは検出しない()
    {
        const string source = """
            public sealed class Target
            {
                public bool Run(string value) => value.Contains('A') || Contains("A");
                public bool Contains(string value) => false;
            }
            """;

        ImmutableArray<Diagnostic> diagnostics = await AnalyzeAsync(source, ComparisonOverload);

        Assert.Empty(diagnostics);
    }

    /// <summary>
    /// APIを絞った参照アセンブリを使用し、C# 7.3でコンパイルできるコードの診断を取得します。
    /// </summary>
    /// <param name="source">解析対象のソースです。</param>
    /// <param name="additionalMembers">参照先のstring型に追加するメンバーです。</param>
    /// <returns>アナライザーの診断です。</returns>
    private static async Task<ImmutableArray<Diagnostic>> AnalyzeAsync(string source, string additionalMembers = "")
    {
        string referenceSource = $$"""
            namespace System
            {
                public class Object { }
                public abstract class Delegate { }
                public abstract class ValueType { }
                public abstract class Enum : ValueType { }
                public struct Void { }
                public struct Boolean { }
                public struct Int32 { }
                public struct Char { }
                public enum StringComparison { Ordinal }
                public sealed class String
                {
                    public bool Contains(string value) => false;
                    public bool Contains(char value) => false;
                    public bool StartsWith(string value) => false;
                    public bool EndsWith(string value) => false;
                    public int IndexOf(string value) => 0;
                    public int LastIndexOf(string value) => 0;
                    public bool Equals(string value) => false;
                    public static int Compare(string left, string right) => 0;
                    {{additionalMembers}}
                }
            }
            """;
        CSharpParseOptions parseOptions = new(LanguageVersion.CSharp7_3);
        CSharpCompilationOptions compilationOptions = new(OutputKind.DynamicallyLinkedLibrary);
        CSharpCompilation referenceCompilation = CSharpCompilation.Create(
            "StringComparisonReference",
            [CSharpSyntaxTree.ParseText(referenceSource, parseOptions)],
            options: compilationOptions);
        Assert.Empty(referenceCompilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        CSharpCompilation compilation = CSharpCompilation.Create(
            "Application",
            [CSharpSyntaxTree.ParseText(source, parseOptions, "/repo/src/Target.cs")],
            [referenceCompilation.ToMetadataReference()],
            compilationOptions);
        Assert.Empty(compilation.GetDiagnostics().Where(static diagnostic => diagnostic.Severity == DiagnosticSeverity.Error));

        return await compilation.WithAnalyzers([new NullAndStringComparisonAnalyzer()])
            .GetAnalyzerDiagnosticsAsync().ConfigureAwait(false);
    }
}
