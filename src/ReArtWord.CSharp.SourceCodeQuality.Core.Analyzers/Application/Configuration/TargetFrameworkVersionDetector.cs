using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Configuration;

/// <summary>
/// ターゲットフレームワーク判定ヘルパーです。
/// </summary>
internal static class TargetFrameworkVersionDetector
{
    private static readonly Regex NetSymbolRegex = new(
        "^NET(?<major>[0-9]{1,2})_[0-9]+(?:_OR_GREATER)?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    /// <summary>
    /// ターゲットが.NET6以上か判定します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <returns>.NET6以上の場合は <see langword="true" /> を返します。</returns>
    public static bool IsNet6OrGreater(Compilation compilation)
    {
        return IsNetVersionOrGreater(compilation, 6);
    }

    /// <summary>
    /// ターゲットが.NET7以上か判定します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <returns>.NET7以上の場合は <see langword="true" /> を返します。</returns>
    public static bool IsNet7OrGreater(Compilation compilation)
    {
        return IsNetVersionOrGreater(compilation, 7);
    }

    /// <summary>
    /// ターゲットが.NET8以上か判定します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <returns>.NET8以上の場合は <see langword="true" /> を返します。</returns>
    public static bool IsNet8OrGreater(Compilation compilation)
    {
        return IsNetVersionOrGreater(compilation, 8);
    }

    /// <summary>
    /// ターゲットが.NET9以上か判定します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <returns>.NET9以上の場合は <see langword="true" /> を返します。</returns>
    public static bool IsNet9OrGreater(Compilation compilation)
    {
        return IsNetVersionOrGreater(compilation, 9);
    }

    /// <summary>
    /// ターゲットが.NET10以上か判定します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <returns>.NET10以上の場合は <see langword="true" /> を返します。</returns>
    public static bool IsNet10OrGreater(Compilation compilation)
    {
        return IsNetVersionOrGreater(compilation, 10);
    }

    /// <summary>
    /// ターゲットが指定メジャー以上か判定します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <param name="requiredMajorVersion">必要メジャーバージョンです。</param>
    /// <returns>条件を満たす場合は <see langword="true" /> を返します。</returns>
    private static bool IsNetVersionOrGreater(Compilation compilation, int requiredMajorVersion)
    {
        if (compilation is null)
        {
            throw new ArgumentNullException(nameof(compilation));
        }

        if (TryGetMajorVersionFromPreprocessorSymbol(compilation, out int symbolMajorVersion, out bool hasNetSymbol))
        {
            return symbolMajorVersion >= requiredMajorVersion;
        }

        if (hasNetSymbol)
        {
            return false;
        }

        if (!TryGetSystemRuntimeMajorVersion(compilation, out int runtimeMajorVersion))
        {
            return false;
        }

        return runtimeMajorVersion >= requiredMajorVersion;
    }

    /// <summary>
    /// 事前定義シンボルから.NETメジャーバージョンを取得します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <param name="majorVersion">取得したメジャーバージョンです。</param>
    /// <param name="hasNetSymbol">NET系シンボルが存在したかどうかです。</param>
    /// <returns>取得できた場合は <see langword="true" /> を返します。</returns>
    private static bool TryGetMajorVersionFromPreprocessorSymbol(
        Compilation compilation,
        out int majorVersion,
        out bool hasNetSymbol)
    {
        majorVersion = 0;
        hasNetSymbol = false;

        foreach (SyntaxTree syntaxTree in compilation.SyntaxTrees)
        {
            if (syntaxTree.Options is not CSharpParseOptions parseOptions)
            {
                continue;
            }

            foreach (string symbol in parseOptions.PreprocessorSymbolNames)
            {
                if (!symbol.StartsWith("NET", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                hasNetSymbol = true;
                if (!TryParseNetMajorVersion(symbol, out int parsedMajorVersion))
                {
                    continue;
                }

                if (parsedMajorVersion > majorVersion)
                {
                    majorVersion = parsedMajorVersion;
                }
            }
        }

        return majorVersion > 0;
    }

    /// <summary>
    /// シンボル文字列を.NETメジャーバージョンへ変換します。
    /// </summary>
    /// <param name="symbol">事前定義シンボルです。</param>
    /// <param name="majorVersion">変換結果です。</param>
    /// <returns>変換成功時は <see langword="true" /> を返します。</returns>
    private static bool TryParseNetMajorVersion(string symbol, out int majorVersion)
    {
        majorVersion = 0;
        Match match = NetSymbolRegex.Match(symbol.Trim());
        if (!match.Success)
        {
            return false;
        }

        return int.TryParse(match.Groups["major"].Value, out majorVersion);
    }

    /// <summary>
    /// 参照アセンブリからSystem.Runtimeのメジャーバージョンを取得します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <param name="majorVersion">取得したメジャーバージョンです。</param>
    /// <returns>取得成功時は <see langword="true" /> を返します。</returns>
    private static bool TryGetSystemRuntimeMajorVersion(Compilation compilation, out int majorVersion)
    {
        majorVersion = 0;
        foreach (AssemblyIdentity reference in compilation.ReferencedAssemblyNames)
        {
            if (!reference.Name.Equals("System.Runtime", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            majorVersion = reference.Version.Major;
            return true;
        }

        return false;
    }
}
