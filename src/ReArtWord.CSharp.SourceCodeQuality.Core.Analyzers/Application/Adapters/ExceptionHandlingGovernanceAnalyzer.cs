using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 例外throw運用を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExceptionHandlingGovernanceAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.BareRethrowWarningRule,
            DiagnosticCatalog.GenericExceptionThrowRule,
            DiagnosticCatalog.CapturedExceptionThrowRule,
            DiagnosticCatalog.CatchBlockThrowRule);

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
        context.RegisterSyntaxNodeAction(AnalyzeThrowStatement, SyntaxKind.ThrowStatement);
        context.RegisterSyntaxNodeAction(AnalyzeThrowExpression, SyntaxKind.ThrowExpression);
    }

    /// <summary>
    /// throw文を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ThrowStatementSyntax throwStatement)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(throwStatement.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (throwStatement.Expression is null)
        {
            AnalyzeBareRethrow(context, throwStatement);
            return;
        }

        AnalyzeExpressionThrow(context, throwStatement.Expression, throwStatement.ThrowKeyword);
    }

    /// <summary>
    /// throw式を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeThrowExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ThrowExpressionSyntax throwExpression)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(throwExpression.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        AnalyzeExpressionThrow(context, throwExpression.Expression, throwExpression.ThrowKeyword);
    }

    /// <summary>
    /// bare throwによる再スローを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="throwStatement">throw文です。</param>
    private static void AnalyzeBareRethrow(SyntaxNodeAnalysisContext context, ThrowStatementSyntax throwStatement)
    {
        if (!IsDirectlyInsideCatch(throwStatement))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.BareRethrowWarningRule,
                throwStatement.ThrowKeyword.GetLocation()));
    }

    /// <summary>
    /// 式付きthrowを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="thrownExpression">throw対象式です。</param>
    /// <param name="throwKeyword">throwキーワードです。</param>
    private static void AnalyzeExpressionThrow(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax thrownExpression,
        SyntaxToken throwKeyword)
    {
        if (IsCapturedExceptionThrow(context, thrownExpression, out string variableName))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.CapturedExceptionThrowRule,
                    UnwrapParentheses(thrownExpression).GetLocation(),
                    variableName));
            return;
        }

        if (IsGenericExceptionCreation(context, thrownExpression, out Location genericExceptionLocation))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.GenericExceptionThrowRule,
                    genericExceptionLocation));
        }

        if (!IsDirectlyInsideCatch(thrownExpression))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.CatchBlockThrowRule,
                throwKeyword.GetLocation()));
    }

    /// <summary>
    /// catch変数をthrowしているか判定します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="thrownExpression">throw対象式です。</param>
    /// <param name="variableName">catch変数名です。</param>
    /// <returns>catch変数throwの場合は <see langword="true" /> を返します。</returns>
    private static bool IsCapturedExceptionThrow(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax thrownExpression,
        out string variableName)
    {
        variableName = string.Empty;
        ExpressionSyntax expression = UnwrapParentheses(thrownExpression);
        if (expression is not IdentifierNameSyntax identifierName)
        {
            return false;
        }

        CatchClauseSyntax? catchClause = FindNearestCatchClause(expression);
        CatchDeclarationSyntax? catchDeclaration = catchClause?.Declaration;
        if (catchDeclaration is null)
        {
            return false;
        }

        variableName = catchDeclaration.Identifier.ValueText;
        if (variableName.Length == 0
            || !identifierName.Identifier.ValueText.Equals(variableName, StringComparison.Ordinal))
        {
            return false;
        }

        return IsExceptionCatchDeclaration(context, catchDeclaration);
    }

    /// <summary>
    /// 汎用Exceptionを生成しているか判定します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="thrownExpression">throw対象式です。</param>
    /// <param name="diagnosticLocation">診断位置です。</param>
    /// <returns>汎用Exception生成の場合は <see langword="true" /> を返します。</returns>
    private static bool IsGenericExceptionCreation(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax thrownExpression,
        out Location diagnosticLocation)
    {
        diagnosticLocation = thrownExpression.GetLocation();
        ExpressionSyntax expression = UnwrapParentheses(thrownExpression);
        if (expression is not ObjectCreationExpressionSyntax objectCreation)
        {
            return false;
        }

        ITypeSymbol? thrownType = context.SemanticModel.GetTypeInfo(objectCreation, context.CancellationToken).Type;
        if (!IsSystemExceptionType(context.SemanticModel.Compilation, thrownType))
        {
            return false;
        }

        diagnosticLocation = objectCreation.Type.GetLocation();
        return true;
    }

    /// <summary>
    /// catch宣言がException派生型か判定します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="catchDeclaration">catch宣言です。</param>
    /// <returns>Exception派生型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsExceptionCatchDeclaration(
        SyntaxNodeAnalysisContext context,
        CatchDeclarationSyntax catchDeclaration)
    {
        ITypeSymbol? catchType = context.SemanticModel.GetTypeInfo(catchDeclaration.Type, context.CancellationToken).Type;
        INamedTypeSymbol? exceptionType = context.SemanticModel.Compilation.GetTypeByMetadataName("System.Exception");
        return IsSameOrDerivedFrom(catchType, exceptionType);
    }

    /// <summary>
    /// 型がSystem.Exceptionそのものか判定します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <param name="typeSymbol">判定対象型です。</param>
    /// <returns>System.Exceptionの場合は <see langword="true" /> を返します。</returns>
    private static bool IsSystemExceptionType(Compilation compilation, ITypeSymbol? typeSymbol)
    {
        INamedTypeSymbol? exceptionType = compilation.GetTypeByMetadataName("System.Exception");
        return SymbolEqualityComparer.Default.Equals(typeSymbol, exceptionType);
    }

    /// <summary>
    /// 型が指定基底型と同一または派生型か判定します。
    /// </summary>
    /// <param name="typeSymbol">判定対象型です。</param>
    /// <param name="baseTypeSymbol">基底型です。</param>
    /// <returns>同一または派生型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsSameOrDerivedFrom(ITypeSymbol? typeSymbol, INamedTypeSymbol? baseTypeSymbol)
    {
        ITypeSymbol? current = typeSymbol;
        while (current is not null)
        {
            if (SymbolEqualityComparer.Default.Equals(current, baseTypeSymbol))
            {
                return true;
            }

            current = current.BaseType;
        }

        return false;
    }

    /// <summary>
    /// throwがcatchブロック直下の実行文脈にあるか判定します。
    /// </summary>
    /// <param name="node">throw構文ノードです。</param>
    /// <returns>catchブロック直下の場合は <see langword="true" /> を返します。</returns>
    private static bool IsDirectlyInsideCatch(SyntaxNode node)
    {
        foreach (SyntaxNode ancestor in node.Ancestors())
        {
            if (ancestor is CatchClauseSyntax)
            {
                return true;
            }

            if (IsNestedExecutableBoundary(ancestor))
            {
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// 直近のcatch句を取得します。
    /// </summary>
    /// <param name="node">起点ノードです。</param>
    /// <returns>直近のcatch句です。</returns>
    private static CatchClauseSyntax? FindNearestCatchClause(SyntaxNode node)
    {
        return node.Ancestors().OfType<CatchClauseSyntax>().FirstOrDefault();
    }

    /// <summary>
    /// catchブロック直下とは別の実行境界か判定します。
    /// </summary>
    /// <param name="node">判定対象ノードです。</param>
    /// <returns>別の実行境界の場合は <see langword="true" /> を返します。</returns>
    private static bool IsNestedExecutableBoundary(SyntaxNode node)
    {
        return node is LocalFunctionStatementSyntax
            || node is AnonymousFunctionExpressionSyntax;
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
