using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;

/// <summary>
/// 本番コード判定ロジックです。
/// </summary>
internal static class ProductionCodeFilter
{
    /// <summary>
    /// 解析対象が本番コードかどうかを判定します。
    /// </summary>
    /// <param name="syntaxTree">構文木です。</param>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <returns>本番コードの場合は <see langword="true" /> を返します。</returns>
    public static bool IsProductionCode(SyntaxTree syntaxTree, Compilation compilation)
    {
        string assemblyName = compilation.AssemblyName ?? string.Empty;
        if (assemblyName.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string filePath = syntaxTree.FilePath ?? string.Empty;
        if (filePath.Length == 0)
        {
            return true;
        }

        string normalizedPath = NormalizePath(filePath);

        if (normalizedPath.Contains("/tests/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (normalizedPath.Contains("/obj/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !HasGeneratedSuffix(normalizedPath);
    }

    /// <summary>
    /// 解析対象が本番コードまたはテストコード（AnalyzerTests除外）かどうかを判定します。
    /// </summary>
    /// <param name="syntaxTree">構文木です。</param>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <returns>本番コードまたは対象テストコードの場合は <see langword="true" /> を返します。</returns>
    public static bool IsProductionOrTargetTestCode(SyntaxTree syntaxTree, Compilation compilation)
    {
        if (IsProductionCode(syntaxTree, compilation))
        {
            return true;
        }

        string assemblyName = compilation.AssemblyName ?? string.Empty;
        string normalizedPath = NormalizePath(syntaxTree.FilePath ?? string.Empty);

        if (IsAnalyzerTests(assemblyName, normalizedPath))
        {
            return false;
        }

        if (normalizedPath.Contains("/obj/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (HasGeneratedSuffix(normalizedPath))
        {
            return false;
        }

        if (assemblyName.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return normalizedPath.Contains("/tests/", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// パス区切りを正規化します。
    /// </summary>
    /// <param name="path">対象パスです。</param>
    /// <returns>正規化済みパスです。</returns>
    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    /// <summary>
    /// Analyzer単体テストコードかどうかを判定します。
    /// </summary>
    /// <param name="assemblyName">アセンブリ名です。</param>
    /// <param name="normalizedPath">正規化済みパスです。</param>
    /// <returns>Analyzer単体テストコードの場合は <see langword="true" /> を返します。</returns>
    private static bool IsAnalyzerTests(string assemblyName, string normalizedPath)
    {
        if (assemblyName.Equals("ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return normalizedPath.Contains(
            "/tests/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests/",
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 生成コードファイル名かどうかを判定します。
    /// </summary>
    /// <param name="normalizedPath">正規化済みパスです。</param>
    /// <returns>生成コードの場合は <see langword="true" /> を返します。</returns>
    private static bool HasGeneratedSuffix(string normalizedPath)
    {
        return normalizedPath.EndsWith(".g.cs", StringComparison.OrdinalIgnoreCase)
            || normalizedPath.EndsWith(".g.i.cs", StringComparison.OrdinalIgnoreCase)
            || normalizedPath.EndsWith(".designer.cs", StringComparison.OrdinalIgnoreCase)
            || normalizedPath.EndsWith(".generated.cs", StringComparison.OrdinalIgnoreCase)
            || normalizedPath.EndsWith("AssemblyInfo.cs", StringComparison.OrdinalIgnoreCase);
    }
}
