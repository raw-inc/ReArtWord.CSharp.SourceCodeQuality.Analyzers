# Razor のルール

[ドキュメント一覧](../README.md) · [導入する](../guides/installation.md) · [検査を設定する](../guides/configuration.md)

Razorコンポーネントの処理配置と、取消可能な呼出しへの `CancellationToken` の伝搬を検査します。
この一覧はv1.0.1の2ルールです。Coreと併用してください。

| ID | 既定 | 検査内容 |
| --- | --- | --- |
| RAWRAZOR001 | Warning | `.razor` 内のメンバー宣言を検出し、コードビハインドへの分離を案内 |
| RAWRAZOR002 | Warning | ComponentBase派生型の取消可能な呼出しで、トークンの省略・default・CancellationToken.Noneを検出 |

## 処理と状態をコードビハインドへ置く

`.razor` には表示式、繰り返し、条件表示、イベントの接続を記述し、状態や処理を対応する `.razor.cs` に置いてください。

`RAWRAZOR001` の対象は、メソッド、フィールド、プロパティ、イベント、コンストラクター、入れ子のclass／struct／record宣言です。
診断は移動対象の識別子に出します。複数のフィールドをまとめて宣言した場合は、先頭の識別子を示します。

次は、状態とクリック処理をコードビハインドへ分けた例です。

`Counter.razor`:

```razor
@namespace Example.Components
@using Microsoft.AspNetCore.Components.Web
<button @onclick="Increment">@count</button>
```

`Counter.razor.cs`:

```csharp
namespace Example.Components;

/// <summary>クリック回数を表示します。</summary>
public partial class Counter
{
    private int count;

    /// <summary>表示する回数を増やします。</summary>
    private void Increment()
    {
        count++;
    }
}
```

通常のコードビハインド、描画式中の文字列、`@inject` が生成するプロパティは対象外です。
Razor SDKが生成するC#の行対応情報を使うため、Razorのコンパイルに組み込んで使用してください。

## 画面の終了に合わせて処理を取り消す

`RAWRAZOR002` は、`CancellationToken` 引数を受け取るメソッドの呼出しを検査します。
画面が所有するトークンなど、処理の存続期間に合うトークンを明示してください。

| 呼出し方 | 判定 |
| --- | --- |
| 省略可能なCancellationToken引数を省略 | 診断する |
| `default`／`default(CancellationToken)` を指定 | 診断する |
| `CancellationToken.None` を指定 | 診断する |
| トークンを明示して渡す | この規則では診断しない |

ComponentBaseの間接継承や型の別名も、型情報で判定します。
コンポーネント以外の型と、`Dispose`／`DisposeAsync` 内の解放処理は対象外です。

この規則は、トークンを受け取らない別のオーバーロードの選択や、変数に保存したトークンの値までは追跡しません。
明示したトークンの所有者、取消しのタイミング、処理の完了待ちについては、実装とテストで確認してください。

## リリース情報

診断ID・カテゴリ・既定値のリリース記録は、[Razor の Shipped](../../src/ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers/AnalyzerReleases.Shipped.md)にあります。
