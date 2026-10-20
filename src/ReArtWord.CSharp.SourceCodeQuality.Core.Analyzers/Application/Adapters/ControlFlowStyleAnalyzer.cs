using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 制御構文の可読性ルールを検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ControlFlowStyleAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.WhileTrueProhibitedRule,
            DiagnosticCatalog.LoopBraceRequiredRule,
            DiagnosticCatalog.IfElseBraceRequiredRule,
            DiagnosticCatalog.ForInfiniteLoopProhibitedRule,
            DiagnosticCatalog.ConstantFalseConditionRule,
            DiagnosticCatalog.ElseIfChainSwitchRecommendationRule,
            DiagnosticCatalog.ComparisonOperatorDirectionRule);

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
        context.RegisterSyntaxNodeAction(AnalyzeWhileStatement, Microsoft.CodeAnalysis.CSharp.SyntaxKind.WhileStatement);
        context.RegisterSyntaxNodeAction(AnalyzeForStatement, Microsoft.CodeAnalysis.CSharp.SyntaxKind.ForStatement);
        context.RegisterSyntaxNodeAction(AnalyzeForEachStatement, Microsoft.CodeAnalysis.CSharp.SyntaxKind.ForEachStatement);
        context.RegisterSyntaxNodeAction(AnalyzeIfStatement, Microsoft.CodeAnalysis.CSharp.SyntaxKind.IfStatement);
        context.RegisterSyntaxNodeAction(
            AnalyzeComparisonOperatorDirection,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.GreaterThanExpression,
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.GreaterThanOrEqualExpression);
    }

    /// <summary>
    /// while文を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeWhileStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not WhileStatementSyntax whileStatement)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(whileStatement.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (IsConstantTrueCondition(context, whileStatement.Condition))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.WhileTrueProhibitedRule,
                    whileStatement.Condition.GetLocation()));
        }

        if (IsConstantFalseCondition(context, whileStatement.Condition))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.ConstantFalseConditionRule,
                    whileStatement.Condition.GetLocation()));
        }

        if (!IsBlockStatement(whileStatement.Statement))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.LoopBraceRequiredRule,
                    whileStatement.Statement.GetLocation()));
        }
    }

    /// <summary>
    /// for文を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ForStatementSyntax forStatement)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(forStatement.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (IsForInfiniteLoop(context, forStatement))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.ForInfiniteLoopProhibitedRule,
                    GetForInfiniteLoopLocation(forStatement)));
        }

        if (!IsBlockStatement(forStatement.Statement))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.LoopBraceRequiredRule,
                    forStatement.Statement.GetLocation()));
        }
    }

    /// <summary>
    /// foreach文を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ForEachStatementSyntax forEachStatement)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(forEachStatement.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (IsBlockStatement(forEachStatement.Statement))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.LoopBraceRequiredRule,
                forEachStatement.Statement.GetLocation()));
    }

    /// <summary>
    /// if/else文を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeIfStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not IfStatementSyntax ifStatement)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(ifStatement.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (IsConstantFalseCondition(context, ifStatement.Condition))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.ConstantFalseConditionRule,
                    ifStatement.Condition.GetLocation()));
        }

        if (!IsBlockStatement(ifStatement.Statement))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.IfElseBraceRequiredRule,
                    ifStatement.Statement.GetLocation()));
        }

        if (ifStatement.Else is null || IsBlockStatement(ifStatement.Else.Statement))
        {
            AnalyzeElseIfChainSwitchRequirement(context, ifStatement);
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.IfElseBraceRequiredRule,
                ifStatement.Else.Statement.GetLocation()));

        AnalyzeElseIfChainSwitchRequirement(context, ifStatement);
    }

    /// <summary>
    /// 比較演算子の方向を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeComparisonOperatorDirection(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BinaryExpressionSyntax binaryExpression)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(binaryExpression.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.ComparisonOperatorDirectionRule,
                binaryExpression.OperatorToken.GetLocation(),
                binaryExpression.OperatorToken.Text));
    }

    /// <summary>
    /// else-if連鎖のswitch化必須ルールを検証します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="ifStatement">if文です。</param>
    private static void AnalyzeElseIfChainSwitchRequirement(
        SyntaxNodeAnalysisContext context,
        IfStatementSyntax ifStatement)
    {
        if (ifStatement.Parent is ElseClauseSyntax)
        {
            return;
        }

        int elseIfChainCount = CountElseIfChain(ifStatement);
        if (elseIfChainCount < 3)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.ElseIfChainSwitchRecommendationRule,
                ifStatement.IfKeyword.GetLocation(),
                elseIfChainCount));
    }

    /// <summary>
    /// else-if連鎖数をカウントします。
    /// </summary>
    /// <param name="rootIfStatement">連鎖の起点if文です。</param>
    /// <returns>else-if連鎖数です。</returns>
    private static int CountElseIfChain(IfStatementSyntax rootIfStatement)
    {
        int chainCount = 0;
        IfStatementSyntax? current = rootIfStatement;
        while (current?.Else?.Statement is IfStatementSyntax nextIf)
        {
            chainCount++;
            current = nextIf;
        }

        return chainCount;
    }

    /// <summary>
    /// for文が無条件ループか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="forStatement">for文です。</param>
    /// <returns>条件省略または定数true条件の場合は <see langword="true" /> を返します。</returns>
    private static bool IsForInfiniteLoop(SyntaxNodeAnalysisContext context, ForStatementSyntax forStatement)
    {
        ExpressionSyntax? condition = forStatement.Condition;
        return condition is null
            || IsConstantTrueCondition(context, condition);
    }

    /// <summary>
    /// for無条件ループ違反の診断位置を取得します。
    /// </summary>
    /// <param name="forStatement">for文です。</param>
    /// <returns>診断位置です。</returns>
    private static Location GetForInfiniteLoopLocation(ForStatementSyntax forStatement)
    {
        if (forStatement.Condition is not null)
        {
            return forStatement.Condition.GetLocation();
        }

        return Location.Create(
            forStatement.SyntaxTree,
            new TextSpan(forStatement.FirstSemicolonToken.Span.End, 0));
    }

    /// <summary>
    /// 条件式が定数trueか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="condition">条件式です。</param>
    /// <returns>定数trueの場合は <see langword="true" /> を返します。</returns>
    private static bool IsConstantTrueCondition(SyntaxNodeAnalysisContext context, ExpressionSyntax condition)
    {
        Optional<object?> constantValue = context.SemanticModel.GetConstantValue(condition, context.CancellationToken);
        return constantValue is { HasValue: true, Value: bool booleanValue } && booleanValue;
    }

    /// <summary>
    /// 条件式が定数falseか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="condition">条件式です。</param>
    /// <returns>定数falseの場合は <see langword="true" /> を返します。</returns>
    private static bool IsConstantFalseCondition(SyntaxNodeAnalysisContext context, ExpressionSyntax condition)
    {
        Optional<object?> constantValue = context.SemanticModel.GetConstantValue(condition, context.CancellationToken);
        return constantValue is { HasValue: true, Value: bool booleanValue } && !booleanValue;
    }

    /// <summary>
    /// 波括弧ブロックか判定します。
    /// </summary>
    /// <param name="statement">文です。</param>
    /// <returns>波括弧ブロックの場合は <see langword="true" /> を返します。</returns>
    private static bool IsBlockStatement(StatementSyntax statement)
    {
        return statement is BlockSyntax;
    }
}
