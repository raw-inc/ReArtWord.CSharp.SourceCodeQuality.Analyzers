# ReArtWord.CSharp.SourceCodeQuality.Analyzers

C#の設計、命名、コメント、例外処理、非同期処理などを、IDEとビルド時に検査するRoslynアナライザーです。
共通のC#規則に加え、CommunityToolkit.Mvvmを使うViewModelと、Razorコンポーネントの規則を提供します。
診断ごとに重大度や有効・無効を指定し、プロジェクトの実装方針に合わせて使用できます。

## 使用するアナライザーを選ぶ

| アナライザー | 検査する内容 |
| --- | --- |
| [Core](docs/rules/core.md) | クラス・メンバーの構成、命名、XMLコメント、例外処理、書式、非同期処理 |
| [MVVM](docs/rules/mvvm.md) | 変更通知、コマンド、ViewModelと画面の依存関係 |
| [Razor](docs/rules/razor.md) | 処理と表示の分離、画面から開始する処理への取消トークンの伝搬 |

Coreを基本として、必要なプロジェクトにMVVMまたはRazorを追加してください。
各パッケージ名は `ReArtWord.CSharp.SourceCodeQuality.<種類>.Analyzers` です。

## 使い始める

1. [導入する](docs/guides/installation.md)を参照し、NuGetパッケージ、ソース参照、DLL参照から導入方法を選んでください。
2. [検査を設定する](docs/guides/configuration.md)を参照し、採用する規則、重大度、命名方式、上限値を指定してください。
3. 利用先プロジェクトでビルドし、診断内容を確認してください。

```powershell
dotnet restore
dotnet build -warnaserror
```

Coreには、日本語のXMLコメントや独自の書式・命名規則も含まれます。
既定値と適用範囲は各ルール一覧に記載しています。privateフィールドの命名方式は選択式で、既定では無効です。

v1.0.1は.NET SDK 10.0.400で検証しています。利用条件と参照方法の詳細は[導入手順](docs/guides/installation.md)を参照してください。

## 目的から探す

| やりたいこと | 読むページ |
| --- | --- |
| 文書の全体像を確認する | [ドキュメント一覧](docs/README.md) |
| 診断IDから規則を調べる | [Core](docs/rules/core.md)・[MVVM](docs/rules/mvvm.md)・[Razor](docs/rules/razor.md) |
| 診断の重大度や適用範囲を調整する | [検査を設定する](docs/guides/configuration.md) |
| 読み込まれない、設定が効かない原因を調べる | [困ったとき](docs/guides/troubleshooting.md) |
| ソースからビルドしてテストする | [開発環境と検証](docs/development/build.md) |
| 規則を追加・修正する | [ソース構成](docs/development/architecture.md)・[ルールを開発する](docs/development/rules.md) |
| 配布パッケージを作る | [リリースする](docs/development/releases.md) |
| 不具合や改善案を報告する | [問い合わせ・不具合報告](SUPPORT.md) |

## 開発に参加する

ソリューションは **ReArtWord.CSharp.SourceCodeQuality.Analyzers.slnx** です。
変更の提案方法は [CONTRIBUTING](CONTRIBUTING.md)、参加時の行動指針は [CODE_OF_CONDUCT](CODE_OF_CONDUCT.md) を参照してください。
セキュリティに関する報告は [SECURITY](SECURITY.md) に記載しています。

## ライセンス

本ソフトウェアは **LGPL-3.0-or-later** で提供します。

Copyright (C) 2026 raw inc. and Tomonari Hayashi

適用条件は [NOTICE](NOTICE)、ライセンス本文は [LICENSE](LICENSE) と [GPL v3 本文](licenses/GPL-3.0.txt)を参照してください。
依存ライブラリの表記は [Third-party notices](THIRD_PARTY_NOTICES.md) にまとめています。
