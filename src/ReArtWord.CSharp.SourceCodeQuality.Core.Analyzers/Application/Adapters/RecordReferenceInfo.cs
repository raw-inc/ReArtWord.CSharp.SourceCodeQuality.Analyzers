using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// record参照情報です。
/// </summary>
internal sealed class RecordReferenceInfo(INamedTypeSymbol symbol, SyntaxTree referenceSyntaxTree)
{
    /// <summary>
    /// 参照先シンボルを取得します。
    /// </summary>
    public INamedTypeSymbol Symbol { get; } = symbol;

    /// <summary>
    /// 参照元構文木を取得します。
    /// </summary>
    public SyntaxTree ReferenceSyntaxTree { get; } = referenceSyntaxTree;
}
