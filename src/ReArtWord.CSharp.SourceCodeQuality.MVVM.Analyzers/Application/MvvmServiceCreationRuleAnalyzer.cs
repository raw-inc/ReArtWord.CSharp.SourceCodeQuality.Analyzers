using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// ViewModel内Service直接生成を検証します。
/// </summary>
internal static class MvvmServiceCreationRuleAnalyzer
{
    /// <summary>
    /// ObjectCreation式を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="objectCreation">ObjectCreation式です。</param>
    public static void AnalyzeObjectCreation(
        SyntaxNodeAnalysisContext context,
        ObjectCreationExpressionSyntax objectCreation)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, objectCreation)
            || MvvmSyntaxHelper.GetContainingViewModelSymbol(
                objectCreation,
                context.SemanticModel,
                context.CancellationToken) is null)
        {
            return;
        }

        ITypeSymbol? typeSymbol = GetObjectCreationTypeSymbol(
            objectCreation,
            context.SemanticModel,
            context.CancellationToken);
        if (!MvvmViewModelSymbolHelper.IsServiceCreationType(typeSymbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.ViewModelServiceCreationRule,
            objectCreation.Type.GetLocation()));
    }

    /// <summary>
    /// target-typed ObjectCreation式を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="objectCreation">target-typed ObjectCreation式です。</param>
    public static void AnalyzeImplicitObjectCreation(
        SyntaxNodeAnalysisContext context,
        ImplicitObjectCreationExpressionSyntax objectCreation)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, objectCreation)
            || MvvmSyntaxHelper.GetContainingViewModelSymbol(
                objectCreation,
                context.SemanticModel,
                context.CancellationToken) is null)
        {
            return;
        }

        ITypeSymbol? typeSymbol = context.SemanticModel.GetTypeInfo(
            objectCreation,
            context.CancellationToken).Type;
        if (!MvvmViewModelSymbolHelper.IsServiceCreationType(typeSymbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.ViewModelServiceCreationRule,
            objectCreation.NewKeyword.GetLocation()));
    }

    /// <summary>
    /// ObjectCreationの生成型を取得します。
    /// </summary>
    /// <param name="objectCreation">生成式です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>取得した型シンボルです。</returns>
    private static ITypeSymbol? GetObjectCreationTypeSymbol(
        ObjectCreationExpressionSyntax objectCreation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        TypeInfo typeInfo = semanticModel.GetTypeInfo(objectCreation.Type, cancellationToken);
        if (typeInfo.Type is not null)
        {
            return typeInfo.Type;
        }

        return semanticModel.GetSymbolInfo(objectCreation.Type, cancellationToken).Symbol as ITypeSymbol;
    }
}
