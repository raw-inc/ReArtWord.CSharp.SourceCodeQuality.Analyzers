using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW430AttributeListSpacingAnalyzerBoundaryTests
{
    private const string AttributeSpacingMessage =
        "Attribute list同士の間、および最後のAttribute listと修飾対象宣言の間には空行を入れないでください。";

    [Fact]
    public async Task RAW430_未指定ownerへAttribute空行規則を拡張しない()
    {
        const string source = """
            using System;

            [assembly: A]

            [assembly: B]
            [module: A]

            [module: B]

            public sealed class NonTargets<[A]

                [B] T>
            {
                public int Value
                {
                    [A]

                    [B]
                    get;
                }

                [A]

                [B]
                public static NonTargets<T> operator +(
                    [A]

                    [B]
                    NonTargets<T> left,
                    NonTargets<T> right) => left;

                [A]

                [B]
                public static explicit operator int(
                    [A]

                    [B]
                    NonTargets<T> value) => 0;

                public int this[
                    [A]

                    [B]
                    int index] => 0;

                public void Run()
                {
                    [A]

                    [B]
                    void Local(
                        [A]

                        [B]
                        int input)
                    {
                    }

                    Func<int, int> lambda = (
                        [A]

                        [B]
                        int input) => input;
                }
            }

            public sealed class PrimaryConstructor(
                [A]

                [B]
                int input)
            {
            }

            public enum NonTargetEnum
            {
                [A]

                [B]
                Value,
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzerTestHelper.AnalyzeAsync(
            source,
            new MemberSpacingAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, IsAttributeSpacingDiagnostic);
    }

    [Fact]
    public async Task RAW430_commentとpreprocessor行だけなら空行として診断しない()
    {
        const string source = """
            [A]
            // Attribute list境界の説明です。
            [B]
            public sealed class CommentBetweenLists
            {
            }

            [A]
            [B]
            // 宣言境界の説明です。
            public sealed class CommentBeforeDeclaration
            {
            }

            [A]
            #if FEATURE
            #endif
            [B]
            public sealed class DirectiveBetweenLists
            {
            }

            [A]
            [B]
            #if FEATURE
            #endif
            public sealed class DirectiveBeforeDeclaration
            {
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, IsAttributeSpacingDiagnostic);
    }

    [Fact]
    public async Task RAW430_commentとpreprocessorの前にある空行だけを診断する()
    {
        const string source = """
            [A]

            // Attribute list境界の説明です。
            [B]
            public sealed class CommentBetweenLists
            {
            }

            [A]
            [B]

            // 宣言境界の説明です。
            public sealed class CommentBeforeDeclaration
            {
            }

            [A]

            #if FEATURE
            #endif
            [B]
            public sealed class DirectiveBetweenLists
            {
            }

            [A]
            [B]

            #if FEATURE
            #endif
            public sealed class DirectiveBeforeDeclaration
            {
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Equal(4, diagnostics.Count(IsAttributeSpacingDiagnostic));
    }

    [Fact]
    public async Task RAW430_blockComment内部のwhitespaceOnly行は診断しない()
    {
        const string source = """
            [A]
            /* Attribute list境界の説明です。

            */
            [B]
            public sealed class CommentBetweenLists
            {
            }

            [A]
            [B]
            /* 宣言境界の説明です。

            */
            public sealed class CommentBeforeDeclaration
            {
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.DoesNotContain(diagnostics, IsAttributeSpacingDiagnostic);
    }

    [Fact]
    public async Task RAW430_blockComment外の実空行だけを診断する()
    {
        const string source = """
            [A]

            /* Attribute list境界の説明です。

            */
            [B]
            public sealed class BlankBeforeComment
            {
            }

            [A]
            [B]
            /* 宣言境界の説明です。

            */

            public sealed class BlankAfterComment
            {
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source);

        Assert.Equal(2, diagnostics.Count(IsAttributeSpacingDiagnostic));
    }

    private static bool IsAttributeSpacingDiagnostic(Diagnostic diagnostic)
    {
        return diagnostic.Id == "RAW430"
            && diagnostic.GetMessage(CultureInfo.InvariantCulture).Equals(AttributeSpacingMessage, StringComparison.Ordinal);
    }

    private static Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(string source)
    {
        return AnalyzerTestHelper.AnalyzeAsync(
            source,
            new MemberSpacingAnalyzer(),
            "/repo/src/Target.cs",
            "ReArtWord.SourceCodeQuality.Domain");
    }
}
