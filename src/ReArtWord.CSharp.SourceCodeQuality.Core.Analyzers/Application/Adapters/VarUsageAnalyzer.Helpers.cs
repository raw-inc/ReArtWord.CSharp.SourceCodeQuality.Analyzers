using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// <see cref="VarUsageAnalyzer" /> の判定ヘルパー群です。
/// </summary>
public sealed partial class VarUsageAnalyzer
{
    /// <summary>
    /// RAW421を報告すべきか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="inferredType">推論型です。</param>
    /// <param name="valueExpression">値式です。</param>
    /// <returns>報告対象なら <see langword="true" /> を返します。</returns>
    private static bool ShouldReportSpl012(
        SyntaxNodeAnalysisContext context,
        ITypeSymbol inferredType,
        ExpressionSyntax? valueExpression)
    {
        ITypeSymbol normalizedType = RemoveNullable(inferredType);
        if (!IsPrimitiveOrStringType(normalizedType))
        {
            return false;
        }

        if (normalizedType.SpecialType == SpecialType.System_Boolean)
        {
            return false;
        }

        if (IsAllowedPrimitiveException(context, normalizedType, valueExpression))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// RAW420におけるvar利用許可条件を判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="inferredType">推論型です。</param>
    /// <param name="valueExpression">値式です。</param>
    /// <returns>許可条件を満たす場合は <see langword="true" /> を返します。</returns>
    private static bool IsVarUsageAllowed(
        SyntaxNodeAnalysisContext context,
        ITypeSymbol inferredType,
        ExpressionSyntax? valueExpression)
    {
        if (valueExpression is null)
        {
            return false;
        }

        if (valueExpression is AnonymousObjectCreationExpressionSyntax)
        {
            return true;
        }

        if (IsLinqAnonymousProjection(context, valueExpression))
        {
            return true;
        }

        return IsTypeSelfEvidentInitializer(context, inferredType, valueExpression);
    }

    /// <summary>
    /// LINQ匿名型投影かどうか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="valueExpression">値式です。</param>
    /// <returns>LINQ匿名型投影の場合は <see langword="true" /> を返します。</returns>
    private static bool IsLinqAnonymousProjection(SyntaxNodeAnalysisContext context, ExpressionSyntax valueExpression)
    {
        if (!valueExpression.DescendantNodesAndSelf().OfType<AnonymousObjectCreationExpressionSyntax>().Any())
        {
            return false;
        }

        if (valueExpression is QueryExpressionSyntax)
        {
            return true;
        }

        foreach (InvocationExpressionSyntax invocation in valueExpression.DescendantNodesAndSelf().OfType<InvocationExpressionSyntax>())
        {
            if (!TryGetInvocationMethodSymbol(context, invocation, out IMethodSymbol? methodSymbol)
                || methodSymbol is null)
            {
                continue;
            }

            if (methodSymbol.ContainingType is null)
            {
                continue;
            }

            string namespaceName = methodSymbol.ContainingType.ContainingNamespace.ToDisplayString();
            if (namespaceName.Equals("System.Linq", StringComparison.Ordinal)
                || namespaceName.StartsWith("System.Linq.", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 初期化式が型自明かどうか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="inferredType">推論型です。</param>
    /// <param name="valueExpression">値式です。</param>
    /// <returns>型自明な場合は <see langword="true" /> を返します。</returns>
    private static bool IsTypeSelfEvidentInitializer(
        SyntaxNodeAnalysisContext context,
        ITypeSymbol inferredType,
        ExpressionSyntax valueExpression)
    {
        if (valueExpression is ObjectCreationExpressionSyntax
            || valueExpression is ImplicitObjectCreationExpressionSyntax
            || valueExpression is ArrayCreationExpressionSyntax
            || valueExpression is ImplicitArrayCreationExpressionSyntax
            || valueExpression is StackAllocArrayCreationExpressionSyntax
            || valueExpression is LiteralExpressionSyntax)
        {
            return true;
        }

        if (valueExpression is DefaultExpressionSyntax defaultExpression)
        {
            ITypeSymbol? explicitType = context.SemanticModel.GetTypeInfo(defaultExpression.Type, context.CancellationToken).Type;
            return IsSameType(inferredType, explicitType);
        }

        if (valueExpression is CastExpressionSyntax castExpression)
        {
            ITypeSymbol? explicitType = context.SemanticModel.GetTypeInfo(castExpression.Type, context.CancellationToken).Type;
            return IsSameType(inferredType, explicitType);
        }

        return IsStaticMemberAccessOfSameType(context, inferredType, valueExpression);
    }

    /// <summary>
    /// RAW421における例外許可を判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="normalizedType">nullability除去済み推論型です。</param>
    /// <param name="valueExpression">値式です。</param>
    /// <returns>例外許可対象なら <see langword="true" /> を返します。</returns>
    private static bool IsAllowedPrimitiveException(
        SyntaxNodeAnalysisContext context,
        ITypeSymbol normalizedType,
        ExpressionSyntax? valueExpression)
    {
        if (normalizedType.SpecialType == SpecialType.System_Boolean)
        {
            return true;
        }

        if (valueExpression is null)
        {
            return false;
        }

        return IsStaticMemberAccessOfSameType(context, normalizedType, valueExpression);
    }

    /// <summary>
    /// 自明プリミティブ初期化かどうか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="inferredType">推論型です。</param>
    /// <param name="valueExpression">値式です。</param>
    /// <returns>自明プリミティブ初期化の場合は <see langword="true" /> を返します。</returns>
    private static bool IsObviousPrimitiveInitializer(
        SyntaxNodeAnalysisContext context,
        ITypeSymbol inferredType,
        ExpressionSyntax? valueExpression)
    {
        if (valueExpression is null)
        {
            return false;
        }

        if (valueExpression is LiteralExpressionSyntax)
        {
            return true;
        }

        if (valueExpression is PrefixUnaryExpressionSyntax prefixUnary
            && prefixUnary.Operand is LiteralExpressionSyntax)
        {
            return true;
        }

        return IsStaticMemberAccessOfSameType(context, inferredType, valueExpression);
    }

    /// <summary>
    /// 式が同一型の静的メンバーアクセスか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="inferredType">推論型です。</param>
    /// <param name="expression">値式です。</param>
    /// <returns>同一型静的メンバーアクセスの場合は <see langword="true" /> を返します。</returns>
    private static bool IsStaticMemberAccessOfSameType(
        SyntaxNodeAnalysisContext context,
        ITypeSymbol inferredType,
        ExpressionSyntax expression)
    {
        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken);
        ISymbol? symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();
        if (symbol is not IPropertySymbol and not IFieldSymbol)
        {
            return false;
        }

        if (!symbol.IsStatic || symbol.ContainingType is null)
        {
            return false;
        }

        return IsSameType(inferredType, symbol.ContainingType);
    }

    /// <summary>
    /// 呼び出し式のメソッドシンボルを取得します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="invocation">呼び出し式です。</param>
    /// <param name="methodSymbol">取得結果です。</param>
    /// <returns>取得成功時は <see langword="true" /> を返します。</returns>
    private static bool TryGetInvocationMethodSymbol(
        SyntaxNodeAnalysisContext context,
        InvocationExpressionSyntax invocation,
        out IMethodSymbol? methodSymbol)
    {
        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken);
        methodSymbol = symbolInfo.Symbol as IMethodSymbol
            ?? symbolInfo.CandidateSymbols.OfType<IMethodSymbol>().FirstOrDefault();
        if (methodSymbol is null)
        {
            return false;
        }

        methodSymbol = methodSymbol.ReducedFrom ?? methodSymbol;
        return true;
    }

    /// <summary>
    /// var型構文かどうか判定します。
    /// </summary>
    /// <param name="typeSyntax">型構文です。</param>
    /// <returns>var型構文の場合は <see langword="true" /> を返します。</returns>
    private static bool IsVarType(TypeSyntax typeSyntax)
    {
        return typeSyntax is IdentifierNameSyntax identifierName
            && identifierName.Identifier.Text.Equals("var", StringComparison.Ordinal);
    }

    /// <summary>
    /// プリミティブまたはstring型か判定します。
    /// </summary>
    /// <param name="typeSymbol">対象型です。</param>
    /// <returns>対象型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsPrimitiveOrStringType(ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType is SpecialType.System_Boolean
            or SpecialType.System_Char
            or SpecialType.System_SByte
            or SpecialType.System_Byte
            or SpecialType.System_Int16
            or SpecialType.System_UInt16
            or SpecialType.System_Int32
            or SpecialType.System_UInt32
            or SpecialType.System_Int64
            or SpecialType.System_UInt64
            or SpecialType.System_Single
            or SpecialType.System_Double
            or SpecialType.System_Decimal
            or SpecialType.System_String;
    }

    /// <summary>
    /// 型一致判定を行います。
    /// </summary>
    /// <param name="left">左辺型です。</param>
    /// <param name="right">右辺型です。</param>
    /// <returns>一致する場合は <see langword="true" /> を返します。</returns>
    private static bool IsSameType(ITypeSymbol left, ITypeSymbol? right)
    {
        if (right is null)
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(
            RemoveNullable(left),
            RemoveNullable(right));
    }

    /// <summary>
    /// Nullable注釈を除去します。
    /// </summary>
    /// <param name="typeSymbol">対象型です。</param>
    /// <returns>注釈除去済み型です。</returns>
    private static ITypeSymbol RemoveNullable(ITypeSymbol typeSymbol)
    {
        return typeSymbol is INamedTypeSymbol { IsGenericType: true } namedType
            && namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T
                ? namedType.TypeArguments[0]
                : typeSymbol;
    }
}
