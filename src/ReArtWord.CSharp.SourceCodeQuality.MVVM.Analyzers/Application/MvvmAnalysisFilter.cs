using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// MVVM Analyzerの解析対象判定を提供します。
/// </summary>
internal static class MvvmAnalysisFilter
{
    private static readonly string[] AnalyzerAssemblyNames =
    [
        "ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers",
        "ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests",
        "ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers",
        "ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests",
    ];

    private static readonly string ObjectIntermediatePathSegment = string.Concat("/", "obj", "/");

    /// <summary>
    /// コンパイル単位がMVVM Analyzerの対象か判定します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsCompilationTarget(Compilation compilation)
    {
        string assemblyName = compilation.AssemblyName ?? string.Empty;
        return !AnalyzerAssemblyNames.Contains(assemblyName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 構文木がMVVM Analyzerの対象か判定します。
    /// </summary>
    /// <param name="syntaxTree">構文木です。</param>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsSyntaxTarget(SyntaxTree syntaxTree, Compilation compilation)
    {
        if (!IsCompilationTarget(compilation))
        {
            return false;
        }

        string normalizedPath = NormalizePath(syntaxTree.FilePath ?? string.Empty);
        if (normalizedPath.Length == 0)
        {
            return true;
        }

        if (normalizedPath.Contains(ObjectIntermediatePathSegment, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !HasGeneratedSuffix(normalizedPath);
    }

    /// <summary>
    /// パス区切りを正規化します。
    /// </summary>
    /// <param name="path">対象パスです。</param>
    /// <returns>正規化したパスです。</returns>
    private static string NormalizePath(string path)
    {
        return path.Replace('\\', '/');
    }

    /// <summary>
    /// 生成コードファイル名か判定します。
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
