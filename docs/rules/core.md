# Core のルール

[ドキュメント一覧](../README.md) · [導入する](../guides/installation.md) · [検査を設定する](../guides/configuration.md)

C#の設計、コメント、例外処理、書式、非同期処理、命名を検査します。
この一覧はv1.0.0の71ルールです。既定値が `Error` の規則はエラー、`Warning` は警告として報告します。
`Disabled` は明示的に有効化するまで検査しません。

## 適用範囲

表の「テストを除く」は、アセンブリ名が `.Tests` で終わるコード、またはパスに `tests` フォルダーを含むコードを対象外にする規則です。
「テストも含む」は、通常のコードとテストコードの両方に適用します。

生成コードと `obj` 配下は対象外です。`.g.cs`、`.g.i.cs`、`.designer.cs`、`.generated.cs`、`AssemblyInfo.cs` も除外します。
`RAW301` はC#ソースではなく、`AdditionalFiles` に登録した設定ファイルを検査します。

規則ごとに対象の.NETバージョンや構文上の条件があります。該当する条件は検査内容の欄に記載しています。

## クラス・データ型・メンバー

| ID | 既定 | 対象 | 検査内容 |
| --- | --- | --- | --- |
| RAW101 | Error | テストを除く | 1クラスの行数を制限。既定の上限は350行 |
| RAW102 | Error | テストを除く | class／structのフィールドをprivateに限定。.NET 6以降が対象。定数と構造体の配置互換が必要な宣言は除外 |
| RAW110 | Error | テストを除く | structの等値性を実装する場合、`Equals(object)`、`Equals(T)`、`GetHashCode()`、`==`、`!=` の実装を揃える |
| RAW111 | Error | テストを除く | 具体的な処理を持たないデータ型に `ToString()` の実装を要求 |
| RAW120A | Disabled | テストを除く | 定数を除くprivateフィールド名を `fieldName` 形式に限定 |
| RAW120B | Disabled | テストを除く | 定数を除くprivateフィールド名を `_fieldName` 形式に限定 |
| RAW120C | Error | テストを除く | 同じファイルで `RAW120A` と `RAW120B` が両方有効になっている設定を検出 |
| RAW130 | Error | テストを除く | 同じ型の手書きpartial class宣言数を制限。既定の上限は4個 |
| RAW140 | Error | テストを除く | IDisposable／IAsyncDisposable実装クラスの通常メソッドからの、明示的な `Dispose`／`DisposeAsync` 呼出しを検出。.NET 6以降が対象 |
| RAW141 | Error | テストを除く | IDisposable実装クラスの通常メソッドで、IDisposable型のローカル変数に `using` を要求。.NET 6以降が対象 |
| RAW150 | Error | テストを除く | トップレベルのclass／struct宣言を1ファイルに1つまでに制限。recordは別規則で扱う |
| RAW151 | Error | テストを除く | recordだけを宣言するファイルでは、トップレベルのrecordを1つまでに制限 |
| RAW152 | Error | テストを除く | class／structと同じファイルに宣言したrecordの、別ファイルからの参照を検出 |
| RAW160 | Warning | テストを除く | public asyncメソッドがEnumerable系の型を返す場合に、`IAsyncEnumerable` の利用を提案。.NET 6以降が対象 |
| RAW170 | Warning | テストを除く | コレクションのフィールドにreadonly、プロパティにget-onlyまたはinit-onlyを提案 |
| RAW171 | Warning | テストを除く | `DateTime` の使用を検出し、用途に合う日時型を提案。.NET 8以降が対象 |
| RAW172 | Warning | テストを除く | `DllImport` の使用を検出し、`LibraryImport` の利用を提案。.NET 10以降が対象 |

