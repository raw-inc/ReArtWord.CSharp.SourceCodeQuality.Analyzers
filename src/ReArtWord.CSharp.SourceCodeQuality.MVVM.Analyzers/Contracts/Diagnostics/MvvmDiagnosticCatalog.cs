using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Contracts.Diagnostics;

/// <summary>
/// MVVM品質Analyzerの診断定義です。
/// </summary>
internal static partial class MvvmDiagnosticCatalog
{
    /// <summary>
    /// サポートする診断定義一覧を作成します。
    /// </summary>
    /// <returns>診断定義一覧です。</returns>
    public static ImmutableArray<DiagnosticDescriptor> CreateSupportedDiagnostics()
    {
        return ImmutableArray.Create(
            ToolkitPackageRequiredRule,
            ObservablePropertyRequiredRule,
            PartialHookRequiredRule,
            NotifyPropertyChangedForRequiredRule,
            RelayCommandRequiredRule,
            RelayCommandNameofRequiredRule,
            NotifyPropertyChangedForNameofRequiredRule,
            ObservablePropertyPartialPropertyRule,
            ViewModelSuffixRequiredRule,
            ViewModelObservableBaseRequiredRule,
            DirectNotifyPropertyChangedImplementationRule,
            PropertyChangedSubscriptionRule,
            ObservablePropertyPrivateFieldRule,
            ObservablePropertyInstanceMutableFieldRule,
            ViewModelServiceCreationRule,
            ManualPropertyChangedInvocationRule,
            NotifyPropertyChangedForDependencyRule,
            RelayCommandMethodNameRule,
            RelayCommandPrivateMethodRule,
            RelayCommandReturnTypeRule,
            ObservablePropertyChangedReassignmentRule,
            ViewModelViewReferenceRule,
            ViewModelApplicationCurrentRule,
            ViewModelSealedOrAbstractRule,
            ObservablePropertyCollectionRule,
            ViewModelPublicPropertyNotificationRule,
            RelayCommandConcurrentExecutionCommentRule);
    }

    /// <summary>
    /// CommunityToolkit.Mvvm参照必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ToolkitPackageRequiredRule = new(
        "RAWMVVM001",
        "CommunityToolkit.Mvvm参照必須",
        "MVVM Analyzer適用プロジェクトではCommunityToolkit.MvvmをPackageReferenceまたは参照へ追加してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "MVVM Toolkit規約を適用するため、CommunityToolkit.Mvvmの導入を必須化します.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    /// <summary>
    /// SetProperty禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ObservablePropertyRequiredRule = new(
        "RAWMVVM002",
        "SetProperty利用禁止",
        "SetPropertyを手動で呼び出さず、[ObservableProperty]属性による生成プロパティを使用してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ObservableObject.SetPropertyの手動呼び出しを禁止し、MVVM ToolkitのObservableProperty生成へ統一します.");

    /// <summary>
    /// partial hook利用必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor PartialHookRequiredRule = new(
        "RAWMVVM003",
        "partial hook利用必須",
        "SetPropertyを手動で呼び出さず、partial void On{Name}Changing/On{Name}Changed系メソッドで変更処理を実装してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "プロパティ変更前後の処理はObservablePropertyが生成するpartial hookへ集約します.");

    /// <summary>
    /// NotifyPropertyChangedFor必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor NotifyPropertyChangedForRequiredRule = new(
        "RAWMVVM004",
        "NotifyPropertyChangedFor利用必須",
        "OnPropertyChangedを手動で呼び出さず、[NotifyPropertyChangedFor]属性を使用してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "依存プロパティ通知はOnPropertyChangedの手動呼び出しではなくNotifyPropertyChangedFor属性へ統一します.");

    /// <summary>
    /// RelayCommand属性必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor RelayCommandRequiredRule = new(
        "RAWMVVM005",
        "RelayCommand属性利用必須",
        "Commandを手動実装せず、[RelayCommand]属性で生成してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ICommandやRelayCommand系型の手動実装を禁止し、RelayCommand属性による生成へ統一します.");

    /// <summary>
    /// RelayCommand属性nameof必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor RelayCommandNameofRequiredRule = new(
        "RAWMVVM006",
        "RelayCommand属性nameof必須",
        "[RelayCommand]属性では文字列を直接指定せず、nameof(...)を使用してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "RelayCommand属性のメンバー名指定ではstringハードコーディングを禁止します.");

    /// <summary>
    /// NotifyPropertyChangedFor属性nameof必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor NotifyPropertyChangedForNameofRequiredRule = new(
        "RAWMVVM007",
        "NotifyPropertyChangedFor属性nameof必須",
        "[NotifyPropertyChangedFor]属性では文字列を直接指定せず、nameof(...)を使用してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "NotifyPropertyChangedFor属性のプロパティ名指定ではstringハードコーディングを禁止します.");

    /// <summary>
    /// ObservableProperty partial property推奨ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ObservablePropertyPartialPropertyRule = new(
        "RAWMVVM008",
        "ObservableProperty partial property推奨",
        "[ObservableProperty]はfieldではなくpartial propertyでの実装を検討してください。",
        "MVVM",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "field上のObservableProperty利用に対して、partial property形式への移行検討を促します.");
}
