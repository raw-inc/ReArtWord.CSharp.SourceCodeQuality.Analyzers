using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// 変更通知に関するMVVM規約を検証します。
/// </summary>
internal static class MvvmNotificationRuleAnalyzer
{
    private static readonly string InvokeMethodName = string.Concat("Inv", "oke");
    private static readonly string OnPropertyChangedMethodName = string.Concat("On", "PropertyChanged");
    private static readonly string NameofExpressionName = string.Concat("name", "of");
    private static readonly string PropertyChangedEventName = string.Concat("Property", "Changed");
    private static readonly string PropertyChangingEventName = string.Concat("Property", "Changing");

    /// <summary>
    /// 呼び出し式を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="invocation">呼び出し式です。</param>
    public static void AnalyzeInvocation(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, invocation))
        {
            return;
        }

        IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(
            invocation,
            context.CancellationToken).Symbol as IMethodSymbol;
        Location nameLocation = MvvmSyntaxHelper.GetInvocationNameLocation(invocation);
        if (methodSymbol is not null && MvvmSymbolHelper.IsToolkitSetProperty(methodSymbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(MvvmDiagnosticCatalog.ObservablePropertyRequiredRule, nameLocation));
            context.ReportDiagnostic(Diagnostic.Create(MvvmDiagnosticCatalog.PartialHookRequiredRule, nameLocation));
            return;
        }

        if (methodSymbol is not null && MvvmSymbolHelper.IsToolkitOnPropertyChanged(methodSymbol))
        {
            context.ReportDiagnostic(Diagnostic.Create(MvvmDiagnosticCatalog.NotifyPropertyChangedForRequiredRule, nameLocation));
        }

        AnalyzeNotifyPropertyChangedDependency(context, invocation, methodSymbol, nameLocation);
        AnalyzeManualNotifyEventInvocation(context, invocation);
    }

    /// <summary>
    /// 条件付きアクセス式を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="conditionalAccess">条件付きアクセス式です。</param>
    public static void AnalyzeConditionalAccess(
        SyntaxNodeAnalysisContext context,
        ConditionalAccessExpressionSyntax conditionalAccess)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, conditionalAccess)
            || conditionalAccess.WhenNotNull is not InvocationExpressionSyntax invocation
            || invocation.Expression is not MemberBindingExpressionSyntax memberBinding
            || !memberBinding.Name.Identifier.ValueText.Equals(InvokeMethodName, StringComparison.Ordinal)
            || !IsNotifyEventExpression(context, conditionalAccess.Expression))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.ManualPropertyChangedInvocationRule,
            GetExpressionNameLocation(conditionalAccess.Expression)));
    }

    /// <summary>
    /// 代入式を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="assignment">代入式です。</param>
    public static void AnalyzeAssignment(
        SyntaxNodeAnalysisContext context,
        AssignmentExpressionSyntax assignment)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, assignment)
            || !assignment.IsKind(SyntaxKind.AddAssignmentExpression)
            && !assignment.IsKind(SyntaxKind.SubtractAssignmentExpression)
            || MvvmSyntaxHelper.GetContainingViewModelSymbol(
                assignment,
                context.SemanticModel,
                context.CancellationToken) is null
            || !IsNotifyEventExpression(context, assignment.Left))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.PropertyChangedSubscriptionRule,
            GetExpressionNameLocation(assignment.Left)));
    }

    /// <summary>
    /// OnPropertyChanged(nameof(...))による依存通知を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="invocation">呼び出し式です。</param>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    /// <param name="nameLocation">呼び出し名位置です。</param>
    private static void AnalyzeNotifyPropertyChangedDependency(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        IMethodSymbol? methodSymbol,
        Location nameLocation)
    {
        if (!IsOnPropertyChangedInvocation(invocation, methodSymbol)
            || MvvmSyntaxHelper.GetContainingViewModelSymbol(
                invocation,
                context.SemanticModel,
                context.CancellationToken) is null
            || GetNameofPropertySymbol(invocation, context.SemanticModel, context.CancellationToken) is not IPropertySymbol propertySymbol
            || propertySymbol.DeclaredAccessibility != Accessibility.Public)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.NotifyPropertyChangedForDependencyRule,
            nameLocation));
    }

    /// <summary>
    /// 通知イベントの手動発火を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="invocation">呼び出し式です。</param>
    private static void AnalyzeManualNotifyEventInvocation(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation)
    {
        if (invocation.Parent is ConditionalAccessExpressionSyntax)
        {
            return;
        }

        ExpressionSyntax? eventExpression = GetInvokedNotifyEventExpression(invocation);
        if (eventExpression is null || !IsNotifyEventExpression(context, eventExpression))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.ManualPropertyChangedInvocationRule,
            GetExpressionNameLocation(eventExpression)));
    }

    /// <summary>
    /// 呼び出しがOnPropertyChangedか判定します。
    /// </summary>
    /// <param name="invocation">呼び出し式です。</param>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    /// <returns>OnPropertyChanged呼び出しの場合は <see langword="true" /> を返します。</returns>
    private static bool IsOnPropertyChangedInvocation(
        InvocationExpressionSyntax invocation,
        IMethodSymbol? methodSymbol)
    {
        if (methodSymbol is not null && methodSymbol.Name.Equals(OnPropertyChangedMethodName, StringComparison.Ordinal))
        {
            return true;
        }

        return invocation.Expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText.Equals(OnPropertyChangedMethodName, StringComparison.Ordinal),
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText.Equals(OnPropertyChangedMethodName, StringComparison.Ordinal),
            _ => false,
        };
    }

    /// <summary>
    /// nameof対象のpublic propertyシンボルを取得します。
    /// </summary>
    /// <param name="invocation">呼び出し式です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>propertyシンボルです。</returns>
    private static IPropertySymbol? GetNameofPropertySymbol(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (invocation.ArgumentList.Arguments.Count != 1
            || invocation.ArgumentList.Arguments[0].Expression is not InvocationExpressionSyntax nameofInvocation
            || nameofInvocation.Expression is not IdentifierNameSyntax nameofIdentifier
            || !nameofIdentifier.Identifier.ValueText.Equals(NameofExpressionName, StringComparison.Ordinal)
            || nameofInvocation.ArgumentList.Arguments.Count != 1)
        {
            return null;
        }

        ExpressionSyntax expression = nameofInvocation.ArgumentList.Arguments[0].Expression;
        return semanticModel.GetSymbolInfo(expression, cancellationToken).Symbol as IPropertySymbol;
    }

    /// <summary>
    /// 呼び出された通知イベント式を取得します。
    /// </summary>
    /// <param name="invocation">呼び出し式です。</param>
    /// <returns>通知イベント候補式です。</returns>
    private static ExpressionSyntax? GetInvokedNotifyEventExpression(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            IdentifierNameSyntax identifier => identifier,
            MemberAccessExpressionSyntax memberAccess
                when memberAccess.Name.Identifier.ValueText.Equals(InvokeMethodName, StringComparison.Ordinal) => memberAccess.Expression,
            _ => null,
        };
    }

    /// <summary>
    /// 式が通知イベントを表すか判定します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="expression">対象式です。</param>
    /// <returns>通知イベントの場合は <see langword="true" /> を返します。</returns>
    private static bool IsNotifyEventExpression(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression)
    {
        ISymbol? symbol = context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken).Symbol;
        if (MvvmViewModelSymbolHelper.IsNotifyEventSymbol(symbol))
        {
            return true;
        }

        return symbol is null && GetExpressionName(expression) is string name && IsNotifyEventName(name);
    }

    /// <summary>
    /// 通知イベント名か判定します。
    /// </summary>
    /// <param name="name">名前です。</param>
    /// <returns>通知イベント名の場合は <see langword="true" /> を返します。</returns>
    private static bool IsNotifyEventName(string name)
    {
        return name.Equals(PropertyChangedEventName, StringComparison.Ordinal)
            || name.Equals(PropertyChangingEventName, StringComparison.Ordinal);
    }

    /// <summary>
    /// 式名を取得します。
    /// </summary>
    /// <param name="expression">対象式です。</param>
    /// <returns>式名です。</returns>
    private static string? GetExpressionName(ExpressionSyntax expression)
    {
        return expression switch
        {
            IdentifierNameSyntax identifier => identifier.Identifier.ValueText,
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.Identifier.ValueText,
            _ => null,
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
