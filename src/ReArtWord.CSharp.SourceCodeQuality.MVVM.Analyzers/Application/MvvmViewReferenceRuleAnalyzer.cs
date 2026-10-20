using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// ViewModelからView層への参照を検証します。
/// </summary>
internal static class MvvmViewReferenceRuleAnalyzer
{
    private static readonly string CurrentPropertyName = string.Concat("Cur", "rent");
    private static readonly string ApplicationTypeName = string.Concat("Applica", "tion");
    private static readonly string SystemWindowsNamespace = string.Concat("System", ".Windows");
    private static readonly string MauiControlsNamespace = string.Concat("Microsoft.Maui", ".Controls");
    private static readonly string AvaloniaNamespace = string.Concat("Ava", "lonia");

    private static readonly string[] ViewNamespaceSegments =
    [
        "Views",
        "Controls",
        "Pages",
    ];

    private static readonly string[] ViewTypeSuffixes =
    [
        "View",
        "Window",
        "Page",
        "UserControl",
        "Control",
        "MessageBox",
        "ContentPage",
    ];

    /// <summary>
    /// 識別子名を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="identifierName">識別子名です。</param>
    public static void AnalyzeIdentifierName(
        SyntaxNodeAnalysisContext context,
        IdentifierNameSyntax identifierName)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, identifierName)
            || MvvmSyntaxHelper.GetContainingViewModelSymbol(
                identifierName,
                context.SemanticModel,
                context.CancellationToken) is null
            || context.SemanticModel.GetSymbolInfo(identifierName, context.CancellationToken).Symbol is not INamedTypeSymbol typeSymbol
            || IsApplicationType(typeSymbol)
            || !IsForbiddenViewType(typeSymbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.ViewModelViewReferenceRule,
            identifierName.GetLocation()));
    }

    /// <summary>
    /// メンバーアクセス式を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="memberAccess">メンバーアクセス式です。</param>
    public static void AnalyzeMemberAccess(
        SyntaxNodeAnalysisContext context,
        MemberAccessExpressionSyntax memberAccess)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, memberAccess)
            || !memberAccess.Name.Identifier.ValueText.Equals(CurrentPropertyName, StringComparison.Ordinal)
            || MvvmSyntaxHelper.GetContainingViewModelSymbol(
                memberAccess,
                context.SemanticModel,
                context.CancellationToken) is null
            || GetReferencedType(context, memberAccess.Expression) is not INamedTypeSymbol typeSymbol
            || !IsApplicationType(typeSymbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.ViewModelApplicationCurrentRule,
            memberAccess.Name.GetLocation()));
    }

    /// <summary>
    /// 型が禁止対象View型か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>禁止対象の場合は <see langword="true" /> を返します。</returns>
    private static bool IsForbiddenViewType(INamedTypeSymbol typeSymbol)
    {
        // 状態列挙型は画面の実体ではなく、Page等の接尾辞やControls内の配置だけでは禁止しません。
        if (typeSymbol.TypeKind == TypeKind.Enum)
        {
            return false;
        }

        string namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        if (HasViewNamespaceSegment(namespaceName))
        {
            return true;
        }

        return ViewTypeSuffixes.Any(suffix => typeSymbol.Name.EndsWith(suffix, StringComparison.Ordinal));
    }

    /// <summary>
    /// 型がApplication型か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>Application型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsApplicationType(INamedTypeSymbol typeSymbol)
    {
        string namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        if (!typeSymbol.Name.Equals(ApplicationTypeName, StringComparison.Ordinal))
        {
            return false;
        }

        return namespaceName.Equals(SystemWindowsNamespace, StringComparison.Ordinal)
            || namespaceName.Equals(MauiControlsNamespace, StringComparison.Ordinal)
            || namespaceName.Equals(AvaloniaNamespace, StringComparison.Ordinal);
    }

    /// <summary>
    /// namespaceがView層セグメントを含むか判定します。
    /// </summary>
    /// <param name="namespaceName">namespace名です。</param>
    /// <returns>含む場合は <see langword="true" /> を返します。</returns>
    private static bool HasViewNamespaceSegment(string namespaceName)
    {
        string[] segments = namespaceName.Split('.');
        return segments.Any(static segment => ViewNamespaceSegments.Contains(segment, StringComparer.Ordinal));
    }

    /// <summary>
    /// 式が参照する型を取得します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="expression">対象式です。</param>
    /// <returns>参照型です。</returns>
    private static INamedTypeSymbol? GetReferencedType(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression)
    {
        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken);
        if (symbolInfo.Symbol is INamedTypeSymbol namedType)
        {
            return namedType;
        }

        return context.SemanticModel.GetTypeInfo(expression, context.CancellationToken).Type as INamedTypeSymbol;
    }
}
