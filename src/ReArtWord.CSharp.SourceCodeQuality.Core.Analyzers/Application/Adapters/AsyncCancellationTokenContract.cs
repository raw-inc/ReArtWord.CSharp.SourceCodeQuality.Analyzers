using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 非同期メソッドのCancellationToken契約違反位置を判定します。
/// </summary>
internal static class AsyncCancellationTokenContract
{
    /// <summary>
    /// CancellationToken契約違反の診断位置を取得します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <param name="location">診断位置です。</param>
    /// <returns>違反がある場合は <see langword="true" /> を返します。</returns>
    public static bool TryGetDiagnosticLocation(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration,
        out Location? location)
    {
        location = null;
        SeparatedSyntaxList<ParameterSyntax> parameters = methodDeclaration.ParameterList.Parameters;
        if (parameters.Count == 0)
        {
            location = AsyncContractDiagnosticLocation.GetMissingCancellationTokenLocation(methodDeclaration.ParameterList);
            return true;
        }

        ParameterSyntax lastParameter = parameters[parameters.Count - 1];
        if (context.SemanticModel.GetDeclaredSymbol(lastParameter, context.CancellationToken) is IParameterSymbol lastParameterSymbol
            && IsCancellationTokenType(lastParameterSymbol.Type))
        {
            return TryGetLastTokenDiagnosticLocation(lastParameter, out location);
        }

        int cancellationTokenParameterIndex = FindCancellationTokenParameterIndex(context, parameters);
        if (cancellationTokenParameterIndex < 0)
        {
            location = AsyncContractDiagnosticLocation.GetMissingCancellationTokenLocation(methodDeclaration.ParameterList);
            return true;
        }

        location = AsyncContractDiagnosticLocation.GetCancellationTokenParameterLocation(
            parameters[cancellationTokenParameterIndex]);
        return true;
    }

    /// <summary>
    /// 最終CancellationTokenパラメータの違反位置を取得します。
    /// </summary>
    /// <param name="lastParameter">最終パラメータです。</param>
    /// <param name="location">診断位置です。</param>
    /// <returns>違反がある場合は <see langword="true" /> を返します。</returns>
    private static bool TryGetLastTokenDiagnosticLocation(ParameterSyntax lastParameter, out Location? location)
    {
        location = null;
        if (HasDefaultTokenValue(lastParameter))
        {
            return false;
        }

        location = AsyncContractDiagnosticLocation.GetCancellationTokenParameterLocation(lastParameter);
        return true;
    }

    /// <summary>
    /// CancellationToken型か判定します。
    /// </summary>
    /// <param name="typeSymbol">判定対象型です。</param>
    /// <returns>CancellationToken型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsCancellationTokenType(ITypeSymbol? typeSymbol)
    {
        return typeSymbol?.ToDisplayString().Equals("System.Threading.CancellationToken", StringComparison.Ordinal) == true;
    }

    /// <summary>
    /// パラメータがdefault値を持つか判定します。
    /// </summary>
    /// <param name="parameter">パラメータ構文です。</param>
    /// <returns>default値を持つ場合は <see langword="true" /> を返します。</returns>
    private static bool HasDefaultTokenValue(ParameterSyntax parameter)
    {
        if (parameter.Default?.Value is LiteralExpressionSyntax literal
            && literal.IsKind(SyntaxKind.DefaultLiteralExpression))
        {
            return true;
        }

        return parameter.Default?.Value is DefaultExpressionSyntax;
    }

    /// <summary>
    /// CancellationTokenパラメータの位置を取得します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="parameters">パラメータ一覧です。</param>
    /// <returns>CancellationTokenパラメータのインデックスです。存在しない場合は -1 を返します。</returns>
    private static int FindCancellationTokenParameterIndex(
        SyntaxNodeAnalysisContext context,
        SeparatedSyntaxList<ParameterSyntax> parameters)
    {
        for (int index = 0; index < parameters.Count; index++)
        {
            ParameterSyntax parameter = parameters[index];
            if (context.SemanticModel.GetDeclaredSymbol(parameter, context.CancellationToken) is not IParameterSymbol parameterSymbol)
            {
                continue;
            }

            if (IsCancellationTokenType(parameterSymbol.Type))
            {
                return index;
            }
        }

        return -1;
    }
}
