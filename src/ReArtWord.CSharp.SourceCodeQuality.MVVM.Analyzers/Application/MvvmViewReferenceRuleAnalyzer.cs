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

    /// <summary>
    /// 型への参照を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="typeName">参照する型名です。</param>
    public static void AnalyzeTypeName(
        SyntaxNodeAnalysisContext context,
        SimpleNameSyntax typeName)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, typeName)
            || MvvmSyntaxHelper.GetContainingViewModelSymbol(
                typeName,
                context.SemanticModel,
                context.CancellationToken) is null
            || context.SemanticModel.GetSymbolInfo(typeName, context.CancellationToken).Symbol is not INamedTypeSymbol typeSymbol
            || IsApplicationType(typeSymbol)
            || !IsForbiddenViewType(typeSymbol, context.Compilation))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.ViewModelViewReferenceRule,
            typeName.Identifier.GetLocation()));
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
    /// UIの基底型と継承関係から、禁止対象の型か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <param name="compilation">参照先の型を解決するコンパイルです。</param>
    /// <returns>禁止対象の場合は <see langword="true" /> を返します。</returns>
    private static bool IsForbiddenViewType(INamedTypeSymbol typeSymbol, Compilation compilation)
    {
        if (typeSymbol.TypeKind != TypeKind.Class)
        {
            return false;
        }

        INamedTypeSymbol? messageBox = compilation.GetTypeByMetadataName("System.Windows.MessageBox");
        if (SymbolEqualityComparer.Default.Equals(typeSymbol, messageBox))
        {
            return true;
        }

        INamedTypeSymbol? dependencyObject = compilation.GetTypeByMetadataName("System.Windows.DependencyObject");
        INamedTypeSymbol? mauiElement = compilation.GetTypeByMetadataName("Microsoft.Maui.Controls.Element");
        INamedTypeSymbol? avaloniaObject = compilation.GetTypeByMetadataName("Avalonia.AvaloniaObject");
        for (INamedTypeSymbol? current = typeSymbol; current is not null; current = current.BaseType)
        {
            if (SymbolEqualityComparer.Default.Equals(current, dependencyObject)
                || SymbolEqualityComparer.Default.Equals(current, mauiElement)
                || SymbolEqualityComparer.Default.Equals(current, avaloniaObject))
            {
                return true;
            }
        }

        return false;
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
