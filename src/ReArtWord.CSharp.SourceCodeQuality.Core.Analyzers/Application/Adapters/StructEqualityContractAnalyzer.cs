using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// structの等値性実装整合性を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class StructEqualityContractAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.StructEqualityContractRule);

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
        if (context.Symbol is not INamedTypeSymbol namedType
            || namedType.TypeKind != TypeKind.Struct)
        {
            return;
        }

        if (!IsTargetProductionType(namedType, context.Compilation))
        {
            return;
        }

        bool hasEqualsObject = HasEqualsObject(namedType);
        bool hasTypedEquals = HasTypedEquals(namedType);
        bool hasGetHashCode = HasGetHashCode(namedType);
        bool hasEqualityOperator = HasOperator(namedType, "op_Equality");
        bool hasInequalityOperator = HasOperator(namedType, "op_Inequality");

        bool hasAny = hasEqualsObject || hasTypedEquals || hasGetHashCode || hasEqualityOperator || hasInequalityOperator;
        if (!hasAny)
        {
            return;
        }

        List<string> missingMembers = [];
        if (!hasEqualsObject)
        {
            missingMembers.Add("Equals(object)");
        }

        if (!hasTypedEquals)
        {
            missingMembers.Add("Equals(T)");
        }

        if (!hasGetHashCode)
        {
            missingMembers.Add("GetHashCode()");
        }

        if (!hasEqualityOperator)
        {
            missingMembers.Add("operator ==");
        }

        if (!hasInequalityOperator)
        {
            missingMembers.Add("operator !=");
        }

        if (missingMembers.Count == 0)
        {
            return;
        }

        Location location = GetPrimaryLocation(namedType);
        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.StructEqualityContractRule,
                location,
                namedType.Name,
                string.Join(", ", missingMembers)));
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
    /// Equals(object)の実装有無を判定します。
    /// </summary>
    /// <param name="namedType">型シンボルです。</param>
    /// <returns>実装済みの場合は <see langword="true" /> を返します。</returns>
    private static bool HasEqualsObject(INamedTypeSymbol namedType)
    {
        return namedType.GetMembers("Equals")
            .OfType<IMethodSymbol>()
            .Any(method =>
                method.Parameters.Length == 1
                && method.Parameters[0].Type.SpecialType == SpecialType.System_Object
                && method.ReturnType.SpecialType == SpecialType.System_Boolean
                && method.IsOverride);
    }

    /// <summary>
    /// Equals(T)の実装有無を判定します。
    /// </summary>
    /// <param name="namedType">型シンボルです。</param>
    /// <returns>実装済みの場合は <see langword="true" /> を返します。</returns>
    private static bool HasTypedEquals(INamedTypeSymbol namedType)
    {
        return namedType.GetMembers("Equals")
            .OfType<IMethodSymbol>()
            .Any(method =>
                method.Parameters.Length == 1
                && method.ReturnType.SpecialType == SpecialType.System_Boolean
                && !method.IsStatic
                && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, namedType));
    }

    /// <summary>
    /// GetHashCode()の実装有無を判定します。
    /// </summary>
    /// <param name="namedType">型シンボルです。</param>
    /// <returns>実装済みの場合は <see langword="true" /> を返します。</returns>
    private static bool HasGetHashCode(INamedTypeSymbol namedType)
    {
        return namedType.GetMembers("GetHashCode")
            .OfType<IMethodSymbol>()
            .Any(method =>
                method.Parameters.Length == 0
                && method.ReturnType.SpecialType == SpecialType.System_Int32
                && method.IsOverride);
    }

    /// <summary>
    /// 演算子の実装有無を判定します。
    /// </summary>
    /// <param name="namedType">型シンボルです。</param>
    /// <param name="metadataName">演算子メタデータ名です。</param>
    /// <returns>実装済みの場合は <see langword="true" /> を返します。</returns>
    private static bool HasOperator(INamedTypeSymbol namedType, string metadataName)
    {
        return namedType.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(method =>
                method.MethodKind == MethodKind.UserDefinedOperator
                && method.Name.Equals(metadataName, StringComparison.Ordinal)
                && method.Parameters.Length == 2
                && SymbolEqualityComparer.Default.Equals(method.Parameters[0].Type, namedType)
                && SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, namedType));
    }
}
