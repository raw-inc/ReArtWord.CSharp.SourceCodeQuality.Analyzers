using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// const利用とawait時のConfigureAwait明示を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConstAndAwaitAnalyzer : DiagnosticAnalyzer
{
    private const string FieldConstRecommendation = "readonly または static readonly";
    private const string LocalConstRecommendation = "通常のローカル変数";

    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.ConstUsageRecommendationRule,
            DiagnosticCatalog.AwaitConfigureAwaitRecommendationRule);

    /// <summary>
    /// Analyzer初期化処理です。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeLocalDeclaration, SyntaxKind.LocalDeclarationStatement);
        context.RegisterSyntaxNodeAction(AnalyzeAwaitExpression, SyntaxKind.AwaitExpression);
    }

    /// <summary>
    /// フィールド宣言におけるconst利用を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not FieldDeclarationSyntax fieldDeclaration)
        {
            return;
        }

        if (!fieldDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.ConstKeyword)))
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(fieldDeclaration.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        SyntaxToken constKeyword = GetConstKeyword(fieldDeclaration.Modifiers);
        foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.ConstUsageRecommendationRule,
                    constKeyword.GetLocation(),
                    variable.Identifier.Text,
                    FieldConstRecommendation));
        }
    }

    /// <summary>
    /// ローカル宣言におけるconst利用を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeLocalDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalDeclarationStatementSyntax localDeclaration)
        {
            return;
        }

        if (!localDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.ConstKeyword)))
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(localDeclaration.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        SyntaxToken constKeyword = GetConstKeyword(localDeclaration.Modifiers);
        foreach (VariableDeclaratorSyntax variable in localDeclaration.Declaration.Variables)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.ConstUsageRecommendationRule,
                    constKeyword.GetLocation(),
                    variable.Identifier.Text,
                    LocalConstRecommendation));
        }
    }

    /// <summary>
    /// constキーワードを取得します。
    /// </summary>
    /// <param name="modifiers">修飾子一覧です。</param>
    /// <returns>constキーワードです。</returns>
    private static SyntaxToken GetConstKeyword(SyntaxTokenList modifiers)
    {
        return modifiers.First(static modifier => modifier.IsKind(SyntaxKind.ConstKeyword));
    }

    /// <summary>
    /// await式のConfigureAwait明示有無を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeAwaitExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AwaitExpressionSyntax awaitExpression)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(awaitExpression.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (HasConfigureAwaitInvocation(awaitExpression.Expression, context))
        {
            return;
        }

        if (!TryGetTaskLikeTypeDisplay(awaitExpression.Expression, context, out string taskTypeDisplay))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.AwaitConfigureAwaitRecommendationRule,
                awaitExpression.AwaitKeyword.GetLocation(),
                taskTypeDisplay));
    }

    /// <summary>
    /// ConfigureAwait呼び出し済みか判定します。
    /// </summary>
    /// <param name="expression">await対象式です。</param>
    /// <param name="context">解析コンテキストです。</param>
    /// <returns>ConfigureAwait呼び出し済みの場合は <see langword="true" /> を返します。</returns>
    private static bool HasConfigureAwaitInvocation(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
    {
        if (expression is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        if (!TryGetInvocationMethodSymbol(context, invocation, out IMethodSymbol? methodSymbol)
            || methodSymbol is null)
        {
            return false;
        }

        if (!methodSymbol.Name.Equals("ConfigureAwait", StringComparison.Ordinal)
            || methodSymbol.Parameters.Length != 1
            || methodSymbol.Parameters[0].Type.SpecialType != SpecialType.System_Boolean)
        {
            return false;
        }

        if (invocation.Expression is not MemberAccessExpressionSyntax memberAccess)
        {
            return false;
        }

        ITypeSymbol? receiverType = context.SemanticModel.GetTypeInfo(memberAccess.Expression, context.CancellationToken).Type;
        return IsTaskLikeType(receiverType);
    }

    /// <summary>
    /// await対象がTask/ValueTask系か判定し、表示用型名を取得します。
    /// </summary>
    /// <param name="expression">await対象式です。</param>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="taskTypeDisplay">表示用型名です。</param>
    /// <returns>Task/ValueTask系の場合は <see langword="true" /> を返します。</returns>
    private static bool TryGetTaskLikeTypeDisplay(
        ExpressionSyntax expression,
        SyntaxNodeAnalysisContext context,
        out string taskTypeDisplay)
    {
        taskTypeDisplay = string.Empty;
        ITypeSymbol? expressionType = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken).Type;
        if (!IsTaskLikeType(expressionType))
        {
            return false;
        }

        taskTypeDisplay = expressionType!.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat);
        return true;
    }

    /// <summary>
    /// Task/ValueTask系の型か判定します。
    /// </summary>
    /// <param name="typeSymbol">判定対象型です。</param>
    /// <returns>対象型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsTaskLikeType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
        {
            return false;
        }

        INamedTypeSymbol normalizedType = namedType.ConstructedFrom;
        if (!normalizedType.ContainingNamespace.ToDisplayString().Equals("System.Threading.Tasks", StringComparison.Ordinal))
        {
            return false;
        }

        return normalizedType.Name.Equals("Task", StringComparison.Ordinal)
            || normalizedType.Name.Equals("ValueTask", StringComparison.Ordinal);
    }

    /// <summary>
    /// 呼び出し式のメソッドシンボルを取得します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="invocation">呼び出し式です。</param>
    /// <param name="methodSymbol">取得結果です。</param>
    /// <returns>取得成功時は <see langword="true" /> を返します。</returns>
    private static bool TryGetInvocationMethodSymbol(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        out IMethodSymbol? methodSymbol)
    {
        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        methodSymbol = symbolInfo.Symbol as IMethodSymbol
            ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
        return methodSymbol is not null;
    }
}
