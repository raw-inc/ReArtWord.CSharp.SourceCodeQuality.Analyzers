using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 非同期契約Analyzerの診断位置を選択します。
/// </summary>
internal static class AsyncContractDiagnosticLocation
{
    /// <summary>
    /// fire-and-forget discard代入の診断位置を取得します。
    /// </summary>
    /// <param name="assignmentExpression">代入式です。</param>
    /// <returns>診断位置です。</returns>
    public static Location GetDiscardAssignmentLocation(AssignmentExpressionSyntax assignmentExpression)
    {
        return assignmentExpression.Right.GetLocation();
    }

    /// <summary>
    /// IAsyncEnumerable推奨診断の位置を取得します。
    /// </summary>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <returns>診断位置です。</returns>
    public static Location GetAsyncEnumerableRecommendationLocation(MethodDeclarationSyntax methodDeclaration)
    {
        return methodDeclaration.ReturnType.GetLocation();
    }

    /// <summary>
    /// CancellationToken不足時の挿入位置を取得します。
    /// </summary>
    /// <param name="parameterList">パラメータリストです。</param>
    /// <returns>診断位置です。</returns>
    public static Location GetMissingCancellationTokenLocation(ParameterListSyntax parameterList)
    {
        return Location.Create(
            parameterList.SyntaxTree,
            new TextSpan(parameterList.CloseParenToken.SpanStart, 0));
    }

    /// <summary>
    /// CancellationToken引数違反の診断位置を取得します。
    /// </summary>
    /// <param name="parameter">違反パラメータです。</param>
    /// <returns>診断位置です。</returns>
    public static Location GetCancellationTokenParameterLocation(ParameterSyntax parameter)
    {
        return parameter.GetLocation();
    }
}
