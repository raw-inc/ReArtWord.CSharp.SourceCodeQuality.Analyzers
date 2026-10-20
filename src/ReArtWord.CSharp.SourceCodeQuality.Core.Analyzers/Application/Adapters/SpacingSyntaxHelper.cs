using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 空白フォーマット解析で共有する構文ヘルパーです。
/// </summary>
internal static class SpacingSyntaxHelper
{
    /// <summary>
    /// 欠落またはファイル終端トークンか判定します。
    /// </summary>
    /// <param name="token">対象トークンです。</param>
    /// <returns>欠落またはファイル終端トークンの場合は <see langword="true" /> を返します。</returns>
    public static bool IsMissingOrEndOfFile(SyntaxToken token)
    {
        return token.RawKind == 0
            || token.IsMissing
            || token.IsKind(SyntaxKind.EndOfFileToken);
    }

    /// <summary>
    /// 2位置が同一行にあるか判定します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="leftPosition">左位置です。</param>
    /// <param name="rightPosition">右位置です。</param>
    /// <returns>同一行の場合は <see langword="true" /> を返します。</returns>
    public static bool IsSameLine(SourceText sourceText, int leftPosition, int rightPosition)
    {
        return sourceText.Lines.GetLinePosition(leftPosition).Line
            == sourceText.Lines.GetLinePosition(rightPosition).Line;
    }

    /// <summary>
    /// 2位置間のテキストを取得します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="start">開始位置です。</param>
    /// <param name="end">終了位置です。</param>
    /// <returns>2位置間のテキストです。</returns>
    public static string GetGapText(SourceText sourceText, int start, int end)
    {
        if (end <= start)
        {
            return string.Empty;
        }

        return sourceText.ToString(TextSpan.FromBounds(start, end));
    }

    /// <summary>
    /// 文字列が空白のみか判定します。
    /// </summary>
    /// <param name="text">対象文字列です。</param>
    /// <returns>空白のみの場合は <see langword="true" /> を返します。</returns>
    public static bool IsWhitespaceOnly(string text)
    {
        for (int index = 0; index < text.Length; index++)
        {
            if (!char.IsWhiteSpace(text[index]))
            {
                return false;
            }
        }

        return true;
    }
}
