using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// RelayCommand属性対象メソッドの規約を検証します。
/// </summary>
internal static class MvvmRelayCommandRuleAnalyzer
{
    private static readonly string CommandSuffix = string.Concat("Com", "mand");
    private static readonly string AllowConcurrentExecutionsPropertyName = string.Concat("AllowConcurrent", "Executions");

    /// <summary>
    /// メソッド宣言を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    public static void AnalyzeMethodDeclaration(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration)
    {
        if (!MvvmSyntaxHelper.IsSyntaxTarget(context, methodDeclaration))
        {
            return;
        }

        ImmutableArray<AttributeSyntax> relayCommandAttributes = MvvmSyntaxHelper.GetMatchingAttributes(
            methodDeclaration.AttributeLists,
            context.SemanticModel,
            MvvmSymbolHelper.IsRelayCommandAttribute,
            context.CancellationToken).ToImmutableArray();
        if (relayCommandAttributes.IsEmpty)
        {
            return;
        }

        IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(
            methodDeclaration,
            context.CancellationToken);
        AnalyzeMethodName(context, methodDeclaration);
        AnalyzeMethodAccessibility(context, methodDeclaration, methodSymbol);
        AnalyzeReturnType(context, methodDeclaration, methodSymbol);
        AnalyzeConcurrentExecution(context, methodDeclaration, methodSymbol, relayCommandAttributes);
    }

    /// <summary>
    /// RelayCommand対象メソッド名を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    private static void AnalyzeMethodName(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration)
    {
        if (!methodDeclaration.Identifier.ValueText.EndsWith(CommandSuffix, StringComparison.Ordinal))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.RelayCommandMethodNameRule,
            methodDeclaration.Identifier.GetLocation()));
    }

    /// <summary>
    /// RelayCommand対象メソッドのアクセス修飾を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    private static void AnalyzeMethodAccessibility(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration,
        IMethodSymbol? methodSymbol)
    {
        if (methodSymbol?.DeclaredAccessibility == Accessibility.Private)
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.RelayCommandPrivateMethodRule,
            methodDeclaration.Identifier.GetLocation()));
    }

    /// <summary>
    /// RelayCommand対象メソッドの戻り値を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    private static void AnalyzeReturnType(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration,
        IMethodSymbol? methodSymbol)
    {
        if (methodSymbol is null
            || methodSymbol.ReturnsVoid
            || MvvmViewModelSymbolHelper.IsNonGenericTaskOrValueTask(methodSymbol.ReturnType))
        {
            return;
        }

        context.ReportDiagnostic(Diagnostic.Create(
            MvvmDiagnosticCatalog.RelayCommandReturnTypeRule,
            methodDeclaration.ReturnType.GetLocation()));
    }

    /// <summary>
    /// RelayCommandの並列実行許可コメントを検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    /// <param name="relayCommandAttributes">RelayCommand属性一覧です。</param>
    private static void AnalyzeConcurrentExecution(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration,
        IMethodSymbol? methodSymbol,
        IEnumerable<AttributeSyntax> relayCommandAttributes)
    {
        if (!IsAsyncCommandMethod(methodDeclaration, methodSymbol))
        {
            return;
        }

        foreach (AttributeSyntax attribute in relayCommandAttributes)
        {
            Location? trueLocation = GetAllowConcurrentExecutionsTrueLocation(attribute);
            if (trueLocation is null
                || attribute.Parent is not AttributeListSyntax attributeList
                || MvvmSyntaxHelper.HasJapaneseLineCommentImmediatelyBefore(attributeList, context.CancellationToken))
            {
                continue;
            }

            context.ReportDiagnostic(Diagnostic.Create(
                MvvmDiagnosticCatalog.RelayCommandConcurrentExecutionCommentRule,
                trueLocation));
        }
    }

    /// <summary>
    /// asyncなCommandメソッドか判定します。
    /// </summary>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    /// <returns>async Commandの場合は <see langword="true" /> を返します。</returns>
    private static bool IsAsyncCommandMethod(
        MethodDeclarationSyntax methodDeclaration,
        IMethodSymbol? methodSymbol)
    {
        return methodDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.AsyncKeyword))
            || MvvmViewModelSymbolHelper.IsTaskOrValueTask(methodSymbol?.ReturnType);
    }

    /// <summary>
    /// AllowConcurrentExecutions = true の位置を取得します。
    /// </summary>
    /// <param name="attribute">RelayCommand属性です。</param>
    /// <returns>true指定の位置です。</returns>
    private static Location? GetAllowConcurrentExecutionsTrueLocation(AttributeSyntax attribute)
    {
        if (attribute.ArgumentList is null)
        {
            return null;
        }

        foreach (AttributeArgumentSyntax argument in attribute.ArgumentList.Arguments)
        {
            if (argument.NameEquals?.Name.Identifier.ValueText.Equals(AllowConcurrentExecutionsPropertyName, StringComparison.Ordinal) == true
                && argument.Expression.IsKind(SyntaxKind.TrueLiteralExpression))
            {
                return argument.Expression.GetLocation();
            }
        }

        return null;
    }
}
