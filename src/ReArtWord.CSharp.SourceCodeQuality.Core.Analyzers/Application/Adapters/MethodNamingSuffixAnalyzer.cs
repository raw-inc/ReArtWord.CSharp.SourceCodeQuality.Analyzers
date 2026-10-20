using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// メソッド名のAsync/Unsafe接尾辞規約を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MethodNamingSuffixAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.AsyncMethodSuffixRule,
            DiagnosticCatalog.UnsafeMethodSuffixRule,
            DiagnosticCatalog.UnsafeAsyncMethodSuffixRule);

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
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    /// <summary>
    /// メソッド宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(methodDeclaration.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        string methodName = methodDeclaration.Identifier.Text;
        bool isAsyncMethod = methodDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.AsyncKeyword));
        bool isUnsafeTarget = IsUnsafeTargetMethod(methodDeclaration);

        if (isAsyncMethod && isUnsafeTarget)
        {
            if (!methodName.EndsWith("UnsafeAsync", StringComparison.Ordinal))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        DiagnosticCatalog.UnsafeAsyncMethodSuffixRule,
                        methodDeclaration.Identifier.GetLocation(),
                        methodName));
            }

            return;
        }

        if (isAsyncMethod && !methodName.EndsWith("Async", StringComparison.Ordinal))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.AsyncMethodSuffixRule,
                    methodDeclaration.Identifier.GetLocation(),
                    methodName));
        }

        if (isUnsafeTarget && !methodName.EndsWith("Unsafe", StringComparison.Ordinal))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.UnsafeMethodSuffixRule,
                    methodDeclaration.Identifier.GetLocation(),
                    methodName));
        }
    }

    /// <summary>
    /// unsafe対象メソッドか判定します。
    /// </summary>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <returns>unsafe対象の場合は <see langword="true" /> を返します。</returns>
    private static bool IsUnsafeTargetMethod(MethodDeclarationSyntax methodDeclaration)
    {
        if (methodDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.UnsafeKeyword)))
        {
            return true;
        }

        if (IsDeclaredInUnsafeType(methodDeclaration))
        {
            return true;
        }

        return ContainsUnsafeStatement(methodDeclaration);
    }

    /// <summary>
    /// unsafe型内で宣言されているか判定します。
    /// </summary>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <returns>unsafe型内の場合は <see langword="true" /> を返します。</returns>
    private static bool IsDeclaredInUnsafeType(MethodDeclarationSyntax methodDeclaration)
    {
        foreach (TypeDeclarationSyntax typeDeclaration in methodDeclaration.Ancestors().OfType<TypeDeclarationSyntax>())
        {
            if (typeDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.UnsafeKeyword)))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// メソッド本体にunsafe文があるか判定します。
    /// </summary>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <returns>unsafe文が含まれる場合は <see langword="true" /> を返します。</returns>
    private static bool ContainsUnsafeStatement(MethodDeclarationSyntax methodDeclaration)
    {
        if (methodDeclaration.Body is null)
        {
            return false;
        }

        return methodDeclaration.Body.DescendantNodes(
            static node => node is not LocalFunctionStatementSyntax && node is not AnonymousFunctionExpressionSyntax)
            .OfType<UnsafeStatementSyntax>()
            .Any();
    }
}
