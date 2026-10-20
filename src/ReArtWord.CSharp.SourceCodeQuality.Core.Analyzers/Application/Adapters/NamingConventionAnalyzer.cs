using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 一般命名規約を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NamingConventionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.VariableUnderscoreNamingRule,
            DiagnosticCatalog.MethodUnderscoreNamingRule,
            DiagnosticCatalog.ParameterUnderscoreNamingRule,
            DiagnosticCatalog.EnumPrefixRecommendationRule,
            DiagnosticCatalog.InterfacePrefixRequiredRule,
            DiagnosticCatalog.TypePascalCaseRule);

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
        context.RegisterSyntaxNodeAction(AnalyzeVariableDeclarator, SyntaxKind.VariableDeclarator);
        context.RegisterSyntaxNodeAction(AnalyzeForEachStatement, SyntaxKind.ForEachStatement);
        context.RegisterSyntaxNodeAction(AnalyzeCatchDeclaration, SyntaxKind.CatchDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
        context.RegisterSyntaxNodeAction(AnalyzeTypeDeclaration, GetTypeDeclarationKinds());
    }

    /// <summary>
    /// 変数宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeVariableDeclarator(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not VariableDeclaratorSyntax variableDeclarator)
        {
            return;
        }

        if (!IsVariableTarget(variableDeclarator)
            || !IsProductionSyntax(context, variableDeclarator)
            || !ShouldReportUnderscoreName(variableDeclarator.Identifier.ValueText)
            || UsesPrivateFieldNamingRule(context, variableDeclarator))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.VariableUnderscoreNamingRule,
                variableDeclarator.Identifier.GetLocation(),
                variableDeclarator.Identifier.ValueText));
    }

    /// <summary>
    /// foreach文を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeForEachStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ForEachStatementSyntax forEachStatement
            || !IsProductionSyntax(context, forEachStatement)
            || !ShouldReportUnderscoreName(forEachStatement.Identifier.ValueText))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.VariableUnderscoreNamingRule,
                forEachStatement.Identifier.GetLocation(),
                forEachStatement.Identifier.ValueText));
    }

    /// <summary>
    /// catch宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeCatchDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not CatchDeclarationSyntax catchDeclaration
            || catchDeclaration.Identifier.IsKind(SyntaxKind.None)
            || !IsProductionSyntax(context, catchDeclaration)
            || !ShouldReportUnderscoreName(catchDeclaration.Identifier.ValueText))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.VariableUnderscoreNamingRule,
                catchDeclaration.Identifier.GetLocation(),
                catchDeclaration.Identifier.ValueText));
    }

    /// <summary>
    /// メソッド宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration
            || !IsProductionSyntax(context, methodDeclaration)
            || !ShouldReportUnderscoreName(methodDeclaration.Identifier.ValueText))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.MethodUnderscoreNamingRule,
                methodDeclaration.Identifier.GetLocation(),
                methodDeclaration.Identifier.ValueText));
    }

    /// <summary>
    /// 引数宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ParameterSyntax parameter
            || !IsProductionSyntax(context, parameter)
            || !ShouldReportUnderscoreName(parameter.Identifier.ValueText))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.ParameterUnderscoreNamingRule,
                parameter.Identifier.GetLocation(),
                parameter.Identifier.ValueText));
    }

    /// <summary>
    /// 型宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BaseTypeDeclarationSyntax typeDeclaration
            || !IsProductionSyntax(context, typeDeclaration))
        {
            return;
        }

        string typeName = typeDeclaration.Identifier.ValueText;
        if (!IsPascalCase(typeName))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.TypePascalCaseRule,
                    typeDeclaration.Identifier.GetLocation(),
                    typeName));
        }

        if (typeDeclaration is InterfaceDeclarationSyntax && !HasInterfacePrefix(typeName))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.InterfacePrefixRequiredRule,
                    typeDeclaration.Identifier.GetLocation(),
                    typeName));
        }

        if (typeDeclaration is EnumDeclarationSyntax && !typeName.StartsWith("E", StringComparison.Ordinal))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.EnumPrefixRecommendationRule,
                    typeDeclaration.Identifier.GetLocation(),
                    typeName));
        }
    }

    /// <summary>
    /// 型宣言のSyntaxKind一覧を取得します。
    /// </summary>
    /// <returns>型宣言のSyntaxKind一覧です。</returns>
    private static SyntaxKind[] GetTypeDeclarationKinds()
    {
        return
        [
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.EnumDeclaration,
        ];
    }

    /// <summary>
    /// 対象変数宣言か判定します。
    /// </summary>
    /// <param name="variableDeclarator">変数宣言です。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    private static bool IsVariableTarget(VariableDeclaratorSyntax variableDeclarator)
    {
        return variableDeclarator.Parent?.Parent is FieldDeclarationSyntax
            or LocalDeclarationStatementSyntax
            or ForStatementSyntax;
    }

    /// <summary>
    /// private フィールドの専用規則が選択されている場合は、そちらで名前全体を検査します。
    /// </summary>
    /// <param name="context">構文解析コンテキストです。</param>
    /// <param name="declaration">変数宣言です。</param>
    /// <returns>専用規則で扱う場合は true を返します。</returns>
    private static bool UsesPrivateFieldNamingRule(SyntaxNodeAnalysisContext context, VariableDeclaratorSyntax declaration)
    {
        if (declaration.Parent?.Parent is not FieldDeclarationSyntax
            || context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken) is not IFieldSymbol field
            || !PrivateFieldNamingPolicy.IsTarget(field))
        {
            return false;
        }

        return PrivateFieldNamingPolicy.IsEnabled(
                DiagnosticCatalog.PrivateFieldNamingWithUnderscoreRule, declaration.SyntaxTree, context.Compilation, context.CancellationToken)
            || PrivateFieldNamingPolicy.IsEnabled(
                DiagnosticCatalog.PrivateFieldNamingWithoutUnderscoreRule, declaration.SyntaxTree, context.Compilation, context.CancellationToken);
    }

    /// <summary>
    /// アンダースコア名として報告すべきか判定します。
    /// </summary>
    /// <param name="name">名前です。</param>
    /// <returns>報告対象の場合は <see langword="true" /> を返します。</returns>
    private static bool ShouldReportUnderscoreName(string name)
    {
        return name.Length > 1 && name.Contains("_", StringComparison.Ordinal);
    }

    /// <summary>
    /// interface名がIプレフィックス形式か判定します。
    /// </summary>
    /// <param name="name">型名です。</param>
    /// <returns>Iプレフィックス形式の場合は <see langword="true" /> を返します。</returns>
    private static bool HasInterfacePrefix(string name)
    {
        return name.Length >= 2 && name[0] == 'I' && char.IsUpper(name[1]);
    }

    /// <summary>
    /// PascalCase形式か判定します。
    /// </summary>
    /// <param name="name">型名です。</param>
    /// <returns>PascalCase形式の場合は <see langword="true" /> を返します。</returns>
    private static bool IsPascalCase(string name)
    {
        if (name.Length == 0 || !char.IsUpper(name[0]))
        {
            return false;
        }

        for (int index = 1; index < name.Length; index++)
        {
            if (!char.IsLetterOrDigit(name[index]))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 本番コード構文か判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="node">構文ノードです。</param>
    /// <returns>本番コードの場合は <see langword="true" /> を返します。</returns>
    private static bool IsProductionSyntax(SyntaxNodeAnalysisContext context, SyntaxNode node)
    {
        return ProductionCodeFilter.IsProductionCode(node.SyntaxTree, context.SemanticModel.Compilation);
    }
}
