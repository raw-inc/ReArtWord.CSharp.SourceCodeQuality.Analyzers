using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// MVVM Toolkit利用規約を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MvvmToolkitUsageAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        MvvmDiagnosticCatalog.CreateSupportedDiagnostics();

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
        context.RegisterCompilationAction(AnalyzeToolkitReference);
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeConditionalAccess, SyntaxKind.ConditionalAccessExpression);
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzeObjectCreation, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeImplicitObjectCreation, SyntaxKind.ImplicitObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeAttribute, SyntaxKind.Attribute);
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        context.RegisterSyntaxNodeAction(
            AnalyzeAssignment,
            SyntaxKind.AddAssignmentExpression,
            SyntaxKind.SubtractAssignmentExpression);
        context.RegisterSyntaxNodeAction(AnalyzeTypeName, SyntaxKind.IdentifierName, SyntaxKind.GenericName);
        context.RegisterSyntaxNodeAction(AnalyzeMemberAccess, SyntaxKind.SimpleMemberAccessExpression);
    }

    /// <summary>
    /// CommunityToolkit.Mvvm参照状態を検証します。
    /// </summary>
    /// <param name="context">コンパイル解析コンテキストです。</param>
    private static void AnalyzeToolkitReference(CompilationAnalysisContext context)
    {
        if (!MvvmAnalysisFilter.IsCompilationTarget(context.Compilation))
        {
            return;
        }

        if (MvvmToolkitReferenceDetector.HasToolkitReference(context.Compilation, context.Options, context.CancellationToken))
        {
            return;
        }

        Location location = MvvmToolkitReferenceDetector.GetProjectStartLocation(context.Options, context.CancellationToken);
        context.ReportDiagnostic(Diagnostic.Create(MvvmDiagnosticCatalog.ToolkitPackageRequiredRule, location));
    }

    /// <summary>
    /// クラス宣言を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ClassDeclarationSyntax classDeclaration)
        {
            MvvmClassRuleAnalyzer.AnalyzeClassDeclaration(context, classDeclaration);
        }
    }

    /// <summary>
    /// 呼び出し式を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is InvocationExpressionSyntax invocation)
        {
            MvvmNotificationRuleAnalyzer.AnalyzeInvocation(context, invocation);
        }
    }

    /// <summary>
    /// 条件付きアクセス式を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeConditionalAccess(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is ConditionalAccessExpressionSyntax conditionalAccess)
        {
            MvvmNotificationRuleAnalyzer.AnalyzeConditionalAccess(context, conditionalAccess);
        }
    }

    /// <summary>
    /// field宣言を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not FieldDeclarationSyntax fieldDeclaration)
        {
            return;
        }

        MvvmCommandManualRuleAnalyzer.AnalyzeFieldDeclaration(context, fieldDeclaration);
        MvvmPropertyRuleAnalyzer.AnalyzeFieldDeclaration(context, fieldDeclaration);
    }

    /// <summary>
    /// property宣言を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
        {
            return;
        }

        MvvmCommandManualRuleAnalyzer.AnalyzePropertyDeclaration(context, propertyDeclaration);
        MvvmPropertyRuleAnalyzer.AnalyzePropertyDeclaration(context, propertyDeclaration);
    }

    /// <summary>
    /// ObjectCreation式を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeObjectCreation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ObjectCreationExpressionSyntax objectCreation)
        {
            return;
        }

        MvvmCommandManualRuleAnalyzer.AnalyzeObjectCreation(context, objectCreation);
        MvvmServiceCreationRuleAnalyzer.AnalyzeObjectCreation(context, objectCreation);
    }

    /// <summary>
    /// target-typed ObjectCreation式を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeImplicitObjectCreation(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ImplicitObjectCreationExpressionSyntax objectCreation)
        {
            return;
        }

        MvvmCommandManualRuleAnalyzer.AnalyzeImplicitObjectCreation(context, objectCreation);
        MvvmServiceCreationRuleAnalyzer.AnalyzeImplicitObjectCreation(context, objectCreation);
    }

    /// <summary>
    /// 属性を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeAttribute(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AttributeSyntax attribute
            || !MvvmSyntaxHelper.IsSyntaxTarget(context, attribute))
        {
            return;
        }

        if (MvvmSymbolHelper.IsRelayCommandAttribute(attribute, context.SemanticModel, context.CancellationToken))
        {
            ReportStringLiteralArguments(context, attribute, MvvmDiagnosticCatalog.RelayCommandNameofRequiredRule);
            return;
        }

        if (MvvmSymbolHelper.IsNotifyPropertyChangedForAttribute(attribute, context.SemanticModel, context.CancellationToken))
        {
            ReportStringLiteralArguments(context, attribute, MvvmDiagnosticCatalog.NotifyPropertyChangedForNameofRequiredRule);
            return;
        }

        if (MvvmSymbolHelper.IsObservablePropertyAttribute(attribute, context.SemanticModel, context.CancellationToken)
            && attribute.Parent is AttributeListSyntax { Parent: FieldDeclarationSyntax })
        {
            context.ReportDiagnostic(Diagnostic.Create(
                MvvmDiagnosticCatalog.ObservablePropertyPartialPropertyRule,
                attribute.Name.GetLocation()));
        }
    }

    /// <summary>
    /// メソッド宣言を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration)
        {
            return;
        }

        MvvmRelayCommandRuleAnalyzer.AnalyzeMethodDeclaration(context, methodDeclaration);
        MvvmChangedHookRuleAnalyzer.AnalyzeMethodDeclaration(context, methodDeclaration);
    }

    /// <summary>
    /// 代入式を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeAssignment(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is AssignmentExpressionSyntax assignment)
        {
            MvvmNotificationRuleAnalyzer.AnalyzeAssignment(context, assignment);
        }
    }

    /// <summary>
    /// 型への参照を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeTypeName(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is SimpleNameSyntax typeName)
        {
            MvvmViewReferenceRuleAnalyzer.AnalyzeTypeName(context, typeName);
        }
    }

    /// <summary>
    /// メンバーアクセス式を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeMemberAccess(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is MemberAccessExpressionSyntax memberAccess)
        {
            MvvmViewReferenceRuleAnalyzer.AnalyzeMemberAccess(context, memberAccess);
        }
    }

    /// <summary>
    /// 属性引数内のstringリテラルを診断します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="attribute">属性構文です。</param>
    /// <param name="descriptor">診断定義です。</param>
    private static void ReportStringLiteralArguments(
        SyntaxNodeAnalysisContext context,
        AttributeSyntax attribute,
        DiagnosticDescriptor descriptor)
    {
        if (attribute.ArgumentList is null)
        {
            return;
        }

        foreach (AttributeArgumentSyntax argument in attribute.ArgumentList.Arguments)
        {
            IEnumerable<LiteralExpressionSyntax> stringLiterals = argument.Expression
                .DescendantNodesAndSelf()
                .OfType<LiteralExpressionSyntax>()
                .Where(static literal => literal.IsKind(SyntaxKind.StringLiteralExpression));

            foreach (LiteralExpressionSyntax literal in stringLiterals)
            {
                context.ReportDiagnostic(Diagnostic.Create(descriptor, literal.GetLocation()));
            }
        }
    }
}
