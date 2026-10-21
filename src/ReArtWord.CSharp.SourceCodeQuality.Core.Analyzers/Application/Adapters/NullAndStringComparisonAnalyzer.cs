using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// null比較と文字列比較方法を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NullAndStringComparisonAnalyzer : DiagnosticAnalyzer
{
    private static readonly HashSet<string> StringSearchMethodNames = new(StringComparer.Ordinal)
    {
        "StartsWith",
        "EndsWith",
        "Contains",
        "IndexOf",
        "LastIndexOf",
    };

    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.NullComparisonOperatorRule,
            DiagnosticCatalog.StringComparisonCultureRule);

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
            AnalyzeBinaryExpression,
            SyntaxKind.EqualsExpression,
            SyntaxKind.NotEqualsExpression);
        context.RegisterSyntaxNodeAction(AnalyzeInvocationExpression, SyntaxKind.InvocationExpression);
    }

    /// <summary>
    /// 二項比較式を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeBinaryExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BinaryExpressionSyntax binaryExpression)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(binaryExpression.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (IsNullLiteral(binaryExpression.Left) || IsNullLiteral(binaryExpression.Right))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.NullComparisonOperatorRule,
                    binaryExpression.OperatorToken.GetLocation(),
                    binaryExpression.OperatorToken.ValueText));
            return;
        }

        if (!IsStringType(context.SemanticModel.GetTypeInfo(binaryExpression.Left, context.CancellationToken).ConvertedType)
            || !IsStringType(context.SemanticModel.GetTypeInfo(binaryExpression.Right, context.CancellationToken).ConvertedType))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.StringComparisonCultureRule,
                binaryExpression.OperatorToken.GetLocation(),
                binaryExpression.OperatorToken.ValueText));
    }

    /// <summary>
    /// 呼び出し式を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeInvocationExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(invocation.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (context.SemanticModel.GetSymbolInfo(invocation, context.CancellationToken).Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (!RequiresStringComparison(methodSymbol, invocation, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.StringComparisonCultureRule,
                GetInvocationNameLocation(invocation),
                methodSymbol.Name));
    }

    /// <summary>
    /// 文字列比較方法の明示が必要か判定します。
    /// </summary>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    /// <param name="invocation">呼び出し式です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセルトークンです。</param>
    /// <returns>明示が必要な場合は <see langword="true" /> を返します。</returns>
    private static bool RequiresStringComparison(
        IMethodSymbol methodSymbol,
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (!IsStringType(methodSymbol.ContainingType))
        {
            return false;
        }

        if (HasExplicitComparisonArgument(invocation, semanticModel, cancellationToken))
        {
            return false;
        }

        if (methodSymbol.Name.Equals("CompareOrdinal", StringComparison.Ordinal))
        {
            return false;
        }

        if (methodSymbol.Name.Equals("Compare", StringComparison.Ordinal))
        {
            return HasAtLeastTwoStringArguments(invocation, semanticModel, cancellationToken);
        }

        if (methodSymbol.Name.Equals("Equals", StringComparison.Ordinal))
        {
            return HasStringEqualsShape(invocation, semanticModel, cancellationToken);
        }

        if (!StringSearchMethodNames.Contains(methodSymbol.Name))
        {
            return false;
        }

        return HasFirstStringArgument(invocation, semanticModel, cancellationToken)
            && (!methodSymbol.Name.Equals("Contains", StringComparison.Ordinal)
                || SupportsContainsWithComparison(methodSymbol.ContainingType, semanticModel.Compilation));
    }

    /// <summary>
    /// 参照先のstring型に、比較方法を指定できるContainsがあるか判定します。
    /// </summary>
    /// <param name="stringType">参照先のstring型です。</param>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <returns>利用できるAPIがある場合は <see langword="true" /> を返します。</returns>
    private static bool SupportsContainsWithComparison(INamedTypeSymbol stringType, Compilation compilation)
    {
        INamedTypeSymbol? comparisonType = compilation.GetTypeByMetadataName("System.StringComparison");
        return comparisonType is not null
            && stringType.GetMembers("Contains").OfType<IMethodSymbol>().Any(method =>
                !method.IsStatic
                && method.DeclaredAccessibility == Accessibility.Public
                && method.Parameters.Length == 2
                && IsStringType(method.Parameters[0].Type)
                && SymbolEqualityComparer.Default.Equals(method.Parameters[1].Type, comparisonType));
    }

    /// <summary>
    /// 比較方針を示す引数があるか判定します。
    /// </summary>
    /// <param name="invocation">呼び出し式です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセルトークンです。</param>
    /// <returns>比較方針引数がある場合は <see langword="true" /> を返します。</returns>
    private static bool HasExplicitComparisonArgument(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        foreach (ArgumentSyntax argument in invocation.ArgumentList.Arguments)
        {
            ITypeSymbol? argumentType = semanticModel.GetTypeInfo(argument.Expression, cancellationToken).Type;
            if (IsExplicitComparisonType(argumentType))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// string.Equals形状か判定します。
    /// </summary>
    /// <param name="invocation">呼び出し式です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセルトークンです。</param>
    /// <returns>対象形状の場合は <see langword="true" /> を返します。</returns>
    private static bool HasStringEqualsShape(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (invocation.ArgumentList.Arguments.Count == 1)
        {
            return IsStringType(semanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression, cancellationToken).Type);
        }

        return HasAtLeastTwoStringArguments(invocation, semanticModel, cancellationToken);
    }

    /// <summary>
    /// 先頭2引数が文字列か判定します。
    /// </summary>
    /// <param name="invocation">呼び出し式です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセルトークンです。</param>
    /// <returns>先頭2引数が文字列の場合は <see langword="true" /> を返します。</returns>
    private static bool HasAtLeastTwoStringArguments(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        if (invocation.ArgumentList.Arguments.Count < 2)
        {
            return false;
        }

        return IsStringType(semanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression, cancellationToken).Type)
            && IsStringType(semanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[1].Expression, cancellationToken).Type);
    }

    /// <summary>
    /// 先頭引数が文字列か判定します。
    /// </summary>
    /// <param name="invocation">呼び出し式です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセルトークンです。</param>
    /// <returns>先頭引数が文字列の場合は <see langword="true" /> を返します。</returns>
    private static bool HasFirstStringArgument(
        InvocationExpressionSyntax invocation,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        return invocation.ArgumentList.Arguments.Count > 0
            && IsStringType(semanticModel.GetTypeInfo(invocation.ArgumentList.Arguments[0].Expression, cancellationToken).Type);
    }

    /// <summary>
    /// 比較方針を表す型か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>比較方針型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsExplicitComparisonType(ITypeSymbol? typeSymbol)
    {
        string? typeName = typeSymbol?.ToDisplayString();
        return typeName is "System.StringComparison"
            or "System.StringComparer"
            or "System.Globalization.CultureInfo"
            or "System.Globalization.CompareInfo"
            or "System.Globalization.CompareOptions";
    }

    /// <summary>
    /// string型か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>string型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsStringType(ITypeSymbol? typeSymbol)
    {
        return typeSymbol?.SpecialType == SpecialType.System_String;
    }

    /// <summary>
    /// nullリテラルか判定します。
    /// </summary>
    /// <param name="expression">式です。</param>
    /// <returns>nullリテラルの場合は <see langword="true" /> を返します。</returns>
    private static bool IsNullLiteral(ExpressionSyntax expression)
    {
        return expression.IsKind(SyntaxKind.NullLiteralExpression);
    }

    /// <summary>
    /// 呼び出し名の診断位置を取得します。
    /// </summary>
    /// <param name="invocation">呼び出し式です。</param>
    /// <returns>診断位置です。</returns>
    private static Location GetInvocationNameLocation(InvocationExpressionSyntax invocation)
    {
        return invocation.Expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name.GetLocation(),
            MemberBindingExpressionSyntax memberBinding => memberBinding.Name.GetLocation(),
            IdentifierNameSyntax identifier => identifier.Identifier.GetLocation(),
            _ => invocation.Expression.GetLocation(),
        };
    }
}
