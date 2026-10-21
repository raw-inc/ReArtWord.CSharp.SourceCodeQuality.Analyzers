using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 処理を持たないデータ型でのToString実装を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DataTypeToStringAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.DataTypeToStringRule);

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
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    /// <summary>
    /// 型シンボルを解析します。
    /// </summary>
    /// <param name="context">シンボル解析コンテキストです。</param>
    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        if (context.Symbol is not INamedTypeSymbol namedType)
        {
            return;
        }

        if (!IsSupportedType(namedType))
        {
            return;
        }

        if (!IsTargetProductionType(namedType, context.Compilation))
        {
            return;
        }

        if (!IsDataTypeLike(namedType))
        {
            return;
        }

        if (HasToStringOverride(namedType))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.DataTypeToStringRule,
                GetPrimaryLocation(namedType),
                namedType.Name));
    }

    /// <summary>
    /// 解析対象の型種別か判定します。
    /// </summary>
    /// <param name="namedType">型シンボルです。</param>
    /// <returns>対象型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsSupportedType(INamedTypeSymbol namedType)
    {
        if (namedType.IsImplicitlyDeclared)
        {
            return false;
        }

        if (namedType.TypeKind is not (TypeKind.Class or TypeKind.Struct))
        {
            return false;
        }

        return !namedType.IsAbstract && !namedType.IsStatic;
    }

    /// <summary>
    /// 解析対象が本番コード型か判定します。
    /// </summary>
    /// <param name="namedType">型シンボルです。</param>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <returns>本番コード型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsTargetProductionType(INamedTypeSymbol namedType, Compilation compilation)
    {
        foreach (Location location in namedType.Locations)
        {
            if (!location.IsInSource || location.SourceTree is null)
            {
                continue;
            }

            if (ProductionCodeFilter.IsProductionCode(location.SourceTree, compilation))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 診断報告用の主位置を取得します。
    /// </summary>
    /// <param name="namedType">型シンボルです。</param>
    /// <returns>主位置です。</returns>
    private static Location GetPrimaryLocation(INamedTypeSymbol namedType)
    {
        foreach (Location location in namedType.Locations)
        {
            if (location.IsInSource)
            {
                return location;
            }
        }

        return namedType.Locations.FirstOrDefault() ?? Location.None;
    }

    /// <summary>
    /// データ保持中心の型か判定します。
    /// </summary>
    /// <param name="namedType">型シンボルです。</param>
    /// <returns>データ型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsDataTypeLike(INamedTypeSymbol namedType)
    {
        bool hasFieldOrProperty = false;

        foreach (ISymbol member in namedType.GetMembers())
        {
            switch (member)
            {
                case IFieldSymbol fieldSymbol:
                    if (fieldSymbol.AssociatedSymbol is not null)
                    {
                        continue;
                    }

                    hasFieldOrProperty = true;
                    continue;

                case IPropertySymbol propertySymbol:
                    if (propertySymbol.IsIndexer)
                    {
                        return false;
                    }

                    hasFieldOrProperty = true;
                    if (!IsAutoProperty(propertySymbol))
                    {
                        return false;
                    }

                    continue;

                case IEventSymbol:
                    return false;

                case IMethodSymbol methodSymbol:
                    if (IsAllowedMethod(namedType, methodSymbol))
                    {
                        continue;
                    }

                    return false;

                default:
                    continue;
            }
        }

        return hasFieldOrProperty;
    }

    /// <summary>
    /// データ型として許可するメソッドか判定します。
    /// </summary>
    /// <param name="namedType">型シンボルです。</param>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    /// <returns>許可対象の場合は <see langword="true" /> を返します。</returns>
    private static bool IsAllowedMethod(INamedTypeSymbol namedType, IMethodSymbol methodSymbol)
    {
        if (methodSymbol.IsImplicitlyDeclared)
        {
            return true;
        }

        if (methodSymbol.MethodKind is MethodKind.Constructor
            or MethodKind.StaticConstructor
            or MethodKind.PropertyGet
            or MethodKind.PropertySet
            or MethodKind.EventAdd
            or MethodKind.EventRemove
            or MethodKind.UserDefinedOperator)
        {
            return true;
        }

        if (IsToStringOverride(methodSymbol)
            || IsEqualsObjectOverride(methodSymbol)
            || IsTypedEquals(namedType, methodSymbol)
            || IsGetHashCodeOverride(methodSymbol))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 自動実装プロパティか判定します。
    /// </summary>
    /// <param name="propertySymbol">プロパティシンボルです。</param>
    /// <returns>自動実装プロパティの場合は <see langword="true" /> を返します。</returns>
    private static bool IsAutoProperty(IPropertySymbol propertySymbol)
    {
        if (propertySymbol.IsImplicitlyDeclared)
        {
            return true;
        }

        if (propertySymbol.DeclaringSyntaxReferences.Length == 0)
        {
            return false;
        }

        foreach (SyntaxReference syntaxReference in propertySymbol.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax() is not PropertyDeclarationSyntax propertyDeclaration)
            {
                return false;
            }

            if (propertyDeclaration.ExpressionBody is not null || propertyDeclaration.AccessorList is null)
            {
                return false;
            }

            foreach (AccessorDeclarationSyntax accessor in propertyDeclaration.AccessorList.Accessors)
            {
                if (accessor.Body is not null || accessor.ExpressionBody is not null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    /// ToStringオーバーライド実装有無を判定します。
    /// </summary>
    /// <param name="namedType">型シンボルです。</param>
    /// <returns>実装済みの場合は <see langword="true" /> を返します。</returns>
    private static bool HasToStringOverride(INamedTypeSymbol namedType)
    {
        return namedType.GetMembers("ToString")
            .OfType<IMethodSymbol>()
            .Any(IsToStringOverride);
    }

    /// <summary>
    /// ToStringオーバーライドか判定します。
    /// </summary>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    /// <returns>ToStringオーバーライドの場合は <see langword="true" /> を返します。</returns>
    private static bool IsToStringOverride(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Name.Equals("ToString", StringComparison.Ordinal)
            && methodSymbol.Parameters.Length == 0
            && methodSymbol.ReturnType.SpecialType == SpecialType.System_String
            && methodSymbol.IsOverride;
    }

    /// <summary>
    /// Equals(object)オーバーライドか判定します。
    /// </summary>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    /// <returns>Equals(object)オーバーライドの場合は <see langword="true" /> を返します。</returns>
    private static bool IsEqualsObjectOverride(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Name.Equals("Equals", StringComparison.Ordinal)
            && methodSymbol.Parameters.Length == 1
            && methodSymbol.Parameters[0].Type.SpecialType == SpecialType.System_Object
            && methodSymbol.ReturnType.SpecialType == SpecialType.System_Boolean
            && methodSymbol.IsOverride;
    }

    /// <summary>
    /// Equals(T)か判定します。
    /// </summary>
    /// <param name="namedType">型シンボルです。</param>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    /// <returns>Equals(T)の場合は <see langword="true" /> を返します。</returns>
    private static bool IsTypedEquals(INamedTypeSymbol namedType, IMethodSymbol methodSymbol)
    {
        return methodSymbol.Name.Equals("Equals", StringComparison.Ordinal)
            && methodSymbol.Parameters.Length == 1
            && methodSymbol.ReturnType.SpecialType == SpecialType.System_Boolean
            && SymbolEqualityComparer.Default.Equals(methodSymbol.Parameters[0].Type, namedType);
    }

    /// <summary>
    /// GetHashCodeオーバーライドか判定します。
    /// </summary>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    /// <returns>GetHashCodeオーバーライドの場合は <see langword="true" /> を返します。</returns>
    private static bool IsGetHashCodeOverride(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Name.Equals("GetHashCode", StringComparison.Ordinal)
            && methodSymbol.Parameters.Length == 0
            && methodSymbol.ReturnType.SpecialType == SpecialType.System_Int32
            && methodSymbol.IsOverride;
    }
}
