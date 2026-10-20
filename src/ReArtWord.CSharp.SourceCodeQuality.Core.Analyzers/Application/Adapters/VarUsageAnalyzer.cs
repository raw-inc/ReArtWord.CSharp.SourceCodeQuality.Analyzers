using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// var使用条件とプリミティブ型制限を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed partial class VarUsageAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.VarUsageEligibilityRule,
            DiagnosticCatalog.VarPrimitiveRestrictionRule);

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
        context.RegisterSyntaxNodeAction(AnalyzeLocalDeclaration, SyntaxKind.LocalDeclarationStatement);
        context.RegisterSyntaxNodeAction(AnalyzeUsingStatement, SyntaxKind.UsingStatement);
        context.RegisterSyntaxNodeAction(AnalyzeForStatement, SyntaxKind.ForStatement);
        context.RegisterSyntaxNodeAction(AnalyzeForEachStatement, SyntaxKind.ForEachStatement);
        context.RegisterSyntaxNodeAction(AnalyzeDeclarationExpression, SyntaxKind.DeclarationExpression);
    }

    /// <summary>
    /// ローカル宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeLocalDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LocalDeclarationStatementSyntax localDeclaration
            || localDeclaration.Declaration is not VariableDeclarationSyntax declaration
            || !IsVarType(declaration.Type))
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(localDeclaration.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        foreach (VariableDeclaratorSyntax variable in declaration.Variables)
        {
            if (context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) is not ILocalSymbol symbol)
            {
                continue;
            }

            EvaluateVarUsage(
                context,
                declaration.Type,
                symbol.Name,
                symbol.Type,
                variable.Initializer?.Value);
        }
    }

    /// <summary>
    /// using文宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeUsingStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not UsingStatementSyntax usingStatement
            || usingStatement.Declaration is not VariableDeclarationSyntax declaration
            || !IsVarType(declaration.Type))
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(usingStatement.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        foreach (VariableDeclaratorSyntax variable in declaration.Variables)
        {
            if (context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) is not ILocalSymbol symbol)
            {
                continue;
            }

            EvaluateVarUsage(
                context,
                declaration.Type,
                symbol.Name,
                symbol.Type,
                variable.Initializer?.Value);
        }
    }

    /// <summary>
    /// for文宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeForStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ForStatementSyntax forStatement
            || forStatement.Declaration is not VariableDeclarationSyntax declaration
            || !IsVarType(declaration.Type))
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(forStatement.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        foreach (VariableDeclaratorSyntax variable in declaration.Variables)
        {
            if (context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) is not ILocalSymbol symbol)
            {
                continue;
            }

            EvaluateVarUsage(
                context,
                declaration.Type,
                symbol.Name,
                symbol.Type,
                variable.Initializer?.Value);
        }
    }

    /// <summary>
    /// foreach文宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ForEachStatementSyntax forEachStatement
            || !IsVarType(forEachStatement.Type))
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(forEachStatement.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (context.SemanticModel.GetDeclaredSymbol(forEachStatement, context.CancellationToken) is not ILocalSymbol symbol)
        {
            return;
        }

        EvaluateVarUsage(
            context,
            forEachStatement.Type,
            symbol.Name,
            symbol.Type,
            valueExpression: null);
    }

    /// <summary>
    /// out var宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeDeclarationExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not DeclarationExpressionSyntax declarationExpression
            || !IsVarType(declarationExpression.Type)
            || declarationExpression.Designation is not SingleVariableDesignationSyntax designation)
        {
            return;
        }

        if (declarationExpression.Parent is not ArgumentSyntax argument
            || !argument.RefKindKeyword.IsKind(SyntaxKind.OutKeyword))
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(declarationExpression.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (context.SemanticModel.GetDeclaredSymbol(designation, context.CancellationToken) is not ILocalSymbol symbol)
        {
            return;
        }

        EvaluateVarUsage(
            context,
            declarationExpression.Type,
            symbol.Name,
            symbol.Type,
            valueExpression: null);
    }

    /// <summary>
    /// var利用可否ルールを評価します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="varTypeSyntax">var型構文です。</param>
    /// <param name="variableName">変数名です。</param>
    /// <param name="inferredType">推論型です。</param>
    /// <param name="valueExpression">値式です。</param>
    private static void EvaluateVarUsage(
        SyntaxNodeAnalysisContext context,
        TypeSyntax varTypeSyntax,
        string variableName,
        ITypeSymbol inferredType,
        ExpressionSyntax? valueExpression)
    {
        bool reportSpl012 = ShouldReportSpl012(context, inferredType, valueExpression);
        if (reportSpl012)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.VarPrimitiveRestrictionRule,
                    varTypeSyntax.GetLocation(),
                    variableName,
                    inferredType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat)));
        }

        if (reportSpl012 && IsObviousPrimitiveInitializer(context, inferredType, valueExpression))
        {
            return;
        }

        if (IsVarUsageAllowed(context, inferredType, valueExpression))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.VarUsageEligibilityRule,
                varTypeSyntax.GetLocation(),
                variableName));
    }
}
