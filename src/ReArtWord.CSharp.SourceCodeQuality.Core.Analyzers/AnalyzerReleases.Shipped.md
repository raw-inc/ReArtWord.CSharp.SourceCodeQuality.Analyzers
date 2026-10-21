## Release 1.0.1

; RAW501: タイトルとメッセージを、用途に応じた排他制御・借用方法の検討を促す表現に揃え、特定APIを必須としない案内と例示に統一します。
; RAW111: static classを検査対象から除外します。
; RAW442: 波括弧付きのelse ifを許可し、各分岐の本体で波括弧を省略した場合に検出します。
; RAW101・RAW130・RAW455: XMLコメント内の上限値を無視します。
; RAW301: XMLコメント内のRAW301AllowedNoWarnIdsを許可IDへ追加しません。
; 独自設定はXMLとして読み取り、XMLが不正なファイルやDTDを含むファイルを無視します。
; RAW461: string.Contains(string, StringComparison) を参照できない環境では、Containsを診断しません。

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------

## Release 1.0.0

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------
RAW101 | 設計 | Error | 本番C#クラスを350行以下に制限します。
RAW102 | 設計 | Error | .NET6以上でclass/structのフィールドを原則privateに制限します。
RAW110 | 設計 | Error | structで等値性メンバーを一部実装した場合は必須セット全実装を要求します。
RAW111 | 設計 | Error | 具体処理を持たないデータ型でToStringオーバーライドを必須化します。
RAW120A | 設計 | Disabled | privateフィールドをアンダースコア無しlowerCamelCaseへ統一します。
RAW120B | 設計 | Disabled | privateフィールドをアンダースコア付きlowerCamelCaseへ統一します。
RAW120C | 設計 | Error | RAW120AとRAW120Bの同時指定を検出します。
RAW130 | 設計 | Error | 手動実装ソースのpartial classを型ごと既定4宣言までに制限します。
RAW140 | 設計 | Error | IDisposable/IAsyncDisposable実装クラスの通常メソッドで明示Dispose呼び出しを禁止します。
RAW141 | 設計 | Error | IDisposable/IAsyncDisposable実装クラスの通常メソッドでIDisposableローカル宣言へusingを必須化します。
RAW150 | 設計 | Error | 1ファイル内のトップレベルclass/struct（record除く）を1つまでに制限します。
RAW151 | 設計 | Error | record-onlyファイルでトップレベルrecordを1つまでに制限します。
RAW152 | 設計 | Error | class/structと同居するrecordのファイル外参照を禁止します。
RAW160 | 設計 | Warning | .NET6以上でpublic asyncかつEnumerable系戻り値はIAsyncEnumerable利用を推奨します。
RAW170 | 設計 | Warning | コレクションフィールドとプロパティのreadonlyまたは読み取り専用化を推奨します。
RAW171 | 設計 | Warning | .NET8以上でDateTime型利用を避けDateTimeOffset等を推奨します。
RAW172 | 設計 | Warning | .NET10以降でDllImport利用を避けLibraryImportへの移行検討を推奨します。
RAW201 | ドキュメント | Error | 本番C#の全型・全メソッドへsummaryを必須化します。
RAW202 | ドキュメント | Error | summary本文に日本語を必須化します。
RAW203 | ドキュメント | Error | 戻り値のあるメソッドへreturnsコメントを必須化します。
RAW301 | 運用 | Error | NoWarnへ未許可ID追加を禁止します。
RAW310 | 運用 | Error | SuppressMessageのJustification必須を強制します。
RAW311 | 運用 | Error | SuppressMessage直前の抑制理由コメント必須を強制します。
RAW320 | 運用 | Error | TODO/FIXME/FYIコメントはPrefix後に日本語理由を必須化します。
RAW321 | 運用 | Error | NotImplementedExceptionの利用箇所へTODO/FIXME/FYIコメントを必須化します。
RAW322 | 運用 | Error | #if FALSE系プリプロセッサ条件の直前にTODO/FIXMEコメントを必須化します。
RAW330 | 運用 | Warning | catch内のthrow;再スローは特別な理由がある場合に限定します。
RAW331 | 運用 | Error | 汎用Exceptionの直接throwを禁止します。
RAW332 | 運用 | Error | catch変数をthrow ex形式で再スローすることを禁止します。
RAW333 | 運用 | Error | catchブロック内の式付きthrowを禁止します。
RAW334 | 運用 | Error | #pragma warning disableによる警告抑制を禁止します。
RAW340 | 運用 | Error | .NET6以上でArgumentNullException.ThrowIfNull利用を必須化します。
RAW341 | 運用 | Error | 対象TFMでArgumentException.ThrowIfNullOrEmpty/ThrowIfNullOrWhiteSpace利用を必須化します。
RAW342 | 運用 | Error | 対象TFMでArgumentOutOfRangeException.ThrowIf系メソッド利用を必須化します。
RAW401 | 可読性 | Error | インデント増加直後を除きコメント直前の1行空行を必須化します。
RAW402 | 可読性 | Error | インデント増加直後のコメント直前空行を禁止します。
RAW410 | 可読性 | Warning | const利用を抑制し、将来変更を見据えた宣言を推奨します。
RAW411 | 可読性 | Warning | Task/ValueTaskをawaitする際のConfigureAwait明示を推奨します。
RAW420 | 可読性 | Error | varは型が自明または匿名型（LINQ匿名型含む）の場合のみ許可します。
RAW421 | 可読性 | Error | varのプリミティブ型利用をbool除き禁止し、型付き静的メンバー例外のみ許可します。
RAW430 | 可読性 | Error | クラス内メンバー終端の'}'と次要素の間を空行1行に固定し、明示対象宣言のAttribute list境界を空行0行に固定します。
RAW431 | 可読性 | Error | 実行ブロック内で空行が2つ以上連続することを禁止します。
RAW440 | 可読性 | Error | while(true)の使用を禁止します。
RAW441 | 可読性 | Error | for/foreach/while本体の波括弧省略を禁止します。
RAW442 | 可読性 | Error | if/else本体の波括弧省略を禁止します。
RAW443 | 可読性 | Error | for文の条件省略または定数true条件を禁止します。
RAW444 | 可読性 | Error | if/else-if/whileの定数false条件を禁止し、#if FALSEなどの利用を要求します。
RAW450 | 可読性 | Error | else ifが3連鎖以上の場合にswitch文またはswitch式を必須化します。
RAW451 | 可読性 | Warning | 大なり・大なりイコールを原則禁止し、左ほど小さい方向へ統一して直感性を保ち認知負荷を下げます。
RAW452 | 可読性 | Error | C#ブロックの開き波括弧を前の構文と同じ行に置く書き方を禁止します。
RAW453 | 可読性 | Warning | 条件部にあるstringリテラルと補間文字列の直接記述を警告します。
RAW454 | 可読性 | Error | 空白として使われるTabを禁止しSpaceへ統一します。
RAW455 | 可読性 | Error | 三項演算子の条件数が設定閾値以上の場合に禁止します。
RAW456 | 可読性 | Error | 三項演算子の条件式・疑問符・真側式同一行化と疑問符・コロンの縦整列を必須化します。
RAW457 | 可読性 | Error | 同一行のC#トークン間でSpaceを2つ以上連続させることを禁止します。
RAW458 | 可読性 | Error | 代入、二項演算子、ラムダ/矢印の同一行側にSpaceを1つ必須化します。
RAW459 | 可読性 | Error | カンマ区切りを直前Spaceなし、同一行次要素前Space1つの形式へ統一します。
RAW460 | 可読性 | Error | nullとの==または!=比較を禁止します。
RAW461 | 可読性 | Error | 文字列比較で比較方法の明示を必須化します。
RAW501 | 並行性 | Error | .NET8以上でlock文を禁止し、モダンな同期機構への移行を促します。
RAW510 | 並行性 | Error | Task/ValueTaskメソッドの最終引数にCancellationTokenとdefault指定を必須化します。
RAW511 | 並行性 | Error | Task/ValueTaskのfire-and-forget（discard/未await単独呼び出し）を禁止します。
RAW601 | 命名 | Error | async修飾子付きメソッドにAsync接尾辞を必須化します。
RAW610 | 命名 | Warning | unsafe対象メソッドへUnsafe接尾辞を推奨します。
RAW611 | 命名 | Warning | asyncかつunsafe対象メソッドへUnsafeAsync接尾辞を推奨します。
RAW612 | 命名 | Warning | 変数名のアンダースコア利用を警告します。専用の命名規則を有効にしたprivateフィールドは、その規則で検査します。
RAW613 | 命名 | Warning | メソッド名のアンダースコア利用を警告します。
RAW614 | 命名 | Warning | 引数名のアンダースコア利用を警告します。
RAW615 | 命名 | Warning | enum名へEプレフィックス付与を推奨します。
RAW616 | 命名 | Error | interface名へIプレフィックスを必須化します。
RAW617 | 命名 | Error | 型名をUpperCamelCase(PascalCase)へ統一します。
