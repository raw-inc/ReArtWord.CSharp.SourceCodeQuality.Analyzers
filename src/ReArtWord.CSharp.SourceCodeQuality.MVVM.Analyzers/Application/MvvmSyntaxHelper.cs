using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// MVVM Analyzerの構文補助処理を提供します。
/// </summary>
internal static class MvvmSyntaxHelper
{
    private static readonly string XmlDocumentationCommentPrefix = string.Concat("/", "//");
    private static readonly string LineCommentPrefix = string.Concat("/", "/");
    private static readonly string BlockCommentPrefix = string.Concat("/", "*");

    /// <summary>
    /// 構文ノードが解析対象か判定します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="node">対象ノードです。</param>
    /// <returns>解析対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsSyntaxTarget(SyntaxNodeAnalysisContext context, SyntaxNode node)
    {
        return MvvmAnalysisFilter.IsSyntaxTarget(node.SyntaxTree, context.SemanticModel.Compilation);
    }

    /// <summary>
    /// 所属クラスの型シンボルを取得します。
    /// </summary>
    /// <param name="node">対象ノードです。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>所属クラスの型シンボルです。</returns>
    public static INamedTypeSymbol? GetContainingClassSymbol(
        SyntaxNode node,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        ClassDeclarationSyntax? classDeclaration = node.FirstAncestorOrSelf<ClassDeclarationSyntax>();
        if (classDeclaration is null)
        {
            return null;
        }

        return semanticModel.GetDeclaredSymbol(classDeclaration, cancellationToken) as INamedTypeSymbol;
    }

    /// <summary>
    /// 所属ViewModelの型シンボルを取得します。
    /// </summary>
    /// <param name="node">対象ノードです。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>所属ViewModel型です。</returns>
    public static INamedTypeSymbol? GetContainingViewModelSymbol(
        SyntaxNode node,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        INamedTypeSymbol? typeSymbol = GetContainingClassSymbol(node, semanticModel, cancellationToken);
        if (MvvmViewModelSymbolHelper.IsViewModel(typeSymbol))
        {
            return typeSymbol;
        }

        return null;
    }

    /// <summary>
    /// 呼び出し名の位置を取得します。
    /// </summary>
    /// <param name="invocation">呼び出し式です。</param>
    /// <returns>呼び出し名の位置です。</returns>
    public static Location GetInvocationNameLocation(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.GetLocation(),
            IdentifierNameSyntax identifierName => identifierName.GetLocation(),
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name.GetLocation(),
            _ => invocation.Expression.GetLocation(),
        };
    }

    /// <summary>
    /// 属性一覧から指定条件に一致する属性を取得します。
    /// </summary>
    /// <param name="attributeLists">属性一覧です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <param name="predicate">属性判定です。</param>
    /// <returns>一致した属性一覧です。</returns>
    public static IEnumerable<AttributeSyntax> GetMatchingAttributes(
        SyntaxList<AttributeListSyntax> attributeLists,
        SemanticModel semanticModel,
        Func<AttributeSyntax, SemanticModel, CancellationToken, bool> predicate,
        CancellationToken cancellationToken)
    {
        foreach (AttributeListSyntax attributeList in attributeLists)
        {
            foreach (AttributeSyntax attribute in attributeList.Attributes)
            {
                if (predicate(attribute, semanticModel, cancellationToken))
                {
                    yield return attribute;
                }
            }
        }
    }

    /// <summary>
    /// 属性一覧が指定条件に一致する属性を持つか判定します。
    /// </summary>
    /// <param name="attributeLists">属性一覧です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <param name="predicate">属性判定です。</param>
    /// <returns>一致する属性がある場合は <see langword="true" /> を返します。</returns>
    public static bool HasMatchingAttribute(
        SyntaxList<AttributeListSyntax> attributeLists,
        SemanticModel semanticModel,
        Func<AttributeSyntax, SemanticModel, CancellationToken, bool> predicate,
        CancellationToken cancellationToken)
    {
        return GetMatchingAttributes(attributeLists, semanticModel, predicate, cancellationToken).Any();
    }

    /// <summary>
    /// 属性直前行に日本語を含む通常コメントがあるか判定します。
    /// </summary>
    /// <param name="attributeList">属性リストです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>直前コメントがある場合は <see langword="true" /> を返します。</returns>
    public static bool HasJapaneseLineCommentImmediatelyBefore(
        AttributeListSyntax attributeList,
        CancellationToken cancellationToken)
    {
        FileLinePositionSpan span = attributeList.SyntaxTree.GetLineSpan(attributeList.Span, cancellationToken);
        int startLine = span.StartLinePosition.Line;
        if (startLine < 1)
        {
            return false;
        }

        SourceText sourceText = attributeList.SyntaxTree.GetText(cancellationToken);
        string previousLine = sourceText.Lines[startLine - 1].ToString().Trim();
        if (!IsNormalComment(previousLine))
        {
            return false;
        }

        return ContainsJapaneseText(previousLine);
    }

    /// <summary>
    /// 通常コメント行か判定します。
    /// </summary>
    /// <param name="lineText">行文字列です。</param>
    /// <returns>通常コメント行の場合は <see langword="true" /> を返します。</returns>
    private static bool IsNormalComment(string lineText)
    {
        if (lineText.StartsWith(XmlDocumentationCommentPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        return lineText.StartsWith(LineCommentPrefix, StringComparison.Ordinal)
            || lineText.StartsWith(BlockCommentPrefix, StringComparison.Ordinal);
    }

    /// <summary>
    /// 日本語文字を含むか判定します。
    /// </summary>
    /// <param name="text">対象文字列です。</param>
    /// <returns>日本語文字を含む場合は <see langword="true" /> を返します。</returns>
    private static bool ContainsJapaneseText(string text)
    {
        foreach (char character in text)
        {
            if (('\u3040' <= character && character <= '\u30ff')
                || ('\u3400' <= character && character <= '\u9fff'))
            {
                return true;
            }
        }

        return false;
    }
}
