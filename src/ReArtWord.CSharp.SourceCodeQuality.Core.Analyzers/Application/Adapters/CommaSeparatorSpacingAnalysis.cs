using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// カンマ区切りのSpaceを解析します。
/// </summary>
internal static class CommaSeparatorSpacingAnalysis
{
    /// <summary>
    /// カンマ区切りのSpaceを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="compilationUnit">コンパイル単位です。</param>
    /// <param name="sourceText">ソーステキストです。</param>
    public static void Analyze(
        SyntaxNodeAnalysisContext context,
        CompilationUnitSyntax compilationUnit,
        SourceText sourceText)
    {
        foreach (SyntaxToken token in compilationUnit.DescendantTokens(descendIntoTrivia: false))
        {
            if (!token.IsKind(SyntaxKind.CommaToken) || IsNonSeparatorComma(token))
            {
                continue;
            }

            AnalyzeBeforeSpacing(context, sourceText, token);
            AnalyzeAfterSpacing(context, sourceText, token);
        }
    }

    /// <summary>
    /// カンマ直前のSpaceを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="commaToken">カンマトークンです。</param>
    private static void AnalyzeBeforeSpacing(
        SyntaxNodeAnalysisContext context,
        SourceText sourceText,
        SyntaxToken commaToken)
    {
        SyntaxToken previousToken = commaToken.GetPreviousToken();
        if (SpacingSyntaxHelper.IsMissingOrEndOfFile(previousToken))
        {
            return;
        }

        if (!SpacingSyntaxHelper.IsSameLine(sourceText, previousToken.Span.End, commaToken.SpanStart))
        {
            return;
        }

        string gapText = SpacingSyntaxHelper.GetGapText(sourceText, previousToken.Span.End, commaToken.SpanStart);
        if (gapText.Length == 0 || !SpacingSyntaxHelper.IsWhitespaceOnly(gapText))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.CommaSeparatorSpacingRule,
                commaToken.GetLocation()));
    }

    /// <summary>
    /// カンマ直後のSpaceを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="commaToken">カンマトークンです。</param>
    private static void AnalyzeAfterSpacing(
        SyntaxNodeAnalysisContext context,
        SourceText sourceText,
        SyntaxToken commaToken)
    {
        SyntaxToken nextToken = commaToken.GetNextToken();
        if (SpacingSyntaxHelper.IsMissingOrEndOfFile(nextToken) || IsClosingDelimiter(nextToken))
        {
            return;
        }

        if (!SpacingSyntaxHelper.IsSameLine(sourceText, commaToken.Span.End, nextToken.SpanStart))
        {
            return;
        }

        string gapText = SpacingSyntaxHelper.GetGapText(sourceText, commaToken.Span.End, nextToken.SpanStart);
        if (gapText == " ")
        {
            return;
        }

        if (gapText.Length > 0 && !SpacingSyntaxHelper.IsWhitespaceOnly(gapText))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.CommaSeparatorSpacingRule,
                commaToken.GetLocation()));
    }

    /// <summary>
    /// 区切り用途ではないカンマか判定します。
    /// </summary>
    /// <param name="token">対象トークンです。</param>
    /// <returns>区切り用途ではないカンマの場合は <see langword="true" /> を返します。</returns>
    private static bool IsNonSeparatorComma(SyntaxToken token)
    {
        return token.Parent is InterpolationAlignmentClauseSyntax
            || IsOmittedArrayRankComma(token);
    }

    /// <summary>
    /// 配列ランク指定の省略サイズカンマか判定します。
    /// </summary>
    /// <param name="token">対象トークンです。</param>
    /// <returns>配列ランク指定の省略サイズカンマの場合は <see langword="true" /> を返します。</returns>
    private static bool IsOmittedArrayRankComma(SyntaxToken token)
    {
        return token.Parent is ArrayRankSpecifierSyntax rankSpecifier
            && rankSpecifier.Sizes.All(static size => size.IsKind(SyntaxKind.OmittedArraySizeExpression));
    }

    /// <summary>
    /// 閉じ区切りトークンか判定します。
    /// </summary>
    /// <param name="token">対象トークンです。</param>
    /// <returns>閉じ区切りトークンの場合は <see langword="true" /> を返します。</returns>
    private static bool IsClosingDelimiter(SyntaxToken token)
    {
        return token.IsKind(SyntaxKind.CloseParenToken)
            || token.IsKind(SyntaxKind.CloseBracketToken)
            || token.IsKind(SyntaxKind.CloseBraceToken);
    }
}
