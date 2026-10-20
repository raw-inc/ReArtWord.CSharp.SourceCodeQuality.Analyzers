using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

/// <summary>
/// ViewModel判定とMVVM関連型判定を提供します。
/// </summary>
internal static class MvvmViewModelSymbolHelper
{
    private static readonly string ComponentModelNamespace = string.Concat("CommunityToolkit.Mvvm", ".ComponentModel");
    private static readonly string ToolkitNamespacePrefix = string.Concat("CommunityToolkit", ".Mvvm");
    private static readonly string SystemComponentModelNamespace = string.Concat("System", ".ComponentModel");
    private static readonly string SystemCollectionsNamespace = string.Concat("System", ".Collections");
    private static readonly string GenericNamespace = string.Concat("System.Collections", ".Generic");
    private static readonly string ObjectModelNamespace = string.Concat("System.Collections", ".ObjectModel");
    private static readonly string TasksNamespace = string.Concat("System.Threading", ".Tasks");
    private static readonly string ViewModelSuffix = string.Concat("View", "Model");
    private static readonly string NotifyPropertyChangedInterfaceName = string.Concat("INotifyProperty", "Changed");
    private static readonly string PropertyChangedEventName = string.Concat("Property", "Changed");
    private static readonly string PropertyChangingEventName = string.Concat("Property", "Changing");
    private static readonly string PropertyChangedEventHandlerName = string.Concat("PropertyChanged", "EventHandler");
    private static readonly string PropertyChangingEventHandlerName = string.Concat("PropertyChanging", "EventHandler");
    private static readonly string ServiceSuffix = string.Concat("Ser", "vice");
    private static readonly string TaskTypeName = string.Concat("Ta", "sk");
    private static readonly string ValueTaskTypeName = string.Concat("Value", "Task");
    private static readonly string ObservableCollectionTypeName = string.Concat("Observable", "Collection");
    private static readonly string ReadOnlyObservableCollectionTypeName = string.Concat("ReadOnlyObservable", "Collection");
    private static readonly string EnumerableTypeName = string.Concat("IEnum", "erable");
    private static readonly string ReadOnlyListTypeName = string.Concat("IReadOnly", "List");

    private static readonly string[] ToolkitObservableTypes =
    [
        "ObservableObject",
        "ObservableRecipient",
        "ObservableValidator",
    ];

    /// <summary>
    /// 型がViewModel対象か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>ViewModel対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsViewModel(INamedTypeSymbol? typeSymbol)
    {
        return typeSymbol is not null
            && (HasViewModelSuffix(typeSymbol) || HasToolkitObservableBase(typeSymbol));
    }

    /// <summary>
    /// 型がCommunityToolkit.Mvvm自身の型か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>Toolkit自身の型の場合は <see langword="true" /> を返します。</returns>
    public static bool IsToolkitOwnedType(INamedTypeSymbol? typeSymbol)
    {
        return typeSymbol is not null
            && typeSymbol.ContainingNamespace.ToDisplayString().StartsWith(ToolkitNamespacePrefix, StringComparison.Ordinal);
    }

    /// <summary>
    /// 型名がViewModel接尾辞を持つか判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>ViewModel接尾辞を持つ場合は <see langword="true" /> を返します。</returns>
    public static bool HasViewModelSuffix(INamedTypeSymbol? typeSymbol)
    {
        return typeSymbol is not null
            && typeSymbol.Name.EndsWith(ViewModelSuffix, StringComparison.Ordinal);
    }

    /// <summary>
    /// 型がMVVM ToolkitのObservable系基底型を継承しているか判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>継承している場合は <see langword="true" /> を返します。</returns>
    public static bool HasToolkitObservableBase(INamedTypeSymbol? typeSymbol)
    {
        INamedTypeSymbol? currentType = typeSymbol?.BaseType;
        while (currentType is not null)
        {
            if (IsToolkitObservableType(currentType))
            {
                return true;
            }

            currentType = currentType.BaseType;
        }

        return false;
    }

