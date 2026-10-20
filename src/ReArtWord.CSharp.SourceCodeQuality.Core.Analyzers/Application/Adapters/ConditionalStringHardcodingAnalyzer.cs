using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 条件部におけるstringハードコーディングを検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ConditionalStringHardcodingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.ConditionalStringHardcodingRule);

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
        context.RegisterSyntaxNodeAction(
            AnalyzeStringExpression,
            SyntaxKind.StringLiteralExpression,
            SyntaxKind.InterpolatedStringExpression);
    }

    /// <summary>
    /// stringリテラル式を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeStringExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ExpressionSyntax expression)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(expression.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (!IsInConditionPart(expression))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.ConditionalStringHardcodingRule,
                expression.GetLocation()));
    }

    /// <summary>
    /// 対象式が条件部に含まれるかどうか判定します。
    /// </summary>
    /// <param name="expression">対象式です。</param>
    /// <returns>条件部に含まれる場合は <see langword="true" /> を返します。</returns>
    private static bool IsInConditionPart(ExpressionSyntax expression)
    {
        foreach (SyntaxNode ancestor in expression.Ancestors())
        {
            switch (ancestor)
            {
                case IfStatementSyntax ifStatement when ContainsNode(ifStatement.Condition, expression):
                case WhileStatementSyntax whileStatement when ContainsNode(whileStatement.Condition, expression):
                case DoStatementSyntax doStatement when ContainsNode(doStatement.Condition, expression):
                case ForStatementSyntax { Condition: not null } forStatement when ContainsNode(forStatement.Condition, expression):
                case ConditionalExpressionSyntax conditionalExpression
                    when ContainsNode(conditionalExpression.Condition, expression):
                    return true;
            }
        }

        return false;
    }

    /// <summary>
    /// コンテナー式が対象ノードを範囲内に含むか判定します。
    /// </summary>
    /// <param name="container">コンテナー式です。</param>
    /// <param name="target">対象ノードです。</param>
    /// <returns>対象ノードを含む場合は <see langword="true" /> を返します。</returns>
    private static bool ContainsNode(ExpressionSyntax container, SyntaxNode target)
    {
        return container.SpanStart <= target.SpanStart && target.Span.End <= container.Span.End;
    }
}
