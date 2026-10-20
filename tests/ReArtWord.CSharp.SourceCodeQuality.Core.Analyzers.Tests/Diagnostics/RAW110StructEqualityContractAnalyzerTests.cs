using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW110StructEqualityContractAnalyzerTests
{
    [Fact]
    public async Task RAW110_一部実装のみのstructを検出する()
    {
        const string source = """
            public struct Target
            {
                public int Value;

                public override bool Equals(object obj)
                {
                    return obj is Target other && other.Value == Value;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new StructEqualityContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW110");
    }

    [Fact]
    public async Task RAW110_必須セット完備structは非検出()
    {
        const string source = """
            using System;

            public struct Target : IEquatable<Target>
            {
                public int Value;

                public override bool Equals(object obj)
                {
                    return obj is Target other && Equals(other);
                }

                public bool Equals(Target other)
                {
                    return Value == other.Value;
                }

                public override int GetHashCode()
                {
                    return Value.GetHashCode();
                }

                public static bool operator ==(Target left, Target right)
                {
                    return left.Equals(right);
                }

                public static bool operator !=(Target left, Target right)
                {
                    return !left.Equals(right);
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new StructEqualityContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW110");
    }

    [Fact]
    public async Task RAW110_等値性未実装structは非検出()
    {
        const string source = """
            public struct Target
            {
                public int Value;
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new StructEqualityContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW110");
    }

    [Fact]
    public async Task RAW110_record_struct既定実装は非検出()
    {
        const string source = """
            public readonly record struct Target(int Value);
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new StructEqualityContractAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW110");
    }

    [Fact]
    public async Task RAW110_tests配下は検出しない()
    {
        const string source = """
            public struct Target
            {
                public int Value;

                public override bool Equals(object obj)
                {
                    return obj is Target other && other.Value == Value;
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new StructEqualityContractAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW110");
    }
}
