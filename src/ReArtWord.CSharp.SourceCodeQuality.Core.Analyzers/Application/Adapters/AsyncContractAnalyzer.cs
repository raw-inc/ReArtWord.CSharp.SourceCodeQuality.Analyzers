using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Configuration;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 非同期メソッド契約（CancellationToken、fire-and-forget、IAsyncEnumerable推奨）を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AsyncContractAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.AsyncCancellationTokenRequiredRule,
            DiagnosticCatalog.FireAndForgetProhibitedRule,
            DiagnosticCatalog.AsyncEnumerableRecommendationRule);

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
        context.RegisterCompilationStartAction(compilationContext =>
        {
            bool isNet6OrGreater = TargetFrameworkVersionDetector.IsNet6OrGreater(compilationContext.Compilation);

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeMethodDeclaration(nodeContext, isNet6OrGreater),
                SyntaxKind.MethodDeclaration);
            compilationContext.RegisterSyntaxNodeAction(
                AnalyzeExpressionStatement,
                SyntaxKind.ExpressionStatement);
        });
    }

    /// <summary>
    /// メソッド宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="isNet6OrGreater">.NET6以上かどうかです。</param>
    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context, bool isNet6OrGreater)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(methodDeclaration.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken) is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (!IsTaskLikeType(methodSymbol.ReturnType))
        {
            return;
        }

        AnalyzeCancellationTokenContract(context, methodDeclaration);
        AnalyzeAsyncEnumerableRecommendation(context, methodDeclaration, methodSymbol, isNet6OrGreater);
    }

    /// <summary>
    /// 式文を解析してfire-and-forgetを検出します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeExpressionStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ExpressionStatementSyntax expressionStatement)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(expressionStatement.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (expressionStatement.Expression is AssignmentExpressionSyntax assignmentExpression
            && assignmentExpression.IsKind(SyntaxKind.SimpleAssignmentExpression)
            && IsDiscardIdentifier(assignmentExpression.Left)
            && IsTaskLikeType(context.SemanticModel.GetTypeInfo(assignmentExpression.Right, context.CancellationToken).Type))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.FireAndForgetProhibitedRule,
                    AsyncContractDiagnosticLocation.GetDiscardAssignmentLocation(assignmentExpression)));
            return;
        }

        if (expressionStatement.Expression is AwaitExpressionSyntax)
        {
            return;
        }

        ITypeSymbol? expressionType = context.SemanticModel.GetTypeInfo(expressionStatement.Expression, context.CancellationToken).Type;
        if (!IsTaskLikeType(expressionType))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.FireAndForgetProhibitedRule,
                expressionStatement.Expression.GetLocation()));
    }

    /// <summary>
    /// CancellationToken必須契約を検証します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    private static void AnalyzeCancellationTokenContract(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration)
    {
        if (!HasMethodImplementation(methodDeclaration))
        {
            return;
        }

        if (methodDeclaration.Modifiers.Any(static modifier =>
                modifier.IsKind(SyntaxKind.OverrideKeyword) || modifier.IsKind(SyntaxKind.ExternKeyword))
            || methodDeclaration.ExplicitInterfaceSpecifier is not null)
        {
            return;
        }

        if (!AsyncCancellationTokenContract.TryGetDiagnosticLocation(context, methodDeclaration, out Location? location))
        {
            return;
        }

        ReportAsyncCancellationTokenDiagnostic(context, methodDeclaration, location!);
    }

    /// <summary>
    /// IAsyncEnumerable推奨ルールを検証します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    /// <param name="isNet6OrGreater">.NET6以上かどうかです。</param>
    private static void AnalyzeAsyncEnumerableRecommendation(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration,
        IMethodSymbol methodSymbol,
        bool isNet6OrGreater)
    {
        if (!isNet6OrGreater)
        {
            return;
        }

        if (methodSymbol.DeclaredAccessibility != Accessibility.Public)
        {
            return;
        }

        if (!methodDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.AsyncKeyword)))
        {
            return;
        }

        if (!TryGetTaskLikeResultType(methodSymbol.ReturnType, out ITypeSymbol? resultType)
            || resultType is null)
        {
            return;
        }

        if (resultType.SpecialType == SpecialType.System_String)
        {
            return;
        }

        if (!ImplementsGenericIEnumerable(resultType))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.AsyncEnumerableRecommendationRule,
                AsyncContractDiagnosticLocation.GetAsyncEnumerableRecommendationLocation(methodDeclaration),
                methodSymbol.Name));
    }

    /// <summary>
    /// メソッドが実装本体を持つか判定します。
    /// </summary>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <returns>実装本体を持つ場合は <see langword="true" /> を返します。</returns>
    private static bool HasMethodImplementation(MethodDeclarationSyntax methodDeclaration)
    {
        return methodDeclaration.Body is not null || methodDeclaration.ExpressionBody is not null;
    }

    /// <summary>
    /// Task/ValueTask戻り値から実戻り値型を取得します。
    /// </summary>
    /// <param name="returnType">戻り値型です。</param>
    /// <param name="resultType">実戻り値型です。</param>
    /// <returns>取得成功時は <see langword="true" /> を返します。</returns>
    private static bool TryGetTaskLikeResultType(ITypeSymbol returnType, out ITypeSymbol? resultType)
    {
        resultType = null;
        if (returnType is not INamedTypeSymbol namedReturnType)
        {
            return false;
        }

        if (!IsTaskLikeType(namedReturnType) || namedReturnType.TypeArguments.Length != 1)
        {
            return false;
        }

        resultType = namedReturnType.TypeArguments[0];
        return true;
    }

    /// <summary>
    /// 破棄代入の左辺がdiscardか判定します。
    /// </summary>
    /// <param name="leftExpression">左辺式です。</param>
    /// <returns>discardの場合は <see langword="true" /> を返します。</returns>
    private static bool IsDiscardIdentifier(ExpressionSyntax leftExpression)
    {
        return leftExpression is IdentifierNameSyntax identifier
            && identifier.Identifier.ValueText.Equals("_", StringComparison.Ordinal);
    }

    /// <summary>
    /// 型がTask/ValueTask系か判定します。
    /// </summary>
    /// <param name="typeSymbol">判定対象型です。</param>
    /// <returns>Task/ValueTask系の場合は <see langword="true" /> を返します。</returns>
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
    /// 型がジェネリック IEnumerable 実装か判定します。
    /// </summary>
    /// <param name="typeSymbol">判定対象型です。</param>
    /// <returns>実装型の場合は <see langword="true" /> を返します。</returns>
    private static bool ImplementsGenericIEnumerable(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedType && IsGenericIEnumerable(namedType))
        {
            return true;
        }

        return typeSymbol.AllInterfaces.Any(IsGenericIEnumerable);
    }

    /// <summary>
    /// 型がジェネリック IEnumerable か判定します。
    /// </summary>
    /// <param name="namedType">判定対象型です。</param>
    /// <returns>ジェネリック IEnumerable の場合は <see langword="true" /> を返します。</returns>
    private static bool IsGenericIEnumerable(INamedTypeSymbol namedType)
    {
        INamedTypeSymbol constructed = namedType.ConstructedFrom;
        return constructed.Name.Equals("IEnumerable", StringComparison.Ordinal)
            && constructed.Arity == 1
            && constructed.ContainingNamespace.ToDisplayString().Equals("System.Collections.Generic", StringComparison.Ordinal);
    }

    /// <summary>
    /// CancellationToken必須違反診断を報告します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <param name="location">診断位置です。</param>
    private static void ReportAsyncCancellationTokenDiagnostic(
        SyntaxNodeAnalysisContext context,
        MethodDeclarationSyntax methodDeclaration,
        Location location)
    {
        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.AsyncCancellationTokenRequiredRule,
                location,
                methodDeclaration.Identifier.ValueText));
    }
}
