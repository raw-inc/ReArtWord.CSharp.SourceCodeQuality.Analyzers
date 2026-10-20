using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW430AttributeListSpacingAnalyzerTests
{
    private const string AttributeSpacingMessage =
        "Attribute list同士の間、および最後のAttribute listと修飾対象宣言の間には空行を入れないでください。";

    private const string MemberSpacingMessage =
        "メンバー終端の '}' と次のコメントまたは宣言の間には空行を1行だけ入れてください。";

    [Theory]
    [InlineData("\n", 0, false)]
    [InlineData("\n", 1, true)]
    [InlineData("\n", 2, true)]
    [InlineData("\r\n", 0, false)]
    [InlineData("\r\n", 1, true)]
    [InlineData("\r\n", 2, true)]
    [InlineData("\r", 0, false)]
    [InlineData("\r", 1, true)]
    [InlineData("\r", 2, true)]
    public async Task RAW430_AttributeList間の空行数と改行コードを検証する(
        string newline,
        int blankLineCount,
        bool expectsDiagnostic)
    {
        string separator = newline + string.Concat(Enumerable.Repeat(newline, blankLineCount));
        string source = string.Concat(
            "[A]",
            separator,
            "[B]",
            newline,
            "public sealed class Target",
            newline,
            "{",
            newline,
            "}",
            newline);

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Equal(
            expectsDiagnostic,
            diagnostics.Any(IsAttributeSpacingDiagnostic));
    }

    [Fact]
    public async Task RAW430_最後のAttributeListから宣言までの空行を検出する()
    {
        const string source = """
            [A]
            [B]

            public sealed class Target
            {
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Single(diagnostics, IsAttributeSpacingDiagnostic);
    }

    [Theory]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\r")]
    public async Task RAW430_最後のAttributeListから宣言までの空行を改行コード差分で検出する(string newline)
    {
        string source = string.Concat(
            "[A]",
            newline,
            newline,
            "public sealed class Target",
            newline,
            "{",
            newline,
            "}",
            newline);

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Single(diagnostics, IsAttributeSpacingDiagnostic);
    }

    [Fact]
    public async Task RAW430_明示対象宣言種別のAttribute境界を検出する()
    {
        const string source = """
            using System;

            [A]

            [B]
            public sealed class TargetClass
            {
                [A]

                [B]
                public TargetClass(
                    [A]

                    [B]
                    string constructorInput)
                {
                }

                [A]

                [B]
                public int Value
                {
                    get;
                }

                [A]

                [B]
                private int value;

                [A]

                [B]
                public event EventHandler? Changed;

                [A]

                [B]
                public event EventHandler? Updated
                {
                    add
                    {
                    }
                    remove
                    {
                    }
                }

                [A]

                [B]
                public void Run(
                    [A]

                    [B]
                    string input)
                {
                }
            }

            [A]

            [B]
            public struct TargetStruct
            {
            }

            [A]

            [B]
            public interface TargetInterface
            {
            }

            [A]

            [B]
            public enum TargetEnum
            {
                Value,
            }

            [A]

            [B]
            public sealed record TargetRecord;

            [A]

            [B]
            public delegate void TargetDelegate(
                [A]

                [B]
                string input);
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Equal(15, diagnostics.Count(IsAttributeSpacingDiagnostic));
    }

    [Theory]
    [InlineData("/repo/src/Target.cs", "ReArtWord.SourceCodeQuality.Domain")]
    [InlineData("/repo/tests/Target.cs", "ReArtWord.SourceCodeQuality.Domain.Tests")]
    public async Task RAW430_productionと対象testの双方へ適用する(string filePath, string assemblyName)
    {
        const string source = """
            [A]

            public sealed class Target
            {
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics =
            await AnalyzeAsync(source, filePath, assemblyName);

        Assert.Single(diagnostics, IsAttributeSpacingDiagnostic);
    }

    [Fact]
    public async Task RAW430_単一Listと同一List内の複数Attributeは形式を変更しない()
    {
        const string source = """
            [A]
            public sealed class SingleAttribute
            {
            }

            [A, B]
            public sealed class MultipleAttributesInOneList
            {
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, IsAttributeSpacingDiagnostic);
    }

    [Fact]
    public async Task RAW430_前member終端から後続field最初Attributeまで既存1行規則を維持する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                }
                [A]
                [B]
                private int value;
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Diagnostic diagnostic = Assert.Single(diagnostics, IsMemberSpacingDiagnostic);
        Assert.Equal(MemberSpacingMessage, diagnostic.GetMessage(CultureInfo.InvariantCulture));
        Assert.DoesNotContain(diagnostics, IsAttributeSpacingDiagnostic);
    }

    [Fact]
    public async Task RAW430_前member終端から1空行後のAttribute付きfieldは全境界を満たす()
    {
        const string source = """
            public sealed class Target
            {
                public void Run()
                {
                }

                [A]
                [B]
                private int value;
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW430");
    }

    [Fact]
    public void RAW430_descriptorTitleはmemberとAttribute境界の双方を示す()
    {
        string title = new MemberSpacingAnalyzer()
            .SupportedDiagnostics
            .Single()
            .Title
            .ToString(CultureInfo.InvariantCulture);

        Assert.Equal("メンバー間およびAttribute境界空行ルール違反", title);
    }

    [Fact]
    public async Task RAW430_Attribute違反には専用messageを表示する()
    {
        const string source = """
            [A]

            [B]
            public sealed class Target
            {
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Diagnostic diagnostic = Assert.Single(diagnostics, IsAttributeSpacingDiagnostic);
        Assert.Equal(AttributeSpacingMessage, diagnostic.GetMessage(CultureInfo.InvariantCulture));
    }

    private static bool IsAttributeSpacingDiagnostic(Diagnostic diagnostic)
    {
        return diagnostic.Id == "RAW430"
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Equals(AttributeSpacingMessage, StringComparison.Ordinal);
    }

    private static bool IsMemberSpacingDiagnostic(Diagnostic diagnostic)
    {
        return diagnostic.Id == "RAW430"
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Equals(MemberSpacingMessage, StringComparison.Ordinal);
    }

    private static Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(
        string source,
        string filePath = "/repo/src/Target.cs",
        string assemblyName = "ReArtWord.SourceCodeQuality.Domain")
    {
        return AnalyzerTestHelper.AnalyzeAsync(
            source,
            new MemberSpacingAnalyzer(),
            filePath,
            assemblyName);
    }
}
