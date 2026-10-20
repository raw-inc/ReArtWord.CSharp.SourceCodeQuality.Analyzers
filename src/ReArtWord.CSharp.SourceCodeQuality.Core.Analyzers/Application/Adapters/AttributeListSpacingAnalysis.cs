using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// RAW430対象宣言のAttribute list境界にある空行を解析します。
/// </summary>
internal static class AttributeListSpacingAnalysis
{
    /// <summary>
    /// Attribute list境界の空行違反を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    public static void Analyze(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AttributeListSyntax attributeList
            || !TryGetTargetAttributeLists(attributeList.Parent, out SyntaxList<AttributeListSyntax> attributeLists))
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(attributeList.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        int attributeListIndex = attributeLists.IndexOf(attributeList);
        if (attributeListIndex < 0)
        {
            return;
        }

        SyntaxToken boundaryToken = attributeListIndex < attributeLists.Count - 1
            ? attributeLists[attributeListIndex + 1].GetFirstToken()
            : attributeList.GetLastToken().GetNextToken();
        SourceText sourceText = attributeList.SyntaxTree.GetText(context.CancellationToken);
        if (!TryGetFirstBlankLinePosition(
                attributeList.SyntaxTree,
                sourceText,
                attributeList.Span.End,
                boundaryToken.SpanStart,
                context.CancellationToken,
                out int position))
        {
            return;
        }

        Location location = Location.Create(attributeList.SyntaxTree, new TextSpan(position, 0));
        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.MemberSpacingRule,
                location,
                DiagnosticCatalog.AttributeListSpacingMessage));
    }

    /// <summary>
    /// 明示対象ownerのAttribute list一覧を取得します。
    /// </summary>
    /// <param name="owner">Attribute listのownerです。</param>
    /// <param name="attributeLists">Attribute list一覧です。</param>
    /// <returns>明示対象ownerの場合は <see langword="true" /> を返します。</returns>
    private static bool TryGetTargetAttributeLists(
        SyntaxNode? owner,
        out SyntaxList<AttributeListSyntax> attributeLists)
    {
        switch (owner)
        {
            case BaseTypeDeclarationSyntax declaration:
                attributeLists = declaration.AttributeLists;
                return true;
            case DelegateDeclarationSyntax declaration:
                attributeLists = declaration.AttributeLists;
                return true;
            case MethodDeclarationSyntax declaration:
                attributeLists = declaration.AttributeLists;
                return true;
            case ConstructorDeclarationSyntax declaration:
                attributeLists = declaration.AttributeLists;
                return true;
            case PropertyDeclarationSyntax declaration:
                attributeLists = declaration.AttributeLists;
                return true;
            case FieldDeclarationSyntax declaration:
                attributeLists = declaration.AttributeLists;
                return true;
            case EventDeclarationSyntax declaration:
                attributeLists = declaration.AttributeLists;
                return true;
            case EventFieldDeclarationSyntax declaration:
                attributeLists = declaration.AttributeLists;
                return true;
            case ParameterSyntax declaration when IsTargetParameter(declaration):
                attributeLists = declaration.AttributeLists;
                return true;
            default:
                attributeLists = default;
                return false;
        }
    }

    /// <summary>
    /// method、constructor、delegateのparameterか判定します。
    /// </summary>
    /// <param name="parameter">判定対象parameterです。</param>
    /// <returns>明示対象containerのparameterの場合は <see langword="true" /> を返します。</returns>
    private static bool IsTargetParameter(ParameterSyntax parameter)
    {
        return parameter.Parent?.Parent is MethodDeclarationSyntax
            or ConstructorDeclarationSyntax
            or DelegateDeclarationSyntax;
    }

    /// <summary>
    /// 境界間の最初のwhitespace-only物理行位置を取得します。
    /// </summary>
    /// <param name="syntaxTree">解析対象syntax treeです。</param>
    /// <param name="sourceText">解析対象sourceです。</param>
    /// <param name="fromPosition">直前Attribute listの終了位置です。</param>
    /// <param name="toPosition">次Attribute listまたは修飾対象宣言tokenの開始位置です。</param>
    /// <param name="cancellationToken">キャンセル通知です。</param>
    /// <param name="firstBlankLinePosition">最初の空行行頭位置です。</param>
    /// <returns>診断対象の空行がある場合は <see langword="true" /> を返します。</returns>
    private static bool TryGetFirstBlankLinePosition(
        SyntaxTree syntaxTree,
        SourceText sourceText,
        int fromPosition,
        int toPosition,
        CancellationToken cancellationToken,
        out int firstBlankLinePosition)
    {
        firstBlankLinePosition = 0;
        if (toPosition <= fromPosition)
        {
            return false;
        }

        int previousLineNumber = sourceText.Lines
            .GetLineFromPosition(fromPosition - 1)
            .LineNumber;
        int currentLineNumber = sourceText.Lines
            .GetLineFromPosition(toPosition)
            .LineNumber;
        ImmutableArray<TextSpan> commentSpans = GetCommentSpans(
            syntaxTree,
            TextSpan.FromBounds(fromPosition, toPosition),
            cancellationToken);
        for (int lineNumber = previousLineNumber + 1; lineNumber < currentLineNumber; lineNumber++)
        {
            if (!string.IsNullOrWhiteSpace(sourceText.Lines[lineNumber].ToString()))
            {
                continue;
            }

            int lineStart = sourceText.Lines[lineNumber].Start;
            if (commentSpans.Any(commentSpan => commentSpan.Contains(lineStart)))
            {
                continue;
            }

            firstBlankLinePosition = lineStart;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 指定境界内のcomment trivia範囲を取得します。
    /// </summary>
    /// <param name="syntaxTree">解析対象syntax treeです。</param>
    /// <param name="boundarySpan">解析対象境界です。</param>
    /// <param name="cancellationToken">キャンセル通知です。</param>
    /// <returns>comment trivia範囲です。</returns>
    private static ImmutableArray<TextSpan> GetCommentSpans(
        SyntaxTree syntaxTree,
        TextSpan boundarySpan,
        CancellationToken cancellationToken)
    {
        ImmutableArray<TextSpan>.Builder builder = ImmutableArray.CreateBuilder<TextSpan>();
        foreach (SyntaxTrivia trivia in syntaxTree.GetRoot(cancellationToken).DescendantTrivia(boundarySpan))
        {
            if (!IsCommentTrivia(trivia))
            {
                continue;
            }

            builder.Add(trivia.FullSpan);
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// comment triviaか判定します。
    /// </summary>
    /// <param name="trivia">判定対象triviaです。</param>
    /// <returns>comment triviaの場合は <see langword="true" /> を返します。</returns>
    private static bool IsCommentTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
            || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
            || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
            || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
            || trivia.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia);
    }
}
