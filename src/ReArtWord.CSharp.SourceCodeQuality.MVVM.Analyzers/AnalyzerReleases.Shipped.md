## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------
RAWMVVM001 | MVVM | Error | MVVM Analyzer適用プロジェクトにCommunityToolkit.Mvvm参照を必須化します。
RAWMVVM002 | MVVM | Error | SetProperty呼び出しを禁止しObservableProperty利用を要求します。
RAWMVVM003 | MVVM | Error | SetProperty呼び出しを禁止しpartial void On{Name}Changing/Changed系hook利用を要求します。
RAWMVVM004 | MVVM | Error | OnPropertyChanged呼び出しを禁止しNotifyPropertyChangedFor利用を要求します。
RAWMVVM005 | MVVM | Error | 手動Command実装を禁止しRelayCommand属性利用を要求します。
RAWMVVM006 | MVVM | Error | RelayCommand属性のstringハードコーディングを禁止しnameof利用を要求します。
RAWMVVM007 | MVVM | Error | NotifyPropertyChangedFor属性のstringハードコーディングを禁止しnameof利用を要求します。
RAWMVVM008 | MVVM | Warning | field上のObservablePropertyに対してpartial property実装の検討を促します。
RAWMVVM009 | MVVM | Error | Observable系基底型を継承するViewModel候補へViewModel接尾辞を必須化します。
RAWMVVM010 | MVVM | Error | ViewModel命名クラスへObservable系基底型継承を必須化します。
RAWMVVM011 | MVVM | Error | INotifyPropertyChangedの直接実装を禁止します。
RAWMVVM012 | MVVM | Error | ViewModel内のPropertyChangedイベント直接購読を禁止します。
RAWMVVM013 | MVVM | Error | ObservableProperty対象フィールドをprivateに限定します。
RAWMVVM014 | MVVM | Error | ObservableProperty対象フィールドのstatic/readonlyを禁止します。
RAWMVVM015 | MVVM | Error | ViewModel内のService直接生成を禁止します。
RAWMVVM016 | MVVM | Error | PropertyChanged/PropertyChangingイベントの手動発火を禁止します。
RAWMVVM017 | MVVM | Error | OnPropertyChanged(nameof(...))による依存通知を禁止しNotifyPropertyChangedFor利用を要求します。
RAWMVVM018 | MVVM | Error | RelayCommand対象メソッド名のCommand接尾辞を禁止します。
RAWMVVM019 | MVVM | Error | RelayCommand対象メソッドをprivateに限定します。
RAWMVVM020 | MVVM | Error | RelayCommand対象メソッドの戻り値をvoid/Task/ValueTaskに限定します。
RAWMVVM021 | MVVM | Error | On{Name}Changed内で同じプロパティへ再代入することを禁止します。
RAWMVVM022 | MVVM | Error | ViewModelからView/Control/Page/Window/MessageBox等への参照を禁止します。
RAWMVVM023 | MVVM | Error | ViewModelからApplication.Currentを直接参照することを禁止します。
RAWMVVM024 | MVVM | Warning | ViewModelクラスへのsealedまたはabstract明示を推奨します。
RAWMVVM025 | MVVM | Warning | ObservableProperty対象のIEnumerable/IReadOnlyList型へ通知可能コレクションの検討を促します。
RAWMVVM026 | MVVM | Warning | ViewModelのpublic可変プロパティへ変更通知実装の確認を促します。
RAWMVVM027 | MVVM | Warning | async RelayCommandのAllowConcurrentExecutions=trueに日本語理由コメントを要求します。
