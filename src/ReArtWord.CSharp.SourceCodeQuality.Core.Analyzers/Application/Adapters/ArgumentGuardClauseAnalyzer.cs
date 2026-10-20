using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 引数guard句のThrowIf利用を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ArgumentGuardClauseAnalyzer : DiagnosticAnalyzer
{
    private const string ArgumentExceptionTypeName = "System.ArgumentException";
    private const string ArgumentNullExceptionTypeName = "System.ArgumentNullException";
    private const string ArgumentOutOfRangeExceptionTypeName = "System.ArgumentOutOfRangeException";

    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.ArgumentNullThrowIfNullRule,
            DiagnosticCatalog.ArgumentExceptionThrowIfStringRule,
            DiagnosticCatalog.ArgumentOutOfRangeThrowIfRule);

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
        context.RegisterCompilationStartAction(static compilationContext =>
        {
            ArgumentGuardApiAvailability apiAvailability = ArgumentGuardApiAvailability.Create(compilationContext.Compilation);

            if (!apiAvailability.SupportsAny)
            {
                return;
            }

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeThrowStatement(nodeContext, apiAvailability),
                SyntaxKind.ThrowStatement);
            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeThrowExpression(nodeContext, apiAvailability),
                SyntaxKind.ThrowExpression);
        });
    }

    /// <summary>
    /// throw文を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="apiAvailability">guard API利用可否です。</param>
    private static void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context, ArgumentGuardApiAvailability apiAvailability)
    {
        if (context.Node is not ThrowStatementSyntax { Expression: not null } throwStatement)
        {
            return;
        }

        AnalyzeThrownExpression(context, throwStatement.Expression, throwStatement, apiAvailability);
    }

    /// <summary>
    /// throw式を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="apiAvailability">guard API利用可否です。</param>
    private static void AnalyzeThrowExpression(SyntaxNodeAnalysisContext context, ArgumentGuardApiAvailability apiAvailability)
    {
        if (context.Node is not ThrowExpressionSyntax throwExpression)
        {
            return;
        }

        AnalyzeThrownExpression(context, throwExpression.Expression, throwExpression, apiAvailability);
    }

    /// <summary>
    /// throw対象式を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="expression">throw対象式です。</param>
    /// <param name="throwNode">throw構文です。</param>
    /// <param name="apiAvailability">guard API利用可否です。</param>
    private static void AnalyzeThrownExpression(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        SyntaxNode throwNode,
        ArgumentGuardApiAvailability apiAvailability)
    {
        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(expression.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (!TryGetExceptionCreation(context, expression, out INamedTypeSymbol? exceptionType, out Location reportLocation))
        {
            return;
        }

        if (apiAvailability.SupportsThrowIfNull
            && IsExactType(context.SemanticModel.Compilation, exceptionType, ArgumentNullExceptionTypeName))
        {
            context.ReportDiagnostic(Diagnostic.Create(DiagnosticCatalog.ArgumentNullThrowIfNullRule, reportLocation));
            return;
        }

        IfStatementSyntax? guardIfStatement = FindGuardIfStatement(throwNode);
        if (guardIfStatement is null)
        {
            return;
        }

        if (apiAvailability.SupportsStringGuards
            && IsExactType(context.SemanticModel.Compilation, exceptionType, ArgumentExceptionTypeName)
            && TryGetStringGuardMethod(context, guardIfStatement.Condition, apiAvailability, out string stringGuardMethod))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.ArgumentExceptionThrowIfStringRule,
                    reportLocation,
                    stringGuardMethod));
            return;
        }

        if (apiAvailability.SupportsRangeGuards
            && IsExactType(context.SemanticModel.Compilation, exceptionType, ArgumentOutOfRangeExceptionTypeName)
            && TryGetRangeGuardMethod(guardIfStatement.Condition, apiAvailability, out string rangeGuardMethod))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.ArgumentOutOfRangeThrowIfRule,
                    reportLocation,
                    rangeGuardMethod));
        }
    }

    /// <summary>
    /// 例外生成式を取得します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="expression">throw対象式です。</param>
    /// <param name="exceptionType">例外型です。</param>
    /// <param name="reportLocation">診断位置です。</param>
    /// <returns>例外生成式の場合は <see langword="true" /> を返します。</returns>
    private static bool TryGetExceptionCreation(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax expression,
        out INamedTypeSymbol? exceptionType,
        out Location reportLocation)
    {
        exceptionType = null;
        reportLocation = expression.GetLocation();
        ExpressionSyntax unwrappedExpression = UnwrapParentheses(expression);

        if (unwrappedExpression is ObjectCreationExpressionSyntax objectCreation)
        {
            exceptionType = context.SemanticModel.GetTypeInfo(objectCreation, context.CancellationToken).Type as INamedTypeSymbol;
            reportLocation = objectCreation.Type.GetLocation();
            return exceptionType is not null;
        }

        if (unwrappedExpression is ImplicitObjectCreationExpressionSyntax implicitCreation)
        {
            exceptionType = context.SemanticModel.GetTypeInfo(implicitCreation, context.CancellationToken).Type as INamedTypeSymbol;
            reportLocation = implicitCreation.NewKeyword.GetLocation();
            return exceptionType is not null;
        }

        return false;
    }

    /// <summary>
    /// 文字列guardに対応するThrowIfメソッド名を取得します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="condition">if条件です。</param>
    /// <param name="apiAvailability">guard API利用可否です。</param>
    /// <param name="methodName">推奨メソッド名です。</param>
    /// <returns>対応するメソッドがある場合は <see langword="true" /> を返します。</returns>
    private static bool TryGetStringGuardMethod(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax condition,
        ArgumentGuardApiAvailability apiAvailability,
        out string methodName)
    {
        methodName = string.Empty;
        foreach (InvocationExpressionSyntax invocation in condition.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
        {
            if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            if (methodSymbol.ContainingType.SpecialType != SpecialType.System_String)
            {
                continue;
            }

            if (methodSymbol.Name.Equals("IsNullOrEmpty", StringComparison.Ordinal)
                && apiAvailability.HasArgumentExceptionMethod("ThrowIfNullOrEmpty"))
            {
                methodName = "ThrowIfNullOrEmpty";
                return true;
            }

            if (methodSymbol.Name.Equals("IsNullOrWhiteSpace", StringComparison.Ordinal)
                && apiAvailability.HasArgumentExceptionMethod("ThrowIfNullOrWhiteSpace"))
            {
                methodName = "ThrowIfNullOrWhiteSpace";
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 範囲guardに対応するThrowIfメソッド名を取得します。
    /// </summary>
    /// <param name="condition">if条件です。</param>
    /// <param name="apiAvailability">guard API利用可否です。</param>
    /// <param name="methodName">推奨メソッド名です。</param>
    /// <returns>対応するメソッドがある場合は <see langword="true" /> を返します。</returns>
    private static bool TryGetRangeGuardMethod(
        ExpressionSyntax condition,
        ArgumentGuardApiAvailability apiAvailability,
        out string methodName)
    {
        methodName = string.Empty;
        foreach (string candidate in ArgumentOutOfRangeGuardPattern.GetMethodNames(condition))
        {
            if (!apiAvailability.HasOutOfRangeMethod(candidate))
            {
                continue;
            }

            methodName = candidate;
            return true;
        }

        return false;
    }

    /// <summary>
    /// throwを含むguard if文を取得します。
    /// </summary>
    /// <param name="throwNode">throw構文です。</param>
    /// <returns>guard if文です。</returns>
    private static IfStatementSyntax? FindGuardIfStatement(SyntaxNode throwNode)
    {
        foreach (SyntaxNode ancestor in throwNode.Ancestors())
        {
            if (ancestor is IfStatementSyntax ifStatement)
            {
                return ifStatement;
            }

            if (ancestor is MethodDeclarationSyntax
                || ancestor is LocalFunctionStatementSyntax
                || ancestor is AnonymousFunctionExpressionSyntax)
            {
                return null;
            }
        }

        return null;
    }

    /// <summary>
    /// 型が指定メタデータ名と完全一致するか判定します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <param name="typeSymbol">判定対象型です。</param>
    /// <param name="metadataName">メタデータ名です。</param>
    /// <returns>一致する場合は <see langword="true" /> を返します。</returns>
    private static bool IsExactType(Compilation compilation, ITypeSymbol? typeSymbol, string metadataName)
    {
        INamedTypeSymbol? expectedType = compilation.GetTypeByMetadataName(metadataName);
        return SymbolEqualityComparer.Default.Equals(typeSymbol, expectedType);
    }

    /// <summary>
    /// 括弧を除いた式を取得します。
    /// </summary>
    /// <param name="expression">式です。</param>
    /// <returns>括弧を除いた式です。</returns>
    private static ExpressionSyntax UnwrapParentheses(ExpressionSyntax expression)
    {
        ExpressionSyntax current = expression;
        while (current is ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            current = parenthesizedExpression.Expression;
        }

        return current;
    }
}
