using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Configuration;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// IDisposable/IAsyncDisposable利用時のusing強制ルールを検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DisposableUsageAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.ExplicitDisposeInvocationRule,
            DiagnosticCatalog.DisposableLocalUsingRequiredRule);

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
        context.RegisterCompilationStartAction(startContext =>
        {
            if (!TargetFrameworkVersionDetector.IsNet6OrGreater(startContext.Compilation))
            {
                return;
            }

            INamedTypeSymbol? disposableTypeSymbol =
                startContext.Compilation.GetTypeByMetadataName("System.IDisposable");
            INamedTypeSymbol? asyncDisposableTypeSymbol =
                startContext.Compilation.GetTypeByMetadataName("System.IAsyncDisposable");

            startContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeInvocation(nodeContext, disposableTypeSymbol, asyncDisposableTypeSymbol),
                SyntaxKind.InvocationExpression);
            startContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeLocalDeclaration(nodeContext, disposableTypeSymbol, asyncDisposableTypeSymbol),
                SyntaxKind.LocalDeclarationStatement);
        });
    }

    /// <summary>
    /// 明示Dispose呼び出しを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="disposableTypeSymbol">IDisposable型シンボルです。</param>
    /// <param name="asyncDisposableTypeSymbol">IAsyncDisposable型シンボルです。</param>
    private static void AnalyzeInvocation(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol? disposableTypeSymbol,
        INamedTypeSymbol? asyncDisposableTypeSymbol)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(invocation.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (!TryGetContainingMethodSymbol(context, invocation, out IMethodSymbol? containingMethod)
            || containingMethod is null)
        {
            return;
        }

        if (DisposableUsageAnalyzerHelper.IsDisposeMethodDeclaration(containingMethod))
        {
            return;
        }

        if (!DisposableUsageAnalyzerHelper.IsDisposableOwnerClass(
            containingMethod.ContainingType,
            disposableTypeSymbol,
            asyncDisposableTypeSymbol))
        {
            return;
        }

        if (!DisposableUsageAnalyzerHelper.TryGetInvokedMethodSymbol(context, invocation, out IMethodSymbol? invokedMethod)
            || invokedMethod is null)
        {
            return;
        }

        if (!DisposableUsageAnalyzerHelper.TryGetDisposeInvocationDisplayName(
            invokedMethod,
            disposableTypeSymbol,
            asyncDisposableTypeSymbol,
            out string invocationName))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.ExplicitDisposeInvocationRule,
                GetDisposeInvocationLocation(invocation),
                invocationName));
    }

    /// <summary>
    /// 明示Dispose呼び出しの診断位置を取得します。
    /// </summary>
    /// <param name="invocation">呼び出し式です。</param>
    /// <returns>診断位置です。</returns>
    private static Location GetDisposeInvocationLocation(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.GetLocation(),
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name.GetLocation(),
            IdentifierNameSyntax identifierName => identifierName.Identifier.GetLocation(),
            _ => invocation.GetLocation(),
        };
    }

    /// <summary>
    /// ローカル宣言におけるusing必須を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="disposableTypeSymbol">IDisposable型シンボルです。</param>
    /// <param name="asyncDisposableTypeSymbol">IAsyncDisposable型シンボルです。</param>
    private static void AnalyzeLocalDeclaration(
        SyntaxNodeAnalysisContext context,
        INamedTypeSymbol? disposableTypeSymbol,
        INamedTypeSymbol? asyncDisposableTypeSymbol)
    {
        if (context.Node is not LocalDeclarationStatementSyntax localDeclaration)
        {
            return;
        }

        if (!localDeclaration.UsingKeyword.IsKind(SyntaxKind.None))
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(localDeclaration.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (!TryGetContainingMethodSymbol(context, localDeclaration, out IMethodSymbol? containingMethod)
            || containingMethod is null)
        {
            return;
        }

        if (DisposableUsageAnalyzerHelper.IsDisposeMethodDeclaration(containingMethod))
        {
            return;
        }

        if (!DisposableUsageAnalyzerHelper.IsDisposableOwnerClass(
            containingMethod.ContainingType,
            disposableTypeSymbol,
            asyncDisposableTypeSymbol))
        {
            return;
        }

        foreach (VariableDeclaratorSyntax variable in localDeclaration.Declaration.Variables)
        {
            if (context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) is not ILocalSymbol localSymbol)
            {
                continue;
            }

            if (!DisposableUsageAnalyzerHelper.ImplementsDisposableContract(
                localSymbol.Type,
                disposableTypeSymbol,
                asyncDisposableTypeSymbol))
            {
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.DisposableLocalUsingRequiredRule,
                    variable.Identifier.GetLocation(),
                    localSymbol.Name));
        }
    }

    /// <summary>
    /// 対象ノードを包含するメソッドシンボルを取得します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="node">対象ノードです。</param>
    /// <param name="methodSymbol">取得したメソッドシンボルです。</param>
    /// <returns>取得成功時は <see langword="true" /> を返します。</returns>
    private static bool TryGetContainingMethodSymbol(
        SyntaxNodeAnalysisContext context,
        SyntaxNode node,
        out IMethodSymbol? methodSymbol)
    {
        return DisposableUsageAnalyzerHelper.TryGetContainingMethodSymbol(context, node, out methodSymbol);
    }
}
