using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Configuration;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Domain.Configuration;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Configuration;

/// <summary>
/// Analyzer設定値をAdditionalFilesから読み取るヘルパーです。
/// </summary>
internal static class AnalyzerConfigurationReader
{
    /// <summary>
    /// RAW101の行数上限を取得します。
    /// </summary>
    /// <param name="options">Analyzerオプションです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <param name="defaultValue">既定値です。</param>
    /// <returns>解析した行数上限です。</returns>
    public static int GetClassLengthLimit(AnalyzerOptions options, int defaultValue, CancellationToken cancellationToken)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return GetPositiveIntPropertyValue(
            options,
            AnalyzerPropertyNames.Raw101MaxClassLineCount,
            defaultValue,
            cancellationToken);
    }

    /// <summary>
    /// RAW130のpartial class宣言数上限を取得します。
    /// </summary>
    /// <param name="options">Analyzerオプションです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <param name="defaultValue">既定値です。</param>
    /// <returns>解析したpartial class宣言数上限です。</returns>
    public static int GetPartialClassDeclarationLimit(AnalyzerOptions options, int defaultValue, CancellationToken cancellationToken)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return GetPositiveIntPropertyValue(
            options,
            AnalyzerPropertyNames.Raw130MaxPartialClassDeclarationCount,
            defaultValue,
            cancellationToken);
    }

    /// <summary>
    /// RAW455の三項演算子条件数閾値を取得します。
    /// </summary>
    /// <param name="options">Analyzerオプションです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <param name="defaultValue">既定値です。</param>
    /// <returns>解析した条件数閾値です。</returns>
    public static int GetTernaryConditionCountThreshold(AnalyzerOptions options, int defaultValue, CancellationToken cancellationToken)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        return GetPositiveIntPropertyValue(
            options,
            AnalyzerPropertyNames.Raw455TernaryConditionCountThreshold,
            defaultValue,
            cancellationToken);
    }

    /// <summary>
    /// 指定プロパティの正の整数設定値を取得します。
    /// </summary>
    /// <param name="options">Analyzerオプションです。</param>
    /// <param name="propertyName">プロパティ名です。</param>
    /// <param name="defaultValue">既定値です。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>解析した正の整数値です。</returns>
    private static int GetPositiveIntPropertyValue(
        AnalyzerOptions options,
        string propertyName,
        int defaultValue,
        CancellationToken cancellationToken)
    {
        string? propsValue = GetLastPropertyValue(
            options,
            ".props",
            propertyName,
            cancellationToken);

        string? csprojValue = GetLastPropertyValue(
            options,
            ".csproj",
            propertyName,
            cancellationToken);

        if (TryParsePositiveInt(csprojValue, out int csprojLimit))
        {
            return csprojLimit;
        }

        if (TryParsePositiveInt(propsValue, out int propsLimit))
        {
            return propsLimit;
        }

        return defaultValue;
    }

    /// <summary>
    /// RAW301の許可NoWarn ID集合を取得します。
    /// </summary>
    /// <param name="options">Analyzerオプションです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <param name="defaultIds">既定許可IDです。</param>
    /// <returns>許可NoWarn ID集合です。</returns>
    public static HashSet<string> GetAllowedNoWarnIds(
        AnalyzerOptions options,
        IEnumerable<string> defaultIds,
        CancellationToken cancellationToken)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (defaultIds is null)
        {
            throw new ArgumentNullException(nameof(defaultIds));
        }

        HashSet<string> allowedIds = new(StringComparer.OrdinalIgnoreCase);
        foreach (string id in defaultIds)
        {
            AddNoWarnId(allowedIds, id);
        }

        foreach (string value in EnumeratePropertyValues(
            options,
            ".props",
            AnalyzerPropertyNames.Raw301AllowedNoWarnIds,
            cancellationToken))
        {
            AddNoWarnIdsFromProperty(allowedIds, value);
        }

        foreach (string value in EnumeratePropertyValues(
            options,
            ".csproj",
            AnalyzerPropertyNames.Raw301AllowedNoWarnIds,
            cancellationToken))
        {
            AddNoWarnIdsFromProperty(allowedIds, value);
        }

        return allowedIds;
    }

    /// <summary>
    /// 設定値文字列から許可IDを追加します。
    /// </summary>
    /// <param name="allowedIds">追加先集合です。</param>
    /// <param name="propertyValue">設定値文字列です。</param>
    private static void AddNoWarnIdsFromProperty(HashSet<string> allowedIds, string propertyValue)
    {
        foreach (string token in propertyValue.Split(';'))
        {
            AddNoWarnId(allowedIds, token);
        }
    }

    /// <summary>
    /// 許可IDを集合へ追加します。
    /// </summary>
    /// <param name="allowedIds">追加先集合です。</param>
    /// <param name="id">追加対象IDです。</param>
    private static void AddNoWarnId(HashSet<string> allowedIds, string id)
    {
        string normalized = id.Trim();
        if (normalized.Length == 0)
        {
            return;
        }

        if (normalized.StartsWith("$(", StringComparison.Ordinal))
        {
            return;
        }

        if (!DiagnosticIdPolicy.IsValidDiagnosticId(normalized))
        {
            return;
        }

        allowedIds.Add(normalized);
    }

    /// <summary>
    /// 対象拡張子の最後の設定値を取得します。
    /// </summary>
    /// <param name="options">Analyzerオプションです。</param>
    /// <param name="extension">対象拡張子です。</param>
    /// <param name="propertyName">プロパティ名です。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>最後に見つかった設定値です。未検出時は <see langword="null" /> を返します。</returns>
    private static string? GetLastPropertyValue(
        AnalyzerOptions options,
        string extension,
        string propertyName,
        CancellationToken cancellationToken)
    {
        string? lastValue = null;
        foreach (string value in EnumeratePropertyValues(options, extension, propertyName, cancellationToken))
        {
            if (value.Length == 0)
            {
                continue;
            }

            lastValue = value;
        }

        return lastValue;
    }

    /// <summary>
    /// AdditionalFilesから指定プロパティ値を列挙します。
    /// </summary>
    /// <param name="options">Analyzerオプションです。</param>
    /// <param name="extension">対象拡張子です。</param>
    /// <param name="propertyName">プロパティ名です。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>抽出した設定値列です。</returns>
    private static IEnumerable<string> EnumeratePropertyValues(
        AnalyzerOptions options,
        string extension,
        string propertyName,
        CancellationToken cancellationToken)
    {
        Regex propertyRegex = CreatePropertyRegex(propertyName);
        foreach (AdditionalText additionalFile in options.AdditionalFiles)
        {
            if (!additionalFile.Path.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            SourceText? text = additionalFile.GetText(cancellationToken);
            if (text is null)
            {
                continue;
            }

            string content = text.ToString();
            foreach (Match match in propertyRegex.Matches(content))
            {
                string value = match.Groups["value"].Value.Trim();
                if (value.Length == 0)
                {
                    continue;
                }

                yield return value;
            }
        }
    }

    /// <summary>
    /// プロパティ抽出用正規表現を生成します。
    /// </summary>
    /// <param name="propertyName">プロパティ名です。</param>
    /// <returns>生成した正規表現です。</returns>
    private static Regex CreatePropertyRegex(string propertyName)
    {
        string escapedName = Regex.Escape(propertyName);
        return new Regex(
            $"<{escapedName}[^>]*>(?<value>.*?)</{escapedName}>",
            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// 正の整数を解析します。
    /// </summary>
    /// <param name="value">解析対象文字列です。</param>
    /// <param name="parsedValue">解析結果です。</param>
    /// <returns>解析成功時は <see langword="true" /> を返します。</returns>
    private static bool TryParsePositiveInt(string? value, out int parsedValue)
    {
        if (!int.TryParse(value, out parsedValue))
        {
            return false;
        }

        return parsedValue > 0;
    }
}
