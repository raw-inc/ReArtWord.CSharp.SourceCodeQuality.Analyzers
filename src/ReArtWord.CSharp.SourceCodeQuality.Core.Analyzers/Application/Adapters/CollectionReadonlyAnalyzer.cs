using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// コレクション再代入抑制を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CollectionReadonlyAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.CollectionReadonlyRecommendationRule);

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
        context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
        context.RegisterSyntaxNodeAction(AnalyzePropertyDeclaration, SyntaxKind.PropertyDeclaration);
    }

    /// <summary>
    /// フィールド宣言を解析します。
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

        if (fieldDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.ReadOnlyKeyword))
            || fieldDeclaration.Modifiers.Any(static modifier => modifier.IsKind(SyntaxKind.ConstKeyword)))
        {
            return;
        }

        ITypeSymbol? fieldType = context.SemanticModel.GetTypeInfo(fieldDeclaration.Declaration.Type, context.CancellationToken).Type;
        if (!IsCollectionType(context.SemanticModel.Compilation, fieldType))
        {
            return;
        }

        foreach (VariableDeclaratorSyntax variable in fieldDeclaration.Declaration.Variables)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.CollectionReadonlyRecommendationRule,
                    variable.Identifier.GetLocation(),
                    variable.Identifier.ValueText));
        }
    }

    /// <summary>
    /// プロパティ宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzePropertyDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not PropertyDeclarationSyntax propertyDeclaration)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(propertyDeclaration.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        ITypeSymbol? propertyType = context.SemanticModel.GetTypeInfo(propertyDeclaration.Type, context.CancellationToken).Type;
        if (!IsCollectionType(context.SemanticModel.Compilation, propertyType)
            || !HasOrdinarySetAccessor(propertyDeclaration))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.CollectionReadonlyRecommendationRule,
                propertyDeclaration.Identifier.GetLocation(),
                propertyDeclaration.Identifier.ValueText));
    }

    /// <summary>
    /// 通常setアクセサを持つか判定します。
    /// </summary>
    /// <param name="propertyDeclaration">プロパティ宣言です。</param>
    /// <returns>通常setアクセサを持つ場合は <see langword="true" /> を返します。</returns>
    private static bool HasOrdinarySetAccessor(PropertyDeclarationSyntax propertyDeclaration)
    {
        return propertyDeclaration.AccessorList?.Accessors.Any(static accessor =>
            accessor.IsKind(SyntaxKind.SetAccessorDeclaration)) == true;
    }

    /// <summary>
    /// コレクション型か判定します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>コレクション型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsCollectionType(Compilation compilation, ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null || typeSymbol.SpecialType == SpecialType.System_String)
        {
            return false;
        }

        if (typeSymbol is IArrayTypeSymbol)
        {
            return true;
        }

        INamedTypeSymbol? enumerableType = compilation.GetTypeByMetadataName("System.Collections.IEnumerable");
        INamedTypeSymbol? genericEnumerableType = compilation.GetTypeByMetadataName("System.Collections.Generic.IEnumerable`1");

        return IsSameOrImplements(typeSymbol, enumerableType)
            || IsSameOrImplements(typeSymbol, genericEnumerableType);
    }

    /// <summary>
    /// 型が指定インターフェイスと同一または実装型か判定します。
    /// </summary>
    /// <param name="typeSymbol">判定対象型です。</param>
    /// <param name="interfaceSymbol">インターフェイス型です。</param>
    /// <returns>同一または実装型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsSameOrImplements(ITypeSymbol typeSymbol, INamedTypeSymbol? interfaceSymbol)
    {
        if (interfaceSymbol is null)
        {
            return false;
        }

        INamedTypeSymbol? namedType = typeSymbol as INamedTypeSymbol;
        if (namedType?.OriginalDefinition is not null
            && SymbolEqualityComparer.Default.Equals(namedType.OriginalDefinition, interfaceSymbol))
        {
            return true;
        }

        return typeSymbol.AllInterfaces.Any(implementedInterface =>
            SymbolEqualityComparer.Default.Equals(implementedInterface.OriginalDefinition, interfaceSymbol));
    }
}
