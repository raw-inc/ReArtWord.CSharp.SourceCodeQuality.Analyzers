using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers.Application;

/// <summary>
/// コンポーネントから開始する取消可能な処理へのトークン伝搬を検証します。
/// </summary>
internal static class RazorCancellationRuleAnalyzer
{
    /// <summary>
    /// コンポーネント内で取消トークンを省略した呼出しへ診断します。
    /// </summary>
    /// <param name="context">操作の解析コンテキストです。</param>
    /// <param name="component">コンポーネントの基底型です。</param>
    /// <param name="cancellation">取消トークン型です。</param>
    public static void AnalyzeInvocation(
        OperationAnalysisContext context,
        INamedTypeSymbol component,
        INamedTypeSymbol cancellation)
    {
        string disposeMethodName = nameof(IDisposable.Dispose);
        string disposeAsyncMethodName = "DisposeAsync";
        if (context.ContainingSymbol.Name.Equals(disposeMethodName, StringComparison.Ordinal)
            || context.ContainingSymbol.Name.Equals(disposeAsyncMethodName, StringComparison.Ordinal)
            || !IsComponent(context.ContainingSymbol.ContainingType, component))
        {
            return;
        }

        IInvocationOperation invocation = (IInvocationOperation)context.Operation;
        foreach (IArgumentOperation argument in invocation.Arguments)
        {
            if (SymbolEqualityComparer.Default.Equals(argument.Parameter?.Type, cancellation)
                && IsUncancelled(argument, cancellation))
            {
                context.ReportDiagnostic(Diagnostic.Create(
                    RazorDiagnosticCatalog.CancellationPropagationRule,
                    GetInvocationLocation(invocation.Syntax),
                    invocation.TargetMethod.Name));
                return;
            }
        }
    }

    /// <summary>
    /// 同じコンパイル内の ComponentBase を継承しているか判定します。
    /// </summary>
    /// <param name="type">呼出しを含む型です。</param>
    /// <param name="component">コンポーネントの基底型です。</param>
    /// <returns>基底型が一致した場合は true を返します。</returns>
    private static bool IsComponent(INamedTypeSymbol? type, INamedTypeSymbol component)
    {
        for (INamedTypeSymbol? current = type; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, component))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 省略値・default・CancellationToken.None を通常の伝搬と区別します。
    /// </summary>
    /// <param name="argument">呼出し引数です。</param>
    /// <param name="cancellation">取消トークン型です。</param>
    /// <returns>取消不能な指定の場合は true を返します。</returns>
    private static bool IsUncancelled(IArgumentOperation argument, INamedTypeSymbol cancellation)
    {
        IOperation value = argument.Value;
        while (value is IConversionOperation conversion)
        {
            value = conversion.Operand;
        }

        string nonePropertyName = nameof(CancellationToken.None);
        return argument.IsImplicit
            || value is IDefaultValueOperation
            || (value is IPropertyReferenceOperation property
                && property.Property.Name.Equals(nonePropertyName, StringComparison.Ordinal)
                && SymbolEqualityComparer.Default.Equals(property.Property.ContainingType, cancellation));
    }

    /// <summary>
    /// 修正対象となる呼出しのメソッド名を選択します。
    /// </summary>
    /// <param name="syntax">呼出し構文です。</param>
    /// <returns>メソッド名、または対応する呼出し式の位置です。</returns>
    private static Location GetInvocationLocation(SyntaxNode syntax)
    {
        if (syntax is InvocationExpressionSyntax invocation)
        {
            return invocation.Expression switch
            {
                MemberAccessExpressionSyntax access => access.Name.Identifier.GetLocation(),
                MemberBindingExpressionSyntax binding => binding.Name.Identifier.GetLocation(),
                SimpleNameSyntax name => name.Identifier.GetLocation(),
                _ => invocation.Expression.GetLocation(),
            };
        }

        return syntax.GetLocation();
    }
}
