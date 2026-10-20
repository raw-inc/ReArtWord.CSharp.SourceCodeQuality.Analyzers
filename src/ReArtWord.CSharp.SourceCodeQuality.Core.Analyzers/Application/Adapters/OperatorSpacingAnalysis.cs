using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 対象演算子の左右Spaceを解析します。
/// </summary>
internal static class OperatorSpacingAnalysis
{
    /// <summary>
    /// 対象演算子の左右Spaceを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="compilationUnit">コンパイル単位です。</param>
    /// <param name="sourceText">ソーステキストです。</param>
    public static void Analyze(
        SyntaxNodeAnalysisContext context,
        CompilationUnitSyntax compilationUnit,
        SourceText sourceText)
    {
        foreach (SyntaxNode node in compilationUnit.DescendantNodes(descendIntoTrivia: false))
        {
            switch (node)
            {
                case BinaryExpressionSyntax binaryExpression:
                    AnalyzeToken(context, sourceText, binaryExpression.OperatorToken);
                    break;

                case AssignmentExpressionSyntax assignmentExpression:
                    AnalyzeToken(context, sourceText, assignmentExpression.OperatorToken);
                    break;

                case EqualsValueClauseSyntax equalsValueClause:
                    AnalyzeToken(context, sourceText, equalsValueClause.EqualsToken);
                    break;

                case NameEqualsSyntax nameEquals:
                    AnalyzeToken(context, sourceText, nameEquals.EqualsToken);
                    break;

                case ParenthesizedLambdaExpressionSyntax parenthesizedLambda:
                    AnalyzeToken(context, sourceText, parenthesizedLambda.ArrowToken);
                    break;

                case SimpleLambdaExpressionSyntax simpleLambda:
                    AnalyzeToken(context, sourceText, simpleLambda.ArrowToken);
                    break;

                case ArrowExpressionClauseSyntax arrowExpressionClause:
                    AnalyzeToken(context, sourceText, arrowExpressionClause.ArrowToken);
                    break;

                case SwitchExpressionArmSyntax switchExpressionArm:
                    AnalyzeToken(context, sourceText, switchExpressionArm.EqualsGreaterThanToken);
                    break;

                case IsPatternExpressionSyntax isPatternExpression:
                    AnalyzeToken(context, sourceText, isPatternExpression.IsKeyword);
                    break;
            }
        }
    }

    /// <summary>
    /// 演算子トークンの左右Spaceを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="operatorToken">演算子トークンです。</param>
    private static void AnalyzeToken(
        SyntaxNodeAnalysisContext context,
        SourceText sourceText,
        SyntaxToken operatorToken)
    {
        if (SpacingSyntaxHelper.IsMissingOrEndOfFile(operatorToken))
        {
            return;
        }

        bool hasViolation = HasMissingSingleSpaceBefore(sourceText, operatorToken)
            || HasMissingSingleSpaceAfter(sourceText, operatorToken);
        if (!hasViolation)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.OperatorSpacingRule,
                operatorToken.GetLocation(),
                operatorToken.Text));
    }

    /// <summary>
    /// 演算子直前に必要なSpaceがないか判定します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="operatorToken">演算子トークンです。</param>
    /// <returns>Space不足の場合は <see langword="true" /> を返します。</returns>
    private static bool HasMissingSingleSpaceBefore(SourceText sourceText, SyntaxToken operatorToken)
    {
        SyntaxToken previousToken = operatorToken.GetPreviousToken();
        if (SpacingSyntaxHelper.IsMissingOrEndOfFile(previousToken))
        {
            return false;
        }

        return SpacingSyntaxHelper.IsSameLine(sourceText, previousToken.Span.End, operatorToken.SpanStart)
            && SpacingSyntaxHelper.GetGapText(sourceText, previousToken.Span.End, operatorToken.SpanStart).Length == 0;
    }

    /// <summary>
    /// 演算子直後に必要なSpaceがないか判定します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="operatorToken">演算子トークンです。</param>
    /// <returns>Space不足の場合は <see langword="true" /> を返します。</returns>
    private static bool HasMissingSingleSpaceAfter(SourceText sourceText, SyntaxToken operatorToken)
    {
        SyntaxToken nextToken = operatorToken.GetNextToken();
        if (SpacingSyntaxHelper.IsMissingOrEndOfFile(nextToken))
        {
            return false;
        }

        return SpacingSyntaxHelper.IsSameLine(sourceText, operatorToken.Span.End, nextToken.SpanStart)
            && SpacingSyntaxHelper.GetGapText(sourceText, operatorToken.Span.End, nextToken.SpanStart).Length == 0;
    }
}
