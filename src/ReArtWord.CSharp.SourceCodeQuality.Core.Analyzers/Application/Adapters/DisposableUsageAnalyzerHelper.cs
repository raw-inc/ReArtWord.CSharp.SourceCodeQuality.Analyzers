using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// <see cref="DisposableUsageAnalyzer" /> の判定ヘルパーです。
/// </summary>
internal static class DisposableUsageAnalyzerHelper
{
    private const string DisposeMethodName = "Dispose";
    private const string DisposeAsyncMethodName = "DisposeAsync";

    /// <summary>
    /// 対象ノードを包含するメソッドシンボルを取得します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="node">対象ノードです。</param>
    /// <param name="methodSymbol">取得したメソッドシンボルです。</param>
    /// <returns>取得成功時は <see langword="true" /> を返します。</returns>
    public static bool TryGetContainingMethodSymbol(
        SyntaxNodeAnalysisContext context,
        SyntaxNode node,
        out IMethodSymbol? methodSymbol)
    {
        methodSymbol = null;
        MethodDeclarationSyntax? methodDeclaration = node.FirstAncestorOrSelf<MethodDeclarationSyntax>();
        if (methodDeclaration is null)
        {
            return false;
        }

        methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken) as IMethodSymbol;
        return methodSymbol is not null;
    }

    /// <summary>
    /// Dispose系メソッド宣言か判定します。
    /// </summary>
    /// <param name="methodSymbol">判定対象メソッドです。</param>
    /// <returns>Dispose系メソッドの場合は <see langword="true" /> を返します。</returns>
    public static bool IsDisposeMethodDeclaration(IMethodSymbol methodSymbol)
    {
        if (IsDisposeLikeName(methodSymbol.Name, methodSymbol.Parameters.Length))
        {
            return true;
        }

        foreach (IMethodSymbol implementedMethod in methodSymbol.ExplicitInterfaceImplementations)
        {
            if (IsDisposeLikeName(implementedMethod.Name, implementedMethod.Parameters.Length))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 対象クラスがIDisposable/IAsyncDisposable実装クラスか判定します。
    /// </summary>
    /// <param name="containingType">包含型です。</param>
    /// <param name="disposableTypeSymbol">IDisposable型シンボルです。</param>
    /// <param name="asyncDisposableTypeSymbol">IAsyncDisposable型シンボルです。</param>
    /// <returns>対象クラスの場合は <see langword="true" /> を返します。</returns>
    public static bool IsDisposableOwnerClass(
        INamedTypeSymbol containingType,
        INamedTypeSymbol? disposableTypeSymbol,
        INamedTypeSymbol? asyncDisposableTypeSymbol)
    {
        if (containingType.TypeKind != TypeKind.Class)
        {
            return false;
        }

        return ImplementsDisposableContract(containingType, disposableTypeSymbol, asyncDisposableTypeSymbol);
    }

    /// <summary>
    /// 呼び出し式のメソッドシンボルを取得します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="invocation">呼び出し式です。</param>
    /// <param name="methodSymbol">取得結果です。</param>
    /// <returns>取得成功時は <see langword="true" /> を返します。</returns>
    public static bool TryGetInvokedMethodSymbol(
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
    /// 明示Dispose呼び出しか判定し、表示名を取得します。
    /// </summary>
    /// <param name="methodSymbol">呼び出し先メソッドです。</param>
    /// <param name="disposableTypeSymbol">IDisposable型シンボルです。</param>
    /// <param name="asyncDisposableTypeSymbol">IAsyncDisposable型シンボルです。</param>
    /// <param name="invocationDisplayName">表示名です。</param>
    /// <returns>明示Dispose呼び出しの場合は <see langword="true" /> を返します。</returns>
    public static bool TryGetDisposeInvocationDisplayName(
        IMethodSymbol methodSymbol,
        INamedTypeSymbol? disposableTypeSymbol,
        INamedTypeSymbol? asyncDisposableTypeSymbol,
        out string invocationDisplayName)
    {
        invocationDisplayName = string.Empty;
        if (methodSymbol.Parameters.Length != 0 || methodSymbol.ContainingType is null)
        {
            return false;
        }

        if (methodSymbol.Name.Equals(DisposeMethodName, StringComparison.Ordinal)
            && methodSymbol.ReturnsVoid
            && ImplementsInterface(methodSymbol.ContainingType, disposableTypeSymbol))
        {
            invocationDisplayName = "Dispose()";
            return true;
        }

        if (!methodSymbol.Name.Equals(DisposeAsyncMethodName, StringComparison.Ordinal))
        {
            return false;
        }

        if (!IsTaskLikeReturnType(methodSymbol.ReturnType))
        {
            return false;
        }

        if (!ImplementsInterface(methodSymbol.ContainingType, asyncDisposableTypeSymbol))
        {
            return false;
        }

        invocationDisplayName = "DisposeAsync()";
        return true;
    }

    /// <summary>
    /// 型がIDisposable/IAsyncDisposableを実装するか判定します。
    /// </summary>
    /// <param name="typeSymbol">判定対象型です。</param>
    /// <param name="disposableTypeSymbol">IDisposable型シンボルです。</param>
    /// <param name="asyncDisposableTypeSymbol">IAsyncDisposable型シンボルです。</param>
    /// <returns>実装している場合は <see langword="true" /> を返します。</returns>
    public static bool ImplementsDisposableContract(
        ITypeSymbol typeSymbol,
        INamedTypeSymbol? disposableTypeSymbol,
        INamedTypeSymbol? asyncDisposableTypeSymbol)
    {
        if (typeSymbol is INamedTypeSymbol namedType)
        {
            return ImplementsNamedTypeContract(namedType, disposableTypeSymbol, asyncDisposableTypeSymbol);
        }

        if (typeSymbol is not ITypeParameterSymbol typeParameter)
        {
            return false;
        }

        foreach (ITypeSymbol constraintType in typeParameter.ConstraintTypes)
        {
            if (ImplementsDisposableContract(constraintType, disposableTypeSymbol, asyncDisposableTypeSymbol))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Dispose/DisposeAsyncに一致するメソッド名か判定します。
    /// </summary>
    /// <param name="methodName">メソッド名です。</param>
    /// <param name="parameterCount">引数数です。</param>
    /// <returns>一致する場合は <see langword="true" /> を返します。</returns>
    private static bool IsDisposeLikeName(string methodName, int parameterCount)
    {
        if (parameterCount != 0)
        {
            return false;
        }

        return methodName.Equals(DisposeMethodName, StringComparison.Ordinal)
            || methodName.Equals(DisposeAsyncMethodName, StringComparison.Ordinal);
    }

    /// <summary>
    /// 戻り値がTask/ValueTask系か判定します。
    /// </summary>
    /// <param name="returnType">戻り値型です。</param>
    /// <returns>Task/ValueTask系の場合は <see langword="true" /> を返します。</returns>
    private static bool IsTaskLikeReturnType(ITypeSymbol returnType)
    {
        if (returnType is not INamedTypeSymbol namedType)
        {
            return false;
        }

        INamedTypeSymbol normalizedType = namedType.ConstructedFrom;
        if (!normalizedType.ContainingNamespace.ToDisplayString().Equals("System.Threading.Tasks", StringComparison.Ordinal))
        {
            return false;
        }

        return normalizedType.Name.Equals("Task", StringComparison.Ordinal)
            || normalizedType.Name.Equals("ValueTask", StringComparison.Ordinal);
    }

    /// <summary>
    /// 名前付き型がIDisposable/IAsyncDisposableを実装するか判定します。
    /// </summary>
    /// <param name="namedType">判定対象型です。</param>
    /// <param name="disposableTypeSymbol">IDisposable型シンボルです。</param>
    /// <param name="asyncDisposableTypeSymbol">IAsyncDisposable型シンボルです。</param>
    /// <returns>実装している場合は <see langword="true" /> を返します。</returns>
    private static bool ImplementsNamedTypeContract(
        INamedTypeSymbol namedType,
        INamedTypeSymbol? disposableTypeSymbol,
        INamedTypeSymbol? asyncDisposableTypeSymbol)
    {
        if (ImplementsInterface(namedType, disposableTypeSymbol))
        {
            return true;
        }

        return ImplementsInterface(namedType, asyncDisposableTypeSymbol);
    }

    /// <summary>
    /// 型が指定インターフェイスを実装するか判定します。
    /// </summary>
    /// <param name="typeSymbol">判定対象型です。</param>
    /// <param name="interfaceSymbol">判定対象インターフェイスです。</param>
    /// <returns>実装している場合は <see langword="true" /> を返します。</returns>
    private static bool ImplementsInterface(ITypeSymbol typeSymbol, INamedTypeSymbol? interfaceSymbol)
    {
        if (interfaceSymbol is null || typeSymbol is not INamedTypeSymbol namedType)
        {
            return false;
        }

        return SymbolEqualityComparer.Default.Equals(namedType, interfaceSymbol)
            || namedType.AllInterfaces.Any(type => SymbolEqualityComparer.Default.Equals(type, interfaceSymbol));
    }
}
