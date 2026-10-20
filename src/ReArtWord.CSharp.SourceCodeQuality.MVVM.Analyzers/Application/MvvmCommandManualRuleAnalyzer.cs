using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// 手動Command実装を検証します。
/// </summary>
internal static class MvvmCommandManualRuleAnalyzer
{
    /// <summary>
    /// field宣言を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="fieldDeclaration">field宣言です。</param>
    public static void AnalyzeFieldDeclaration(
        SyntaxNodeAnalysisContext context,
        FieldDeclarationSyntax fieldDeclaration)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, fieldDeclaration))
        {
            return;
        }

        ITypeSymbol? typeSymbol = context.SemanticModel.GetTypeInfo(
            fieldDeclaration.Declaration.Type,
            context.CancellationToken).Type;
        if (!MvvmSymbolHelper.IsManualCommandType(typeSymbol))
        {
            return;
        }

        foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MvvmDiagnosticCatalog.RelayCommandRequiredRule,
                variable.Identifier.GetLocation()));
        }
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

        ITypeSymbol? typeSymbol = context.SemanticModel.GetTypeInfo(
            propertyDeclaration.Type,
            context.CancellationToken).Type;
        if (!MvvmSymbolHelper.IsManualCommandType(typeSymbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.RelayCommandRequiredRule,
            propertyDeclaration.Identifier.GetLocation()));
    }

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
            || IsInsideCommandMemberDeclaration(objectCreation))
        {
            return;
        }

        ITypeSymbol? typeSymbol = GetObjectCreationTypeSymbol(
            objectCreation,
            context.SemanticModel,
            context.CancellationToken);
        if (!MvvmSymbolHelper.IsManualCommandCreationType(typeSymbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.RelayCommandRequiredRule,
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
            || IsInsideCommandMemberDeclaration(objectCreation))
        {
            return;
        }

        ITypeSymbol? typeSymbol = context.SemanticModel.GetTypeInfo(
            objectCreation,
            context.CancellationToken).Type;
        if (!MvvmSymbolHelper.IsManualCommandCreationType(typeSymbol))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.RelayCommandRequiredRule,
            objectCreation.NewKeyword.GetLocation()));
    }

    /// <summary>
    /// ObjectCreationがCommandメンバー宣言内にあるか判定します。
    /// </summary>
    /// <param name="objectCreation">生成式です。</param>
    /// <returns>Commandメンバー宣言内の場合は <see langword="true" /> を返します。</returns>
    private static bool IsInsideCommandMemberDeclaration(SyntaxNode objectCreation)
    {
        return objectCreation.Ancestors().Any(static ancestor =>
            ancestor is FieldDeclarationSyntax or PropertyDeclarationSyntax);
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