`RAW120A` と `RAW120B` は選択式です。[privateフィールドの命名方式](../guides/configuration.md#privateフィールドの命名方式を選ぶ)を参照してください。
クラスの行数とpartial宣言数は[独自設定](../guides/configuration.md#行数などの上限を変更する)で変更できます。

`RAW172` は、実際の `DllImportAttribute` を型情報で判定します。別名や完全修飾名も対象です。
`LibraryImportAttribute` を参照できないプロジェクトでは診断しません。
変更する場合は、partial宣言、unsafeの許可、文字列マーシャリング、呼出し規約などの互換性を確認してください。自動修正は提供していません。

## XMLコメント

この検査を使う場合は、[導入手順](../guides/installation.md#coreを追加する)に沿って `GenerateDocumentationFile` を有効にしてください。

| ID | 既定 | 対象 | 検査内容 |
| --- | --- | --- | --- |
| RAW201 | Error | テストも含む | 型・メソッドに `summary` を要求 |
| RAW202 | Error | テストも含む | `summary` 本文に日本語を要求 |
| RAW203 | Error | テストも含む | 戻り値があるメソッドに `returns` を要求 |

コメントには、呼出し側が知る必要のある役割や戻り値を記載してください。
これらの規則はコメントの有無や形式を確認するもので、説明の正確さまでは判定しません。

## 警告の抑制・作業コメント・例外処理

| ID | 既定 | 対象 | 検査内容 |
| --- | --- | --- | --- |
| RAW301 | Error | 設定ファイル | 許可していない診断IDの `NoWarn` への追加を検出 |
| RAW310 | Error | テストを除く | `SuppressMessage` に `Justification` を要求 |
| RAW311 | Error | テストを除く | `SuppressMessage` の直前に「抑制理由」を含むコメントを要求 |
| RAW320 | Error | テストも含む | `TODO`／`FIXME`／`FYI` の後に日本語の理由を要求 |
| RAW321 | Error | テストも含む | `NotImplementedException` の使用箇所に、対応内容を示す作業コメントを要求 |
| RAW322 | Error | テストも含む | `#if FALSE` などで無効化する箇所の直前に、`TODO`／`FIXME` コメントを要求 |
| RAW330 | Warning | テストも含む | catch内の `throw;` による再スローを検出 |
| RAW331 | Error | テストも含む | `Exception` の直接throwを検出 |
| RAW332 | Error | テストも含む | catchで受け取った変数を `throw ex;` の形式で再スローする処理を検出 |
| RAW333 | Error | テストも含む | catch内で式を伴うthrowを検出 |
| RAW334 | Error | テストも含む | `#pragma warning disable` による抑制を検出 |
| RAW340 | Error | テストも含む | null検証で例外を直接throwする代わりに、`ArgumentNullException.ThrowIfNull` を要求。.NET 6以降が対象 |
| RAW341 | Error | テストも含む | 利用可能な対象フレームワークで、`ArgumentException.ThrowIfNullOrEmpty`／`ThrowIfNullOrWhiteSpace` を要求 |
| RAW342 | Error | テストも含む | 利用可能な対象フレームワークで、`ArgumentOutOfRangeException.ThrowIf*` による引数検証を要求 |

同じコードに複数の規則が適用される場合があります。たとえば `throw ex;` は、例外変数の再スローとcatch内のthrowの両方に該当します。
必要な例外の扱いは[特定の宣言だけを例外にする](../guides/configuration.md#特定の宣言だけを例外にする)を参照してください。

## コメント・空白・式・制御構造

| ID | 既定 | 対象 | 検査内容 |
| --- | --- | --- | --- |
| RAW401 | Error | テストも含む | 通常のコメントの直前に空行を1行要求 |
| RAW402 | Error | テストも含む | ブロック開始直後のコメントの前に空行を置かない |
| RAW410 | Warning | テストを除く | `const` の使用を検出し、宣言方法の見直しを提案 |
| RAW411 | Warning | テストを除く | Task／ValueTaskをawaitする際に、`ConfigureAwait(...)` の明示を要求 |
| RAW420 | Error | テストも含む | `var` を、型が自明な場合や匿名型などに限定 |
| RAW421 | Error | テストも含む | プリミティブ型を推論する `var` を検出。boolと型付き静的メンバーによる初期化は除外 |
| RAW430 | Error | テストも含む | メンバー間と属性リストの空行を検査 |
| RAW431 | Error | テストも含む | 実行ブロック内の連続した空行を検出 |
| RAW440 | Error | テストも含む | `while (true)` を検出 |
| RAW441 | Error | テストも含む | for／foreach／whileの本体に波括弧を要求 |
| RAW442 | Error | テストも含む | if／elseの本体に波括弧を要求 |
| RAW443 | Error | テストも含む | for文の条件省略、または定数trueの条件を検出 |
| RAW444 | Error | テストも含む | if／else-if／whileの定数falseの条件を検出 |
| RAW450 | Error | テストも含む | else-ifが3つ以上続く場合に、switch文またはswitch式を要求 |
| RAW451 | Warning | テストも含む | `>`／`>=` を検出し、小さい値を左側に置く比較を提案 |
| RAW452 | Error | テストも含む | C#ブロックの開始波括弧を次の行に置く |
| RAW453 | Warning | テストも含む | 条件部に直接記述した文字列リテラル・補間文字列を検出 |
| RAW454 | Error | テストも含む | 空白に使われているタブ文字を検出 |
| RAW455 | Error | テストも含む | 三項演算子の条件数が閾値以上になると診断。既定の閾値は5 |
| RAW456 | Error | テストも含む | 三項演算子の条件式・`?`・真の場合の式を同じ行に置き、`?` と `:` の桁を揃える |
| RAW457 | Error | テストも含む | 同じ行のC#トークン間にある、2つ以上の連続スペースを検出 |
| RAW458 | Error | テストも含む | 代入・二項演算子・ラムダなどの前後に、同じ行ではスペースを1つ要求 |
| RAW459 | Error | テストも含む | カンマの直前はスペースなし、同じ行の次の要素との間はスペース1つとする |
| RAW460 | Error | テストも含む | nullとの `==`／`!=` 比較を検出し、パターンマッチングなどの使用を要求 |
| RAW461 | Error | テストも含む | 文字列比較に `StringComparison`、`StringComparer`、`CultureInfo` などの比較方法を要求 |

`RAW430` では、メソッド・コンストラクター・プロパティの終端と次のメンバーの間に空行を1行要求します。
連続する属性リストの間、および最後の属性と宣言の間には空行を置きません。コメントやプリプロセッサー指示の内部は空行として数えません。

## 非同期処理・命名

| ID | 既定 | 対象 | 検査内容 |
| --- | --- | --- | --- |
| RAW501 | Error | テストを除く | `lock` 文を検出。.NET 8以降が対象。.NET 9以降で対象の式が `System.Threading.Lock` 型の場合は、`EnterScope()` を `using` 文で使う書き方を案内 |
| RAW510 | Error | テストを除く | Task／ValueTaskを返すメソッドの最終引数に、既定値付きのCancellationTokenを要求 |
| RAW511 | Error | テストを除く | Task／ValueTaskの結果を破棄する処理、awaitしない単独呼出しを検出 |
| RAW601 | Error | テストを除く | asyncメソッド名に `Async` 接尾辞を要求 |
| RAW610 | Warning | テストを除く | unsafeを扱うメソッド名に `Unsafe` 接尾辞を提案 |
| RAW611 | Warning | テストを除く | asyncかつunsafeを扱うメソッド名に `UnsafeAsync` 接尾辞を提案 |
| RAW612 | Warning | テストを除く | 変数名のアンダースコアを検出。選択済みのprivateフィールド命名規則は優先 |
| RAW613 | Warning | テストを除く | メソッド名のアンダースコアを検出 |
| RAW614 | Warning | テストを除く | 引数名のアンダースコアを検出 |
| RAW615 | Warning | テストを除く | enum名に `E` 接頭辞を提案 |
| RAW616 | Error | テストを除く | interface名に `I` 接頭辞を要求 |
| RAW617 | Error | テストを除く | class／struct／interface／record／enum名をPascalCaseに限定 |

`RAW501` は、`System.Threading.Lock` 型を使った `lock` 文も検出します。
.NET 9以降で `System.Threading.Lock` を使う場合は、`lock (gate)` の代わりに次のように記述してください。
`gate` は `System.Threading.Lock` 型のインスタンスです。

```csharp
using (gate.EnterScope())
{
    count++;
}
```

`EnterScope()` が返すスコープは、`using` 文の終了時にロックを解放します。
非同期処理で排他制御が必要な場合は、`SemaphoreSlim` など用途に合う同期機構を使用してください。

## リリース情報

診断ID・カテゴリ・既定値のリリース記録は、[Core の Shipped](../../src/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers/AnalyzerReleases.Shipped.md)にあります。
設定方法は[検査を設定する](../guides/configuration.md)、実装を変更する場合は[ルールを開発する](../development/rules.md)を参照してください。
