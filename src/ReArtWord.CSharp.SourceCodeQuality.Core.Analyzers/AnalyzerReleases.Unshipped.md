; 次のリリースで追加・変更・削除する診断規則。
; RAW501: タイトルとメッセージを、用途に応じた排他制御・借用方法の検討を促す表現に揃え、特定APIを必須としない案内と例示に統一します。
; RAW111: static classを検査対象から除外します。
; RAW442: 波括弧付きのelse ifを許可し、各分岐の本体で波括弧を省略した場合に検出します。
; RAW101・RAW130・RAW455: XMLコメント内の上限値を無視します。
; RAW301: XMLコメント内のRAW301AllowedNoWarnIdsを許可IDへ追加しません。
; 独自設定はXMLとして読み取り、XMLが不正なファイルやDTDを含むファイルを無視します。

; RAW461: string.Contains(string, StringComparison) を参照できない環境では、Containsを診断しません。
