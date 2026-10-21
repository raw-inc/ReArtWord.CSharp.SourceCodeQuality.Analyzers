# 検査を設定する

[ドキュメント一覧](../README.md) · [導入する](installation.md) · [困ったとき](troubleshooting.md)

診断の有効・無効と重大度は `.editorconfig`、行数上限などの独自設定は `.props` または `.csproj` に記述します。

| 設定する内容 | 記述する場所 |
| --- | --- |
| 診断の有効・無効、エラー・警告の指定 | `.editorconfig` |
| privateフィールドの命名方式 | `.editorconfig` の `RAW120A`／`RAW120B` |
| XMLコメントをコンパイラーに解析させる設定 | `.props`／`.csproj` の `GenerateDocumentationFile` |
| クラスの行数、partial宣言数、三項演算子の条件数 | `Directory.Build.props` または `.csproj` |
| `NoWarn` に指定できる診断ID | `Directory.Build.props` または `.csproj` |
| 特定の宣言でのみ必要な例外 | 理由を記載した `SuppressMessage` |

## 診断の重大度を指定する

利用先リポジトリの `.editorconfig` に記述してください。

```editorconfig
root = true

[*.cs]
dotnet_diagnostic.RAW101.severity = error
dotnet_diagnostic.RAW451.severity = warning
dotnet_diagnostic.RAW120A.severity = none
dotnet_diagnostic.RAW120B.severity = none
```

| 値 | 動作 |
| --- | --- |
| `error` | エラーとして報告 |
| `warning` | 警告として報告。`-warnaserror` 使用時はビルドエラー |
| `suggestion` | IDEで提案として表示 |
| `silent` | 非表示の診断として扱う |
| `none` | 無効にする |
| `default` | アナライザーが定める既定値に戻す |

既定値は[ルール一覧](../README.md#利用する)の各ページに記載しています。
フォルダーやファイルを限定したい場合は、`.editorconfig` のセクションを分けてください。
既存の `.editorconfig` に追記する場合は、すでにある設定との重複を確認してください。

## privateフィールドの命名方式を選ぶ

`RAW120A` は `fieldName`、`RAW120B` は `_fieldName` を要求します。どちらも既定では無効です。
使用する場合は一方を有効にしてください。次は `_fieldName` を採用する例です。

```editorconfig
[*.cs]
dotnet_diagnostic.RAW120A.severity = none
dotnet_diagnostic.RAW120B.severity = error
```

対象は定数を除くprivateフィールドです。アクセス修飾子を省略したprivateフィールドや、static・readonlyフィールドも含みます。
両方を有効にすると、`RAW120C` が設定の競合を報告します。この場合は名前を変更せず、一方の規則を `none` にしてください。

専用規則を有効にしたprivateフィールドには、一般のアンダースコア禁止規則 `RAW612` を重ねて適用しません。
たとえば `_count` は `RAW120B` に適合し、`_count_value` は `RAW120B` で検出します。
ローカル変数、private以外のフィールド、定数には `RAW612` を適用します。
専用規則をどちらも無効にした場合も、`RAW612` 自体は有効です。

## 行数などの上限を変更する

利用先の `Directory.Build.props` に、値と読み取り対象ファイルを指定してください。
既存ファイルがある場合は、必要な要素をそのファイルへ追加してください。

```xml
<Project>
  <PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <RAW101MaxClassLineCount>350</RAW101MaxClassLineCount>
    <RAW130MaxPartialClassDeclarationCount>4</RAW130MaxPartialClassDeclarationCount>
    <RAW455TernaryConditionCountThreshold>5</RAW455TernaryConditionCountThreshold>
    <RAW301AllowedNoWarnIds>CS1591;CA1707</RAW301AllowedNoWarnIds>
  </PropertyGroup>
  <ItemGroup>
    <AdditionalFiles Include="$(MSBuildThisFileDirectory)Directory.Build.props" />
    <AdditionalFiles Include="$(MSBuildProjectFullPath)" />
  </ItemGroup>
</Project>
```

| 設定名 | 既定値 | 意味 |
| --- | --- | --- |
| `RAW101MaxClassLineCount` | `350` | 1クラスに許容する行数 |
| `RAW130MaxPartialClassDeclarationCount` | `4` | 同じ型の手書きpartial class宣言数の上限 |
| `RAW455TernaryConditionCountThreshold` | `5` | 三項演算子の条件数がこの値以上になると診断 |
| `RAW301AllowedNoWarnIds` | `CS1591;CA1707` | `NoWarn` に指定できる診断ID。セミコロン区切り |

独自設定は、`AdditionalFiles` に登録した `.props` と `.csproj` のプロパティ要素から読み取ります。
設定ファイルにはDTDを含まない正しいXMLを使用してください。
共通の `.props` とプロジェクト別の `.csproj` の両方に有効な数値がある場合は、`.csproj` の値を優先します。
`.csproj` が不正な値なら `.props`、そこにも有効な値がなければ既定値を使います。数値には正の整数を指定してください。

MSBuildの条件式やプロパティ参照は評価せず、ファイルに書かれた値を読み取ります。
設定値は直接記述し、同じ設定を複数の条件付き `PropertyGroup` に分けないでください。
`RAW301AllowedNoWarnIds` は、既定の許可IDと各ファイルに記述したIDを合わせて使用します。

## 特定の宣言だけを例外にする

規則が適用できない事情がある場合は、`SuppressMessage` に理由を記載してください。
次は、外部仕様に合わせて定数を維持する例です。

```csharp
// 抑制理由: 外部仕様で値が固定されている識別子です。
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "可読性",
    "RAW410",
    Justification = "外部仕様で値が固定されている識別子です。")]
private const int ProtocolVersion = 1;
```

Coreは `Justification` を `RAW310`、直前の理由コメントを `RAW311` で検査します。
`#pragma warning disable` は `RAW334`、許可していない `NoWarn` の追加は `RAW301` の対象です。
プロジェクト全体で採用しない規則は、個別の例外を増やす前に `.editorconfig` で設定してください。