    /// <summary>
    /// 型がINotifyPropertyChangedか判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>INotifyPropertyChangedの場合は <see langword="true" /> を返します。</returns>
    public static bool IsNotifyPropertyChangedInterface(ITypeSymbol? typeSymbol)
    {
        return IsSystemComponentModelType(typeSymbol, NotifyPropertyChangedInterfaceName);
    }

    /// <summary>
    /// シンボルがPropertyChanged/PropertyChanging通知イベントか判定します。
    /// </summary>
    /// <param name="symbol">対象シンボルです。</param>
    /// <returns>通知イベントの場合は <see langword="true" /> を返します。</returns>
    public static bool IsNotifyEventSymbol(ISymbol? symbol)
    {
        if (symbol is null)
        {
            return false;
        }

        ITypeSymbol? typeSymbol = symbol switch
        {
            IEventSymbol eventSymbol => eventSymbol.Type,
            IFieldSymbol fieldSymbol => fieldSymbol.Type,
            IPropertySymbol propertySymbol => propertySymbol.Type,
            _ => null,
        };

        return IsPropertyChangedEvent(symbol.Name, typeSymbol)
            || IsPropertyChangingEvent(symbol.Name, typeSymbol);
    }

    /// <summary>
    /// 型がService直接生成の対象か判定します。
    /// </summary>
    /// <param name="typeSymbol">生成型です。</param>
    /// <returns>Service直接生成対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsServiceCreationType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType || namedType.TypeKind != TypeKind.Class)
        {
            return false;
        }

        if (HasServiceSuffix(namedType.Name))
        {
            return true;
        }

