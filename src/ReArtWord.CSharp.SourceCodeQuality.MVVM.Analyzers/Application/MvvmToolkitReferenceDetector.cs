using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// CommunityToolkit.Mvvmの導入状態を判定します。
/// </summary>
internal static class MvvmToolkitReferenceDetector
{
    private static readonly string ToolkitPackageName = string.Concat("CommunityToolkit", ".Mvvm");

    private static readonly Regex PackageReferenceRegex = new(
        "<PackageReference\\b(?=[^>]*\\bInclude\\s*=\\s*[\"']CommunityToolkit\\.Mvvm[\"'])[^>]*>",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    /// <summary>
    /// CommunityToolkit.Mvvmが参照されているか判定します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <param name="options">Analyzerオプションです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>参照済みの場合は <see langword="true" /> を返します。</returns>
    public static bool HasToolkitReference(
        Compilation compilation,
        AnalyzerOptions options,
        CancellationToken cancellationToken)
    {
        if (compilation.ReferencedAssemblyNames.Any(static reference =>
            reference.Name.Equals(ToolkitPackageName, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return options.AdditionalFiles.Any(file => ContainsToolkitPackageReference(file, cancellationToken));
    }

    /// <summary>
    /// プロジェクトファイル先頭の診断位置を取得します。
    /// </summary>
    /// <param name="options">Analyzerオプションです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>取得できた位置です。</returns>
    public static Location GetProjectStartLocation(AnalyzerOptions options, CancellationToken cancellationToken)
    {
        foreach (AdditionalText additionalFile in options.AdditionalFiles)
        {
            if (!IsProjectFile(additionalFile.Path))
            {
                continue;
            }

            SourceText? sourceText = additionalFile.GetText(cancellationToken);
            if (sourceText is null)
            {
                continue;
            }

            TextSpan span = new(0, 0);
            LinePositionSpan lineSpan = sourceText.Lines.GetLinePositionSpan(span);
            return Location.Create(additionalFile.Path, span, lineSpan);
        }

        return Location.None;
    }

    /// <summary>
    /// AdditionalFileにCommunityToolkit.MvvmのPackageReferenceが含まれるか判定します。
    /// </summary>
    /// <param name="additionalFile">追加ファイルです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>含まれる場合は <see langword="true" /> を返します。</returns>
    private static bool ContainsToolkitPackageReference(AdditionalText additionalFile, CancellationToken cancellationToken)
    {
        if (!IsProjectFile(additionalFile.Path))
        {
            return false;
        }

        SourceText? sourceText = additionalFile.GetText(cancellationToken);
        return sourceText is not null && PackageReferenceRegex.IsMatch(sourceText.ToString());
    }

    /// <summary>
    /// 対象パスがcsprojか判定します。
    /// </summary>
    /// <param name="path">対象パスです。</param>
    /// <returns>csprojの場合は <see langword="true" /> を返します。</returns>
    private static bool IsProjectFile(string path)
    {
        return path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase);
    }
}
