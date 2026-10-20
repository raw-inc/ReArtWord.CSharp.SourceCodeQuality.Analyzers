using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// ArgumentOutOfRangeExceptionへ対応する範囲guardパターンです。
/// </summary>
internal static class ArgumentOutOfRangeGuardPattern
{
    /// <summary>
    /// if条件から利用候補のThrowIfメソッド名を取得します。
    /// </summary>
    /// <param name="condition">if条件です。</param>
    /// <returns>利用候補のThrowIfメソッド名です。</returns>
    public static IEnumerable<string> GetMethodNames(ExpressionSyntax condition)
    {
        foreach (BinaryExpressionSyntax binaryExpression in condition.DescendantNodesAndSelf().OfType<BinaryExpressionSyntax>())
        {
            string? methodName = GetMethodName(binaryExpression);
            if (methodName is not null)
            {
                yield return methodName;
            }
        }
    }

    /// <summary>
    /// 比較演算子からThrowIfメソッド名を取得します。
    /// </summary>
    /// <param name="binaryExpression">二項式です。</param>
    /// <returns>推奨メソッド名です。</returns>
    private static string? GetMethodName(BinaryExpressionSyntax binaryExpression)
    {
        bool rightIsZero = IsZeroLiteral(binaryExpression.Right);
        bool leftIsZero = IsZeroLiteral(binaryExpression.Left);

        return binaryExpression.Kind() switch
        {
            SyntaxKind.LessThanExpression when rightIsZero => "ThrowIfNegative",
            SyntaxKind.LessThanOrEqualExpression when rightIsZero => "ThrowIfNegativeOrZero",
            SyntaxKind.GreaterThanExpression when leftIsZero => "ThrowIfNegative",
            SyntaxKind.GreaterThanOrEqualExpression when leftIsZero => "ThrowIfNegativeOrZero",
            SyntaxKind.LessThanExpression => "ThrowIfLessThan",
            SyntaxKind.LessThanOrEqualExpression => "ThrowIfLessThanOrEqual",
            SyntaxKind.GreaterThanExpression => "ThrowIfGreaterThan",
            SyntaxKind.GreaterThanOrEqualExpression => "ThrowIfGreaterThanOrEqual",
            SyntaxKind.EqualsExpression => "ThrowIfEqual",
            SyntaxKind.NotEqualsExpression => "ThrowIfNotEqual",
            _ => null,
        };
    }

    /// <summary>
    /// 対象式が数値0リテラルか判定します。
    /// </summary>
    /// <param name="expression">式です。</param>
    /// <returns>0リテラルの場合は <see langword="true" /> を返します。</returns>
    private static bool IsZeroLiteral(ExpressionSyntax expression)
    {
        return UnwrapParentheses(expression) is LiteralExpressionSyntax literal
            && literal.Kind() == SyntaxKind.NumericLiteralExpression
            && literal.Token.ValueText.Equals("0", StringComparison.Ordinal);
    }

    /// <summary>
    /// 括弧を除いた式を取得します。
    /// </summary>
    /// <param name="expression">式です。</param>
    /// <returns>括弧を除いた式です。</returns>
    private static ExpressionSyntax UnwrapParentheses(ExpressionSyntax expression)
    {
        ExpressionSyntax current = expression;
        while (current is ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            current = parenthesizedExpression.Expression;
        }

        return current;
    }
}
