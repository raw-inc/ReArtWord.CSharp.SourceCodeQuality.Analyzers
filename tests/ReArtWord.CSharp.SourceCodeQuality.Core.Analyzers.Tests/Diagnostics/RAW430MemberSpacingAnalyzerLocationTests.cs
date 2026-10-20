using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW430MemberSpacingAnalyzerLocationTests
{
    [Fact]
    public async Task RAW430_空行不足は次要素行頭へ報告する()
    {
        string markedSource = string.Join(
            Environment.NewLine,
            "public sealed class Target",
            "{",
            "    public void A()",
            "    {",
            "    }",
            "/*診断位置*/    public void B()",
            "    {",
            "    }",
            "}");

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MemberSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW430", source, expectedStart, string.Empty);
    }

    [Fact]
    public async Task RAW430_空行過剰は2つ目の空行行頭へ報告する()
    {
        string markedSource = string.Join(
            Environment.NewLine,
            "public sealed class Target",
            "{",
            "    public void A()",
            "    {",
            "    }",
            "",
            "/*診断位置*/",
            "    public void B()",
            "    {",
            "    }",
            "}");

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MemberSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW430", source, expectedStart, string.Empty);
    }

    [Fact]
    public async Task RAW430_AttributeList間の空行は最初の空行行頭へ報告する()
    {
        string markedSource = string.Join(
            Environment.NewLine,
            "[A]",
            "/*診断位置*/",
            "[B]",
            "public sealed class Target",
            "{",
            "}");

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MemberSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW430", source, expectedStart, string.Empty);
    }

    [Fact]
    public async Task RAW430_最後のAttributeListから宣言までの空行は最初の空行行頭へ報告する()
    {
        string markedSource = string.Join(
            Environment.NewLine,
            "[A]",
            "[B]",
            "/*診断位置*/",
            "public sealed class Target",
            "{",
            "}");

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MemberSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW430", source, expectedStart, string.Empty);
    }

    [Fact]
    public async Task RAW430_comment前のAttribute空行はcommentではなく空行行頭へ報告する()
    {
        string markedSource = string.Join(
            Environment.NewLine,
            "[A]",
            "/*診断位置*/",
            "// Attribute list境界の説明です。",
            "[B]",
            "public sealed class Target",
            "{",
            "}");

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MemberSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW430", source, expectedStart, string.Empty);
    }

    [Fact]
    public async Task RAW430_directive前の宣言境界空行はdirectiveではなく空行行頭へ報告する()
    {
        string markedSource = string.Join(
            Environment.NewLine,
            "[A]",
            "[B]",
            "/*診断位置*/",
            "#if FEATURE",
            "#endif",
            "public sealed class Target",
            "{",
            "}");

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MemberSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW430", source, expectedStart, string.Empty);
    }

    [Fact]
    public async Task RAW430_blockComment前の実空行をcomment内部ではなく報告する()
    {
        string markedSource = string.Join(
            Environment.NewLine,
            "[A]",
            "/*診断位置*/",
            "/* Attribute list境界の説明です。",
            "",
            "*/",
            "[B]",
            "public sealed class Target",
            "{",
            "}");

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MemberSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW430", source, expectedStart, string.Empty);
    }

    [Fact]
    public async Task RAW430_blockComment後の実空行を内部空行より優先して報告する()
    {
        string markedSource = string.Join(
            Environment.NewLine,
            "[A]",
            "[B]",
            "/* 宣言境界の説明です。",
            "",
            "*/",
            "/*診断位置*/",
            "public sealed class Target",
            "{",
            "}");

        string source = DiagnosticLocationAssert.RemoveMarker(markedSource, out int expectedStart);
        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new MemberSpacingAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        DiagnosticLocationAssert.ContainsAt(diagnostics, "RAW430", source, expectedStart, string.Empty);
    }
}
