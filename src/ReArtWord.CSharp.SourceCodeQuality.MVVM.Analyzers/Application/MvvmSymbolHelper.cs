using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// MVVM Toolkit関連シンボルの判定を提供します。
/// </summary>
internal static class MvvmSymbolHelper
{
    private static readonly string ComponentModelNamespace = string.Concat("CommunityToolkit.Mvvm", ".ComponentModel");
    private static readonly string InputNamespace = string.Concat("CommunityToolkit.Mvvm", ".Input");
    private static readonly string ICommandNamespace = string.Concat("System.Windows", ".Input");

    private static readonly string[] ToolkitComponentModelTypes =
    [
        "ObservableObject",
        "ObservableRecipient",
        "ObservableValidator",
    ];

    /// <summary>
    /// `SetProperty` がMVVM Toolkit由来か判定します。
    /// </summary>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsToolkitSetProperty(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Name.Equals("SetProperty", StringComparison.Ordinal)
            && IsToolkitComponentModelType(methodSymbol.ContainingType);
    }

    /// <summary>
    /// `OnPropertyChanged` がMVVM Toolkit由来か判定します。
    /// </summary>
    /// <param name="methodSymbol">メソッドシンボルです。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsToolkitOnPropertyChanged(IMethodSymbol methodSymbol)
    {
        return methodSymbol.Name.Equals("OnPropertyChanged", StringComparison.Ordinal)
            && IsToolkitComponentModelType(methodSymbol.ContainingType);
    }

    /// <summary>
    /// Command手動実装対象の型か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsManualCommandType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
        {
            return false;
        }

        ITypeSymbol originalType = typeSymbol.OriginalDefinition;
        if (IsKnownCommandType(originalType))
        {
            return true;
        }

        return originalType.AllInterfaces.Any(IsKnownCommandType);
    }

    /// <summary>
    /// Command手動生成対象の具象型か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsManualCommandCreationType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
        {
            return false;
        }

        ITypeSymbol originalType = typeSymbol.OriginalDefinition;
        if (IsToolkitRelayCommandType(originalType))
        {
            return true;
        }

        return originalType.TypeKind == TypeKind.Class
            && originalType.AllInterfaces.Any(IsKnownCommandType);
    }

    /// <summary>
    /// ObservableProperty属性か判定します。
    /// </summary>
    /// <param name="attribute">属性構文です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsObservablePropertyAttribute(
        AttributeSyntax attribute,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        return IsAttribute(
            attribute,
            semanticModel,
            ComponentModelNamespace,
            "ObservablePropertyAttribute",
            "ObservableProperty",
            cancellationToken);
    }

    /// <summary>
    /// NotifyPropertyChangedFor属性か判定します。
    /// </summary>
    /// <param name="attribute">属性構文です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsNotifyPropertyChangedForAttribute(
        AttributeSyntax attribute,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        return IsAttribute(
            attribute,
            semanticModel,
            ComponentModelNamespace,
            "NotifyPropertyChangedForAttribute",
            "NotifyPropertyChangedFor",
            cancellationToken);
    }

    /// <summary>
    /// RelayCommand属性か判定します。
    /// </summary>
    /// <param name="attribute">属性構文です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsRelayCommandAttribute(
        AttributeSyntax attribute,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        return IsAttribute(
            attribute,
            semanticModel,
            InputNamespace,
            "RelayCommandAttribute",
            "RelayCommand",
            cancellationToken);
    }

    /// <summary>
    /// 型がMVVM ToolkitのComponentModel基底型か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    private static bool IsToolkitComponentModelType(INamedTypeSymbol? typeSymbol)
    {
        return typeSymbol is not null
            && typeSymbol.ContainingNamespace.ToDisplayString().Equals(ComponentModelNamespace, StringComparison.Ordinal)
            && ToolkitComponentModelTypes.Contains(typeSymbol.Name, StringComparer.Ordinal);
    }

    /// <summary>
    /// 型がCommand関連型か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    private static bool IsKnownCommandType(ITypeSymbol typeSymbol)
    {
        return IsSystemICommand(typeSymbol) || IsToolkitRelayCommandType(typeSymbol);
    }

    /// <summary>
    /// 型がSystem.Windows.Input.ICommandか判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    private static bool IsSystemICommand(ITypeSymbol typeSymbol)
    {
        return typeSymbol is INamedTypeSymbol
            && typeSymbol.Name.Equals("ICommand", StringComparison.Ordinal)
            && typeSymbol.ContainingNamespace.ToDisplayString().Equals(ICommandNamespace, StringComparison.Ordinal);
    }

    /// <summary>
    /// 型がMVVM ToolkitのRelayCommand関連型か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    private static bool IsToolkitRelayCommandType(ITypeSymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol)
        {
            return false;
        }

        string typeName = typeSymbol.Name;
        return typeSymbol.ContainingNamespace.ToDisplayString().Equals(InputNamespace, StringComparison.Ordinal)
            && (typeName.Equals("IRelayCommand", StringComparison.Ordinal)
                || typeName.Equals("IAsyncRelayCommand", StringComparison.Ordinal)
                || typeName.Equals("RelayCommand", StringComparison.Ordinal)
                || typeName.Equals("AsyncRelayCommand", StringComparison.Ordinal));
    }

    /// <summary>
    /// 属性が指定名に一致するか判定します。
    /// </summary>
    /// <param name="attribute">属性構文です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="namespaceName">名前空間名です。</param>
    /// <param name="attributeTypeName">属性型名です。</param>
    /// <param name="shortName">属性短縮名です。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>対象の場合は <see langword="true" /> を返します。</returns>
    private static bool IsAttribute(
        AttributeSyntax attribute,
        SemanticModel semanticModel,
        string namespaceName,
        string attributeTypeName,
        string shortName,
        CancellationToken cancellationToken)
    {
        SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(attribute, cancellationToken);
        if (symbolInfo.Symbol is IMethodSymbol constructorSymbol)
        {
            INamedTypeSymbol containingType = constructorSymbol.ContainingType;
            return containingType.Name.Equals(attributeTypeName, StringComparison.Ordinal)
                && containingType.ContainingNamespace.ToDisplayString().Equals(namespaceName, StringComparison.Ordinal);
        }

        string attributeName = attribute.Name.ToString();
        return attributeName.EndsWith(shortName, StringComparison.Ordinal)
            || attributeName.EndsWith(attributeTypeName, StringComparison.Ordinal);
    }
}
