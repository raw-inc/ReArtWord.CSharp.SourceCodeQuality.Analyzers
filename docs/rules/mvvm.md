# MVVM のルール

[ドキュメント一覧](../README.md) · [導入する](../guides/installation.md) · [検査を設定する](../guides/configuration.md)

CommunityToolkit.Mvvmを使うViewModelの変更通知、コマンド、画面との依存関係を検査します。
この一覧はv1.0.0の27ルールです。Coreと併用し、MVVMを実装するプロジェクトへ追加してください。

## 適用範囲

名前が `ViewModel` で終わるクラス、または `ObservableObject`／`ObservableRecipient`／`ObservableValidator` を継承するクラスをViewModelとして扱います。
生成コードと `obj` 配下は対象外です。通常のテストコードは、MVVMアナライザーを参照していれば検査対象になります。

`RAWMVVM001` はプロジェクト単位の検査です。必要な参照がない場合に1件報告します。
その他の規則は、対象となる宣言や呼出しへ診断を出します。

## 参照・変更通知・プロパティ

| ID | 既定 | 検査内容 |
| --- | --- | --- |
| RAWMVVM001 | Error | `CommunityToolkit.Mvvm` の参照を要求 |
| RAWMVVM002 | Error | `SetProperty(...)` を検出し、`ObservableProperty` によるプロパティ定義を要求 |
| RAWMVVM003 | Error | `SetProperty(...)` を検出し、変更時の処理を `On{Name}Changing`／`On{Name}Changed` へ置くよう案内 |
| RAWMVVM004 | Error | `OnPropertyChanged(...)` を検出し、依存するプロパティの通知に `NotifyPropertyChangedFor` を要求 |
| RAWMVVM007 | Error | `NotifyPropertyChangedFor` の文字列引数に `nameof(...)` を要求 |
| RAWMVVM008 | Warning | フィールドの `ObservableProperty` に対して、partialプロパティの利用を提案 |
| RAWMVVM009 | Error | Observable系の基底型を継承するクラス名に `ViewModel` 接尾辞を要求 |
| RAWMVVM010 | Error | `ViewModel` で終わるクラスに、Observable系の基底型の継承を要求 |
| RAWMVVM011 | Error | `INotifyPropertyChanged` の直接実装を検出 |
| RAWMVVM012 | Error | ViewModel内の `PropertyChanged` イベントへの直接購読を検出 |
| RAWMVVM013 | Error | `ObservableProperty` を付けたフィールドをprivateに限定 |
| RAWMVVM014 | Error | `ObservableProperty` を付けたフィールドのstatic／readonly指定を検出 |
| RAWMVVM016 | Error | `PropertyChanged`／`PropertyChanging` イベントの手動発火を検出 |
| RAWMVVM017 | Error | `OnPropertyChanged(nameof(...))` による依存通知を検出 |
| RAWMVVM021 | Error | `On{Name}Changed` 内で同じプロパティへ再代入する処理を検出 |
| RAWMVVM025 | Warning | `ObservableProperty` 対象のIEnumerable／IReadOnlyList型に、変更を通知できるコレクションを提案 |
| RAWMVVM026 | Warning | ViewModelのpublicな可変プロパティに、変更通知の実装を提案 |

`SetProperty` の検査は、プロパティ定義と変更時の処理の両方を対象とするため、`RAWMVVM002` と `RAWMVVM003` が同じ呼出しに出る場合があります。

## コマンド

| ID | 既定 | 検査内容 |
| --- | --- | --- |
| RAWMVVM005 | Error | ICommand／RelayCommand系の手動実装を検出し、`RelayCommand` 属性を要求 |
| RAWMVVM006 | Error | `RelayCommand` の文字列引数に `nameof(...)` を要求 |
| RAWMVVM018 | Error | `RelayCommand` 対象メソッド名の `Command` 接尾辞を検出 |
| RAWMVVM019 | Error | `RelayCommand` 対象メソッドをprivateに限定 |
| RAWMVVM020 | Error | `RelayCommand` 対象メソッドの戻り値をvoid／Task／ValueTaskに限定 |
| RAWMVVM027 | Warning | asyncコマンドの `AllowConcurrentExecutions = true` に、日本語の理由コメントを要求 |

非同期処理の取消トークンやawaitの扱いは、[Coreの非同期処理の規則](core.md#非同期処理命名)と併せて確認してください。
実際に使用できるメソッド宣言やソース生成の条件は、利用中のCommunityToolkit.Mvvmのバージョンにも依存します。

## ViewModelの責務

| ID | 既定 | 検査内容 |
| --- | --- | --- |
| RAWMVVM015 | Error | ViewModel内でのServiceの直接生成を検出 |
| RAWMVVM022 | Error | ViewModelからView／Control／Page／Window／MessageBoxなどの画面型への参照を検出 |
| RAWMVVM023 | Error | ViewModelからの `Application.Current` 参照を検出 |
| RAWMVVM024 | Warning | ViewModelクラスにsealedまたはabstractの明示を提案 |

`RAWMVVM022` は列挙型を対象外にしています。`Page` などを含む状態名だけで、画面型への参照とは扱いません。

## リリース情報

診断ID・カテゴリ・既定値のリリース記録は、[MVVM の Shipped](../../src/ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers/AnalyzerReleases.Shipped.md)にあります。
