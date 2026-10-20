using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// ObservableProperty変更hook内の再入リスクを検証します。
/// </summary>
internal static class MvvmChangedHookRuleAnalyzer
{
    /// <summary>
    /// メソッド宣言を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    public static void AnalyzeMethodDeclaration(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, methodDeclaration)
            || MvvmSyntaxHelper.GetContainingViewModelSymbol(
                methodDeclaration,
                context.SemanticModel,
                context.CancellationToken) is null)
        {
            return;
        }

        string? propertyName = GetChangedHookPropertyName(methodDeclaration.Identifier.ValueText);
        if (propertyName is null)
        {
            return;
        }

        AnalyzeAssignments(context, methodDeclaration, propertyName);
        AnalyzeIncrementOrDecrement(context, methodDeclaration, propertyName);
    }

    /// <summary>
    /// On{Name}Changed形式の対象プロパティ名を取得します。
    /// </summary>
    /// <param name="methodName">メソッド名です。</param>
    /// <returns>対象プロパティ名です。</returns>
    private static string? GetChangedHookPropertyName(string methodName)
    {
        string prefix = "On";
        string suffix = "Changed";
        if (!methodName.StartsWith(prefix, StringComparison.Ordinal)
            || !methodName.EndsWith(suffix, StringComparison.Ordinal)
            || methodName.Length <= prefix.Length + suffix.Length)
        {
            return null;
        }

        return methodName.Substring(prefix.Length, methodName.Length - prefix.Length - suffix.Length);
    }

    /// <summary>
    /// hook内の代入式を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <param name="propertyName">対象プロパティ名です。</param>
    private static void AnalyzeAssignments(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration,
        string propertyName)
    {
        IEnumerable<AssignmentExpressionSyntax> assignments = methodDeclaration.DescendantNodes()
            .OfType<AssignmentExpressionSyntax>();
        foreach (AssignmentExpressionSyntax assignment in assignments)
        {
            if (!IsMatchingPropertyExpression(context, assignment.Left, propertyName))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                MvvmDiagnosticCatalog.ObservablePropertyChangedReassignmentRule,
                GetExpressionNameLocation(assignment.Left)));
        }
    }

    /// <summary>
    /// hook内のインクリメント/デクリメントを検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <param name="propertyName">対象プロパティ名です。</param>
    private static void AnalyzeIncrementOrDecrement(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration,
        string propertyName)
    {
        IEnumerable<ExpressionSyntax> expressions = methodDeclaration.DescendantNodes()
            .Where(static node =>
                node.IsKind(SyntaxKind.PreIncrementExpression)
                || node.IsKind(SyntaxKind.PreDecrementExpression)
                || node.IsKind(SyntaxKind.PostIncrementExpression)
                || node.IsKind(SyntaxKind.PostDecrementExpression))
            .Select(static node => node switch
            {
                PrefixUnaryExpressionSyntax prefix => prefix.Operand,
                PostfixUnaryExpressionSyntax postfix => postfix.Operand,
                _ => null,
            })
            .OfType<ExpressionSyntax>();
        foreach (ExpressionSyntax expression in expressions)
        {
            if (!IsMatchingPropertyExpression(context, expression, propertyName))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                MvvmDiagnosticCatalog.ObservablePropertyChangedReassignmentRule,
                GetExpressionNameLocation(expression)));
        }
    }

    /// <summary>
    /// 式が対象プロパティを指すか判定します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="expression">対象式です。</param>
    /// <param name="propertyName">プロパティ名です。</param>
    /// <returns>対象プロパティの場合は <see langword="true" /> を返します。</returns>
    private static bool IsMatchingPropertyExpression(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        string propertyName)
    {
        ISymbol? symbol = context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken).Symbol;
        if (symbol is IPropertySymbol propertySymbol)
        {
            return propertySymbol.Name.Equals(propertyName, StringComparison.Ordinal);
        }

        return expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText.Equals(propertyName, StringComparison.Ordinal),
            MemberAccessExpressionSyntax { Expression: ThisExpressionSyntax } memberAccess =>
                memberAccess.Name.Identifier.ValueText.Equals(propertyName, StringComparison.Ordinal),
            _ => false,
        };
    }

    /// <summary>
    /// 式名の位置を取得します。
    /// </summary>
    /// <param name="expression">対象式です。</param>
    /// <returns>式名の位置です。</returns>
    private static Location GetExpressionNameLocation(ExpressionSyntax expression)
    {
        return expression switch
        {
            IdentifierNameSyntax identifier => identifier.GetLocation(),
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.GetLocation(),
            _ => expression.GetLocation(),
        };
    }
}
