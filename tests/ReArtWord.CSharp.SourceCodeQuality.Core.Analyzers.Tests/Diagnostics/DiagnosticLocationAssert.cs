using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// 診断位置を検証するテストヘルパーです。
/// </summary>
internal static class DiagnosticLocationAssert
{
    private const string Marker = "/*診断位置*/";

    /// <summary>
    /// マーカーを除去し、期待診断位置を取得します。
    /// </summary>
    /// <param name="source">マーカー付きソースです。</param>
    /// <param name="markerPosition">マーカー除去後の期待位置です。</param>
    /// <returns>マーカー除去後のソースです。</returns>
    public static string RemoveMarker(string source, out int markerPosition)
    {
        markerPosition = source.IndexOf(Marker, StringComparison.Ordinal);
        if (markerPosition < 0)
        {
            throw new ArgumentException("診断位置マーカーが見つかりません。", nameof(source));
        }

        return source.Remove(markerPosition, Marker.Length);
    }

    /// <summary>
    /// 指定診断IDの診断が期待位置に存在することを検証します。
    /// </summary>
    /// <param name="diagnostics">診断一覧です。</param>
    /// <param name="diagnosticId">診断IDです。</param>
    /// <param name="source">マーカー除去後のソースです。</param>
    /// <param name="expectedStart">期待開始位置です。</param>
    /// <param name="expectedText">期待範囲のテキストです。</param>
    public static void ContainsAt(
        IReadOnlyCollection<Diagnostic> diagnostics,
        string diagnosticId,
        string source,
        int expectedStart,
        string expectedText)
    {
        int expectedLength = expectedText.Length;
        Assert.Contains(diagnostics, diagnostic =>
            diagnostic.Id.Equals(diagnosticId, StringComparison.Ordinal)
            && diagnostic.Location.SourceSpan.Start == expectedStart
            && diagnostic.Location.SourceSpan.Length == expectedLength
            && source.Substring(expectedStart, expectedLength).Equals(expectedText, StringComparison.Ordinal));
    }
}
