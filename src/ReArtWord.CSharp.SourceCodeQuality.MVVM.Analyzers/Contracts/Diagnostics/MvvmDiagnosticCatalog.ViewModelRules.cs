using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Contracts.Diagnostics;

/// <summary>
/// MVVM品質AnalyzerのViewModel追加診断定義です。
/// </summary>
internal static partial class MvvmDiagnosticCatalog
{
    /// <summary>
    /// ViewModel接尾辞必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ViewModelSuffixRequiredRule = new(
        "RAWMVVM009",
        "ViewModel接尾辞必須",
        "ObservableObject/ObservableRecipient/ObservableValidatorを継承するViewModel候補はViewModel接尾辞を付けてください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Observable系基底型を継承する型をViewModelとして明示できるよう、ViewModel接尾辞を必須化します.");

    /// <summary>
    /// ViewModel基底型必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ViewModelObservableBaseRequiredRule = new(
        "RAWMVVM010",
        "ViewModel基底型必須",
        "ViewModel命名クラスはObservableObject/ObservableRecipient/ObservableValidatorのいずれかを継承してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ViewModel命名クラスへMVVM ToolkitのObservable系基底型継承を必須化します.");

    /// <summary>
    /// INotifyPropertyChanged直接実装禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor DirectNotifyPropertyChangedImplementationRule = new(
        "RAWMVVM011",
        "INotifyPropertyChanged直接実装禁止",
        "INotifyPropertyChangedを直接実装せず、ObservableObject/ObservableRecipient/ObservableValidatorを継承してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ObservableObject継承を迂回したINotifyPropertyChanged直接実装を禁止します.");

    /// <summary>
    /// PropertyChanged直接購読禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor PropertyChangedSubscriptionRule = new(
        "RAWMVVM012",
        "PropertyChanged直接購読禁止",
        "ViewModel内でPropertyChangedイベントを直接購読せず、Messenger等による疎結合な通知へ移行してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ViewModel内のPropertyChangedEventHandler直接購読を禁止します.");

    /// <summary>
    /// ObservableProperty対象フィールドprivate限定ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ObservablePropertyPrivateFieldRule = new(
        "RAWMVVM013",
        "ObservableProperty対象フィールドprivate限定",
        "[ObservableProperty]対象フィールドはprivateに限定してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ObservableProperty属性を付与するfieldの公開範囲をprivateへ限定します.");

    /// <summary>
    /// ObservableProperty対象フィールドstatic/readonly禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ObservablePropertyInstanceMutableFieldRule = new(
        "RAWMVVM014",
        "ObservableProperty対象フィールドstatic/readonly禁止",
        "[ObservableProperty]対象フィールドではstaticまたはreadonlyを使用しないでください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ObservableProperty属性を付与するfieldはインスタンス可変フィールドに限定します.");

    /// <summary>
    /// ViewModel内Service直接生成禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ViewModelServiceCreationRule = new(
        "RAWMVVM015",
        "ViewModel内Service直接生成禁止",
        "ViewModel内でServiceを直接newせず、DIから受け取ってください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ViewModelの責務から外れるServiceインスタンス生成を禁止します.");

    /// <summary>
    /// PropertyChanged/PropertyChanging手動発火禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ManualPropertyChangedInvocationRule = new(
        "RAWMVVM016",
        "通知イベント手動発火禁止",
        "PropertyChanged/PropertyChangingイベントを手動発火せず、ObservablePropertyやNotifyPropertyChangedForを使用してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "手動実装コードにおけるPropertyChanged/PropertyChangingイベントの直接呼び出しを禁止します.");

    /// <summary>
    /// OnPropertyChanged依存通知禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor NotifyPropertyChangedForDependencyRule = new(
        "RAWMVVM017",
        "NotifyPropertyChangedFor利用必須",
        "ViewModel内でOnPropertyChanged(nameof(...))による依存通知を行わず、[NotifyPropertyChangedFor]を使用してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ViewModel内のnameof付きOnPropertyChanged依存通知をNotifyPropertyChangedFor属性へ統一します.");

    /// <summary>
    /// RelayCommand対象メソッド名ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor RelayCommandMethodNameRule = new(
        "RAWMVVM018",
        "RelayCommand対象メソッド名Command接尾辞禁止",
        "[RelayCommand]対象メソッド名をCommandで終えないでください。生成後のCommand名が重複します。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "RelayCommand生成後のXCommandCommand命名を避けるため、対象メソッド名のCommand接尾辞を禁止します.");

    /// <summary>
    /// RelayCommand対象メソッドprivate限定ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor RelayCommandPrivateMethodRule = new(
        "RAWMVVM019",
        "RelayCommand対象メソッドprivate限定",
        "[RelayCommand]対象メソッドはprivateにしてください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "RelayCommand対象メソッドの公開範囲をprivateへ限定します.");

    /// <summary>
    /// RelayCommand対象メソッド戻り値ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor RelayCommandReturnTypeRule = new(
        "RAWMVVM020",
        "RelayCommand対象メソッド戻り値制限",
        "[RelayCommand]対象メソッドの戻り値はvoid/Task/ValueTaskのいずれかにしてください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "RelayCommand対象メソッドの戻り値をMVVM Toolkitの標準Command生成用途へ限定します.");

    /// <summary>
    /// On{Name}Changed内自己再代入禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ObservablePropertyChangedReassignmentRule = new(
        "RAWMVVM021",
        "OnChanged内自己再代入禁止",
        "On{Name}Changed内で同じプロパティへ再代入しないでください。無限通知や再入の原因になります。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ObservableProperty変更hook内で同じプロパティを再代入する実装を禁止します.");

    /// <summary>
    /// ViewModelからView参照禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ViewModelViewReferenceRule = new(
        "RAWMVVM022",
        "ViewModelからView参照禁止",
        "ViewModelからView/Control/Page/Window/MessageBox等を参照せず、UI層と責務を分離してください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ViewModelからWPF・MAUI・AvaloniaのUI基底型とその派生型、およびWPFのMessageBoxへの参照を禁止します.");

    /// <summary>
    /// ViewModelからApplication.Current直接参照禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ViewModelApplicationCurrentRule = new(
        "RAWMVVM023",
        "Application.Current直接参照禁止",
        "ViewModelからApplication.Currentを直接参照せず、必要な機能を抽象化してDIから受け取ってください。",
        "MVVM",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ViewModelからWPF/MAUI/AvaloniaのApplication.Currentを直接参照する実装を禁止します.");

    /// <summary>
    /// ViewModel sealed/abstract明示推奨ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ViewModelSealedOrAbstractRule = new(
        "RAWMVVM024",
        "ViewModel sealed/abstract明示推奨",
        "ViewModelクラスはsealedまたはabstractを明示することを検討してください。",
        "MVVM",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ViewModel型の継承方針を明示するため、sealedまたはabstract指定を推奨します.");

    /// <summary>
    /// ObservablePropertyコレクション型推奨ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ObservablePropertyCollectionRule = new(
        "RAWMVVM025",
        "ObservablePropertyコレクション通知推奨",
        "IEnumerable/IReadOnlyList型の[ObservableProperty]では、WPF/MAUI/Avalonia等での項目変更通知にObservableCollection/ReadOnlyObservableCollection等の利用を検討してください。",
        "MVVM",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ObservableProperty対象の列挙型プロパティに対し、UI更新へ適した通知可能コレクションへの移行検討を促します.");

    /// <summary>
    /// ViewModel publicプロパティ変更通知推奨ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ViewModelPublicPropertyNotificationRule = new(
        "RAWMVVM026",
        "ViewModel publicプロパティ変更通知推奨",
        "ViewModelのpublicな可変プロパティは変更通知を実装してください。ViewModel責務では通知の無いプロパティは少ない想定です。",
        "MVVM",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "ViewModelのpublic可変プロパティに対して変更通知の実装確認を促します.");

    /// <summary>
    /// RelayCommand並列実行理由コメント必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor RelayCommandConcurrentExecutionCommentRule = new(
        "RAWMVVM027",
        "RelayCommand並列実行理由コメント必須",
        "asyncな[RelayCommand]でAllowConcurrentExecutions = trueを指定する場合は、直前に日本語コメントで理由を記載してください。",
        "MVVM",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "非同期RelayCommandの並列実行許可には、直前コメントによる理由明示を必須化します.");
}
