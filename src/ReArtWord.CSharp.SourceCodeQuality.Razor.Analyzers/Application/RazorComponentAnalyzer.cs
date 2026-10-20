using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers.Application;

/// <summary>
/// Razor 内の処理配置とコンポーネント内の取消トークン伝搬を検証します。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class RazorComponentAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        RazorDiagnosticCatalog.CreateSupportedDiagnostics();

    /// <summary>
    /// 構文と意味情報に基づく解析を登録します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        // Razor 内の利用者コードは生成 C# から元の画面へ行マッピングされます。
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            RazorCodeBehindRuleAnalyzer.AnalyzeMember,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.PropertyDeclaration,
            SyntaxKind.FieldDeclaration,
            SyntaxKind.EventDeclaration,
            SyntaxKind.EventFieldDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.RecordDeclaration);
        context.RegisterCompilationStartAction(InitializeCompilation);
    }

    /// <summary>
    /// コンパイル内のコンポーネント型と取消トークン型を解決します。
    /// </summary>
    /// <param name="context">コンパイル開始時の解析コンテキストです。</param>
    private static void InitializeCompilation(CompilationStartAnalysisContext context)
    {
        INamedTypeSymbol? component = context.Compilation.GetTypeByMetadataName("Microsoft.AspNetCore.Components.ComponentBase");
        INamedTypeSymbol? cancellation = context.Compilation.GetTypeByMetadataName("System.Threading.CancellationToken");
        if (component is not null && cancellation is not null)
        {
            context.RegisterOperationAction(
                operation => RazorCancellationRuleAnalyzer.AnalyzeInvocation(operation, component, cancellation),
                OperationKind.Invocation);
        }
    }
}
