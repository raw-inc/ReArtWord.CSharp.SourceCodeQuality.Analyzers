using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// ViewModelクラス単位の規約を検証します。
/// </summary>
internal static class MvvmClassRuleAnalyzer
{
    /// <summary>
    /// クラス宣言を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="classDeclaration">クラス宣言です。</param>
    public static void AnalyzeClassDeclaration(
        SyntaxNodeAnalysisContext context,
        ClassDeclarationSyntax classDeclaration)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, classDeclaration))
        {
            return;
        }

        INamedTypeSymbol? typeSymbol = context.SemanticModel.GetDeclaredSymbol(
            classDeclaration,
            context.CancellationToken) as INamedTypeSymbol;
        if (typeSymbol is null || MvvmViewModelSymbolHelper.IsToolkitOwnedType(typeSymbol))
        {
            return;
        }

        AnalyzeViewModelNaming(context, classDeclaration, typeSymbol);
        AnalyzeDirectNotifyImplementation(context, classDeclaration);
        AnalyzeSealedOrAbstract(context, classDeclaration, typeSymbol);
    }

    /// <summary>
    /// ViewModel命名と基底型を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="classDeclaration">クラス宣言です。</param>
    /// <param name="typeSymbol">クラスの型シンボルです。</param>
    private static void AnalyzeViewModelNaming(
        SyntaxNodeAnalysisContext context,
        ClassDeclarationSyntax classDeclaration,
        INamedTypeSymbol typeSymbol)
    {
        bool hasViewModelSuffix = MvvmViewModelSymbolHelper.HasViewModelSuffix(typeSymbol);
        bool hasObservableBase = MvvmViewModelSymbolHelper.HasToolkitObservableBase(typeSymbol);
        Location identifierLocation = classDeclaration.Identifier.GetLocation();
        if (hasObservableBase && !hasViewModelSuffix)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MvvmDiagnosticCatalog.ViewModelSuffixRequiredRule,
                identifierLocation));
        }

        if (hasViewModelSuffix && !hasObservableBase)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MvvmDiagnosticCatalog.ViewModelObservableBaseRequiredRule,
                identifierLocation));
        }
    }

    /// <summary>
    /// INotifyPropertyChanged直接実装を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="classDeclaration">クラス宣言です。</param>
    private static void AnalyzeDirectNotifyImplementation(
        SyntaxNodeAnalysisContext context,
        ClassDeclarationSyntax classDeclaration)
    {
        if (classDeclaration.BaseList is null)
        {
            return;
        }

        foreach (BaseTypeSyntax baseType in classDeclaration.BaseList.Types)
        {
            ITypeSymbol? typeSymbol = context.SemanticModel.GetTypeInfo(
                baseType.Type,
                context.CancellationToken).Type;
            if (!MvvmViewModelSymbolHelper.IsNotifyPropertyChangedInterface(typeSymbol))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                MvvmDiagnosticCatalog.DirectNotifyPropertyChangedImplementationRule,
                baseType.Type.GetLocation()));
        }
    }

    /// <summary>
    /// ViewModelのsealed/abstract明示を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="classDeclaration">クラス宣言です。</param>
    /// <param name="typeSymbol">クラスの型シンボルです。</param>
    private static void AnalyzeSealedOrAbstract(
        SyntaxNodeAnalysisContext context,
        ClassDeclarationSyntax classDeclaration,
        INamedTypeSymbol typeSymbol)
    {
        if (!MvvmViewModelSymbolHelper.IsViewModel(typeSymbol)
            || typeSymbol.IsSealed
            || typeSymbol.IsAbstract)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.ViewModelSealedOrAbstractRule,
            classDeclaration.Identifier.GetLocation()));
    }
}
