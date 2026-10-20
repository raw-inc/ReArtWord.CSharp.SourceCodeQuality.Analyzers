using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// ObservablePropertyとViewModelプロパティ規約を検証します。
/// </summary>
internal static class MvvmPropertyRuleAnalyzer
{
    private static readonly string OnPropertyChangedMethodName = string.Concat("On", "PropertyChanged");
    private static readonly string PropertyChangedEventName = string.Concat("Property", "Changed");
    private static readonly string PropertyChangingEventName = string.Concat("Property", "Changing");

    /// <summary>
    /// field宣言を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="fieldDeclaration">field宣言です。</param>
    public static void AnalyzeFieldDeclaration(
        SyntaxNodeAnalysisContext context,
        FieldDeclarationSyntax fieldDeclaration)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, fieldDeclaration)
            || !HasObservablePropertyAttribute(context, fieldDeclaration.AttributeLists))
        {
            return;
        }

        AnalyzeObservablePropertyFieldAccess(context, fieldDeclaration);
        AnalyzeObservablePropertyFieldModifiers(context, fieldDeclaration);
        AnalyzeObservablePropertyCollection(context, fieldDeclaration.Declaration.Type);
    }

    /// <summary>
    /// property宣言を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="propertyDeclaration">property宣言です。</param>
    public static void AnalyzePropertyDeclaration(
        SyntaxNodeAnalysisContext context,
        PropertyDeclarationSyntax propertyDeclaration)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, propertyDeclaration))
        {
            return;
        }

        if (HasObservablePropertyAttribute(context, propertyDeclaration.AttributeLists))
        {
            AnalyzeObservablePropertyCollection(context, propertyDeclaration.Type);
            return;
        }

        AnalyzePublicPropertyNotification(context, propertyDeclaration);
    }

    /// <summary>
    /// ObservableProperty対象fieldのアクセス修飾を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="fieldDeclaration">field宣言です。</param>
    private static void AnalyzeObservablePropertyFieldAccess(
        SyntaxNodeAnalysisContext context,
        FieldDeclarationSyntax fieldDeclaration)
    {
        foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
        {
            if (context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) is not IFieldSymbol fieldSymbol
                || fieldSymbol.DeclaredAccessibility == Accessibility.Private)
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                MvvmDiagnosticCatalog.ObservablePropertyPrivateFieldRule,
                variable.Identifier.GetLocation()));
        }
    }

    /// <summary>
    /// ObservableProperty対象fieldのstatic/readonlyを検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="fieldDeclaration">field宣言です。</param>
    private static void AnalyzeObservablePropertyFieldModifiers(
        SyntaxNodeAnalysisContext context,
        FieldDeclarationSyntax fieldDeclaration)
    {
        foreach (SyntaxToken modifier in fieldDeclaration.Modifiers)
        {
            if (!modifier.IsKind(SyntaxKind.StaticKeyword)
                && !modifier.IsKind(SyntaxKind.ReadOnlyKeyword))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                MvvmDiagnosticCatalog.ObservablePropertyInstanceMutableFieldRule,
                modifier.GetLocation()));
        }
    }

    /// <summary>
    /// ObservableProperty対象型のコレクション種別を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="typeSyntax">型構文です。</param>
    private static void AnalyzeObservablePropertyCollection(
        SyntaxNodeAnalysisContext context,
        TypeSyntax typeSyntax)
    {
        ITypeSymbol? typeSymbol = context.SemanticModel.GetTypeInfo(typeSyntax, context.CancellationToken).Type;
        if (!MvvmViewModelSymbolHelper.IsObservablePropertyEnumerableType(typeSymbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.ObservablePropertyCollectionRule,
            typeSyntax.GetLocation()));
    }

    /// <summary>
    /// ViewModelのpublic可変プロパティに変更通知があるか検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="propertyDeclaration">property宣言です。</param>
    private static void AnalyzePublicPropertyNotification(
        SyntaxNodeAnalysisContext context,
        PropertyDeclarationSyntax propertyDeclaration)
    {
        if (MvvmSyntaxHelper.GetContainingViewModelSymbol(
                propertyDeclaration,
                context.SemanticModel,
                context.CancellationToken) is null
            || context.SemanticModel.GetDeclaredSymbol(propertyDeclaration, context.CancellationToken) is not IPropertySymbol propertySymbol
            || propertySymbol.DeclaredAccessibility != Accessibility.Public
            || propertySymbol.IsStatic
            || propertySymbol.IsAbstract
            || !HasMutableAccessor(propertyDeclaration)
            || HasSetterNotification(propertyDeclaration, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.ViewModelPublicPropertyNotificationRule,
            propertyDeclaration.Identifier.GetLocation()));
    }

    /// <summary>
    /// ObservableProperty属性を持つか判定します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="attributeLists">属性一覧です。</param>
    /// <returns>属性を持つ場合は <see langword="true" /> を返します。</returns>
    private static bool HasObservablePropertyAttribute(
        SyntaxNodeAnalysisContext context,
        SyntaxList<AttributeListSyntax> attributeLists)
    {
        return MvvmSyntaxHelper.HasMatchingAttribute(
            attributeLists,
            context.SemanticModel,
            MvvmSymbolHelper.IsObservablePropertyAttribute,
            context.CancellationToken);
    }

    /// <summary>
    /// set/initアクセサを持つか判定します。
    /// </summary>
    /// <param name="propertyDeclaration">property宣言です。</param>
    /// <returns>可変アクセサを持つ場合は <see langword="true" /> を返します。</returns>
    private static bool HasMutableAccessor(PropertyDeclarationSyntax propertyDeclaration)
    {
        return propertyDeclaration.AccessorList?.Accessors.Any(static accessor =>
            accessor.IsKind(SyntaxKind.SetAccessorDeclaration)
            || accessor.IsKind(SyntaxKind.InitAccessorDeclaration)) == true;
    }

    /// <summary>
    /// setter内に変更通知処理があるか判定します。
    /// </summary>
    /// <param name="propertyDeclaration">property宣言です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>通知処理がある場合は <see langword="true" /> を返します。</returns>
    private static bool HasSetterNotification(
        PropertyDeclarationSyntax propertyDeclaration,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        AccessorDeclarationSyntax? setter = propertyDeclaration.AccessorList?.Accessors.FirstOrDefault(static accessor =>
            accessor.IsKind(SyntaxKind.SetAccessorDeclaration)
            || accessor.IsKind(SyntaxKind.InitAccessorDeclaration));
        if (setter is null || IsAutoAccessor(setter))
        {
            return false;
        }

        return HasNotificationInvocation(setter, semanticModel, cancellationToken)
            || HasNotifyEventIdentifier(setter);
    }

    /// <summary>
    /// 自動アクセサか判定します。
    /// </summary>
    /// <param name="accessor">アクセサです。</param>
    /// <returns>自動アクセサの場合は <see langword="true" /> を返します。</returns>
    private static bool IsAutoAccessor(AccessorDeclarationSyntax accessor)
    {
        return accessor.Body is null
            && accessor.ExpressionBody is null;
    }

    /// <summary>
    /// setter内に通知呼び出しがあるか判定します。
    /// </summary>
    /// <param name="setter">setterです。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>通知呼び出しがある場合は <see langword="true" /> を返します。</returns>
    private static bool HasNotificationInvocation(
        AccessorDeclarationSyntax setter,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        IEnumerable<InvocationExpressionSyntax> invocations = setter.DescendantNodes()
            .OfType<InvocationExpressionSyntax>();
        foreach (InvocationExpressionSyntax invocation in invocations)
        {
            if (semanticModel.GetSymbolInfo(invocation, cancellationToken).Symbol is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (MvvmSymbolHelper.IsToolkitSetProperty(methodSymbol)
                || MvvmSymbolHelper.IsToolkitOnPropertyChanged(methodSymbol)
                || methodSymbol.Name.Equals(OnPropertyChangedMethodName, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// setter内に通知イベント識別子があるか判定します。
    /// </summary>
    /// <param name="setter">setterです。</param>
    /// <returns>通知イベント識別子がある場合は <see langword="true" /> を返します。</returns>
    private static bool HasNotifyEventIdentifier(AccessorDeclarationSyntax setter)
    {
        return setter.DescendantNodes()
            .OfType<IdentifierNameSyntax>()
            .Any(static identifier =>
                identifier.Identifier.ValueText.Equals(PropertyChangedEventName, StringComparison.Ordinal)
                || identifier.Identifier.ValueText.Equals(PropertyChangingEventName, StringComparison.Ordinal));
    }
}
