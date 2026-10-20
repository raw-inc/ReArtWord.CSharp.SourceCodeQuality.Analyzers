using System.Text.RegularExpressions;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Domain.Configuration;

/// <summary>
/// 診断IDトークンの妥当性ルールを提供します。
/// </summary>
internal static class DiagnosticIdPolicy
{
    private static readonly Regex DiagnosticIdRegex = new(
        "^[A-Za-z]{1,}[0-9]{3,4}[A-Za-z]?$",
        RegexOptions.Compiled);

    /// <summary>
    /// 診断ID文字列として妥当か判定します。
    /// </summary>
    /// <param name="candidate">判定対象の文字列です。</param>
    /// <returns>妥当な診断ID形式の場合は <see langword="true" /> を返します。</returns>
    public static bool IsValidDiagnosticId(string candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        return DiagnosticIdRegex.IsMatch(candidate.Trim());
    }
}