        return namedType.AllInterfaces.Any(static interfaceSymbol => HasServiceSuffix(interfaceSymbol.Name));
    }

    /// <summary>
    /// 型がObservablePropertyで警告対象の列挙型か判定します。
    /// </summary>
    /// <param name="typeSymbol">対象型です。</param>
    /// <returns>警告対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsObservablePropertyEnumerableType(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
        {
            return false;
        }

        INamedTypeSymbol originalType = namedType.OriginalDefinition;
        if (IsObservableCollectionType(originalType))
        {
            return false;
        }

        return IsEnumerableType(originalType)
            || originalType.AllInterfaces.Any(static interfaceSymbol => IsEnumerableType(interfaceSymbol.OriginalDefinition));
    }

    /// <summary>
    /// 型が非ジェネリックTaskまたはValueTaskか判定します。
    /// </summary>
    /// <param name="typeSymbol">対象型です。</param>
    /// <returns>許可対象の場合は <see langword="true" /> を返します。</returns>
    public static bool IsNonGenericTaskOrValueTask(ITypeSymbol? typeSymbol)
    {
        return typeSymbol is INamedTypeSymbol namedType
            && namedType.Arity == 0
            && (IsNamedType(namedType, TasksNamespace, TaskTypeName)
                || IsNamedType(namedType, TasksNamespace, ValueTaskTypeName));
    }

    /// <summary>
    /// 型がTaskまたはValueTask系か判定します。
    /// </summary>
    /// <param name="typeSymbol">対象型です。</param>
    /// <returns>TaskまたはValueTask系の場合は <see langword="true" /> を返します。</returns>
    public static bool IsTaskOrValueTask(ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedType)
        {
            return false;
        }

        string namespaceName = namedType.ContainingNamespace.ToDisplayString();
        return namespaceName.Equals(TasksNamespace, StringComparison.Ordinal)
            && (namedType.Name.Equals(TaskTypeName, StringComparison.Ordinal)
                || namedType.Name.Equals(ValueTaskTypeName, StringComparison.Ordinal));
    }

    /// <summary>
    /// 型が指定namespaceと名前に一致するか判定します。
    /// </summary>
    /// <param name="typeSymbol">対象型です。</param>
    /// <param name="namespaceName">namespace名です。</param>
    /// <param name="typeName">型名です。</param>
    /// <returns>一致する場合は <see langword="true" /> を返します。</returns>
    public static bool IsNamedType(ITypeSymbol? typeSymbol, string namespaceName, string typeName)
    {
        return typeSymbol is INamedTypeSymbol namedType
            && namedType.Name.Equals(typeName, StringComparison.Ordinal)
            && namedType.ContainingNamespace.ToDisplayString().Equals(namespaceName, StringComparison.Ordinal);
    }

    /// <summary>
    /// 型がMVVM ToolkitのObservable系型か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <returns>Observable系型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsToolkitObservableType(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.ContainingNamespace.ToDisplayString().Equals(ComponentModelNamespace, StringComparison.Ordinal)
            && ToolkitObservableTypes.Contains(typeSymbol.Name, StringComparer.Ordinal);
    }

    /// <summary>
    /// 型がSystem.ComponentModel配下の指定型か判定します。
    /// </summary>
    /// <param name="typeSymbol">型シンボルです。</param>
    /// <param name="typeName">型名です。</param>
    /// <returns>指定型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsSystemComponentModelType(ITypeSymbol? typeSymbol, string typeName)
    {
        return IsNamedType(typeSymbol, SystemComponentModelNamespace, typeName);
    }

    /// <summary>
    /// PropertyChangedイベントか判定します。
    /// </summary>
    /// <param name="eventName">イベント名です。</param>
    /// <param name="typeSymbol">イベント型です。</param>
    /// <returns>PropertyChangedイベントの場合は <see langword="true" /> を返します。</returns>
    private static bool IsPropertyChangedEvent(string? eventName, ITypeSymbol? typeSymbol)
    {
        return eventName is not null
            && eventName.Equals(PropertyChangedEventName, StringComparison.Ordinal)
            && IsSystemComponentModelType(typeSymbol, PropertyChangedEventHandlerName);
    }

    /// <summary>
    /// PropertyChangingイベントか判定します。
    /// </summary>
    /// <param name="eventName">イベント名です。</param>
    /// <param name="typeSymbol">イベント型です。</param>
    /// <returns>PropertyChangingイベントの場合は <see langword="true" /> を返します。</returns>
    private static bool IsPropertyChangingEvent(string? eventName, ITypeSymbol? typeSymbol)
    {
        return eventName is not null
            && eventName.Equals(PropertyChangingEventName, StringComparison.Ordinal)
            && IsSystemComponentModelType(typeSymbol, PropertyChangingEventHandlerName);
    }

    /// <summary>
    /// 名前がService接尾辞を持つか判定します。
    /// </summary>
    /// <param name="name">対象名です。</param>
    /// <returns>Service接尾辞の場合は <see langword="true" /> を返します。</returns>
    private static bool HasServiceSuffix(string name)
    {
        return name.EndsWith(ServiceSuffix, StringComparison.Ordinal);
    }

    /// <summary>
    /// 型が通知可能コレクション型か判定します。
    /// </summary>
    /// <param name="typeSymbol">対象型です。</param>
    /// <returns>通知可能コレクション型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsObservableCollectionType(INamedTypeSymbol typeSymbol)
    {
        return typeSymbol.ContainingNamespace.ToDisplayString().Equals(ObjectModelNamespace, StringComparison.Ordinal)
            && (typeSymbol.Name.Equals(ObservableCollectionTypeName, StringComparison.Ordinal)
                || typeSymbol.Name.Equals(ReadOnlyObservableCollectionTypeName, StringComparison.Ordinal));
    }

    /// <summary>
    /// 型がIEnumerableまたはIReadOnlyListか判定します。
    /// </summary>
    /// <param name="typeSymbol">対象型です。</param>
    /// <returns>対象型の場合は <see langword="true" /> を返します。</returns>
    private static bool IsEnumerableType(INamedTypeSymbol typeSymbol)
    {
        string namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
        if (typeSymbol.Name.Equals(EnumerableTypeName, StringComparison.Ordinal))
        {
            return namespaceName.Equals(SystemCollectionsNamespace, StringComparison.Ordinal)
                || namespaceName.Equals(GenericNamespace, StringComparison.Ordinal);
        }

        return typeSymbol.Name.Equals(ReadOnlyListTypeName, StringComparison.Ordinal)
            && namespaceName.Equals(GenericNamespace, StringComparison.Ordinal);
    }
}
