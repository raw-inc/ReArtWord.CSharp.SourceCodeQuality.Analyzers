using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Configuration;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// クラス行数上限を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ClassLengthAnalyzer : DiagnosticAnalyzer
{
    private const int DefaultMaxClassLineCount = 350;

    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.ClassLengthRule);

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
            int maxClassLineCount = AnalyzerConfigurationReader.GetClassLengthLimit(
                compilationContext.Options,
                DefaultMaxClassLineCount,
                compilationContext.CancellationToken);

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeClassDeclaration(nodeContext, maxClassLineCount),
                SyntaxKind.ClassDeclaration);
        });
    }

    /// <summary>
    /// クラス宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="maxClassLineCount">許容するクラス行数上限です。</param>
    private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context, int maxClassLineCount)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclaration)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(classDeclaration.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        FileLinePositionSpan lineSpan = classDeclaration.GetLocation().GetLineSpan();
        int lineCount = lineSpan.EndLinePosition.Line - lineSpan.StartLinePosition.Line + 1;
        if (lineCount <= maxClassLineCount)
        {
            return;
        }

        Diagnostic diagnostic = Diagnostic.Create(
            DiagnosticCatalog.ClassLengthRule,
            classDeclaration.Identifier.GetLocation(),
            classDeclaration.Identifier.Text,
            lineCount,
            maxClassLineCount);

        context.ReportDiagnostic(diagnostic);
    }
}
