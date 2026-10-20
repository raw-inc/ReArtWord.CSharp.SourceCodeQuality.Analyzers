using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Configuration;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// lock文禁止とフィールドprivate必須を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ModernSynchronizationAndFieldAnalyzer : DiagnosticAnalyzer
{
    private const string Net8LockGuidance = ".NET8ではSemaphoreSlimなどのモダンな同期機構を使用し、.NET9以上へ移行後はSystem.Threading.Lockを検討してください。";
    private const string Net9LockGuidance = ".NET9以上ではSystem.Threading.Lockの利用を優先し、非同期要件がある場合はSemaphoreSlimなど用途に合う同期機構を使用してください。";

    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.LockStatementRestrictionRule,
            DiagnosticCatalog.FieldPrivateRule);

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
            bool isNet8OrGreater = TargetFrameworkVersionDetector.IsNet8OrGreater(compilationContext.Compilation);
            bool isNet9OrGreater = TargetFrameworkVersionDetector.IsNet9OrGreater(compilationContext.Compilation);

            if (isNet8OrGreater)
            {
                compilationContext.RegisterSyntaxNodeAction(
                    nodeContext => AnalyzeLockStatement(nodeContext, isNet9OrGreater),
                    SyntaxKind.LockStatement);
            }

            if (isNet6OrGreater)
            {
                compilationContext.RegisterSyntaxNodeAction(
                    AnalyzeFieldDeclaration,
                    SyntaxKind.FieldDeclaration);
            }
        });
    }

    /// <summary>
    /// lock文利用を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="isNet9OrGreater">.NET9以上かどうかです。</param>
    private static void AnalyzeLockStatement(SyntaxNodeAnalysisContext context, bool isNet9OrGreater)
    {
        if (context.Node is not LockStatementSyntax lockStatement)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(lockStatement.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        string guidance = isNet9OrGreater
            ? Net9LockGuidance
            : Net8LockGuidance;

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.LockStatementRestrictionRule,
                lockStatement.LockKeyword.GetLocation(),
                guidance));
    }

    /// <summary>
    /// フィールド宣言のアクセシビリティを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not FieldDeclarationSyntax fieldDeclaration)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(fieldDeclaration.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (fieldDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.ConstKeyword)))
        {
            return;
        }

        if (IsStructLayoutCompatibilityField(context, fieldDeclaration))
        {
            return;
        }

        foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
        {
            if (context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) is not IFieldSymbol fieldSymbol)
            {
                continue;
            }

            if (fieldSymbol.DeclaredAccessibility == Accessibility.Private)
            {
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.FieldPrivateRule,
                    variable.Identifier.GetLocation(),
                    variable.Identifier.Text));
        }
    }

    /// <summary>
    /// StructLayout互換のため例外扱いすべきフィールドか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="fieldDeclaration">フィールド宣言です。</param>
    /// <returns>例外対象の場合は <see langword="true" /> を返します。</returns>
    private static bool IsStructLayoutCompatibilityField(
        SyntaxNodeAnalysisContext context,
        FieldDeclarationSyntax fieldDeclaration)
    {
        if (HasStructLayoutAttributeOnContainingType(context, fieldDeclaration))
        {
            return true;
        }

        return HasFieldOffsetAttribute(context, fieldDeclaration);
    }

    /// <summary>
    /// 包含型にStructLayout属性が付与されているか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="fieldDeclaration">フィールド宣言です。</param>
    /// <returns>StructLayout属性がある場合は <see langword="true" /> を返します。</returns>
    private static bool HasStructLayoutAttributeOnContainingType(
        SyntaxNodeAnalysisContext context,
        FieldDeclarationSyntax fieldDeclaration)
    {
        if (fieldDeclaration.Parent is not TypeDeclarationSyntax containingType)
        {
            return false;
        }

        if (context.SemanticModel.GetDeclaredSymbol(containingType, context.CancellationToken) is not INamedTypeSymbol typeSymbol)
        {
            return false;
        }

        return HasAttribute(
            typeSymbol.GetAttributes(),
            "System.Runtime.InteropServices.StructLayoutAttribute",
            "StructLayoutAttribute");
    }

    /// <summary>
    /// フィールドへFieldOffset属性が付与されているか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="fieldDeclaration">フィールド宣言です。</param>
    /// <returns>FieldOffset属性がある場合は <see langword="true" /> を返します。</returns>
    private static bool HasFieldOffsetAttribute(
        SyntaxNodeAnalysisContext context,
        FieldDeclarationSyntax fieldDeclaration)
    {
        foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
        {
            if (context.SemanticModel.GetDeclaredSymbol(variable, context.CancellationToken) is not IFieldSymbol fieldSymbol)
            {
                continue;
            }

            if (HasAttribute(
                fieldSymbol.GetAttributes(),
                "System.Runtime.InteropServices.FieldOffsetAttribute",
                "FieldOffsetAttribute"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 属性集合に特定属性が含まれるか判定します。
    /// </summary>
    /// <param name="attributes">属性集合です。</param>
    /// <param name="fullTypeName">属性の完全修飾名です。</param>
    /// <param name="shortTypeName">属性の短縮名です。</param>
    /// <returns>属性を含む場合は <see langword="true" /> を返します。</returns>
    private static bool HasAttribute(
        IEnumerable<AttributeData> attributes,
        string fullTypeName,
        string shortTypeName)
    {
        return attributes.Any(attribute =>
            attribute.AttributeClass?.ToDisplayString().Equals(fullTypeName, StringComparison.Ordinal) == true
            || attribute.AttributeClass?.Name.Equals(shortTypeName, StringComparison.Ordinal) == true);
    }
}
