# ソース構成

[ドキュメント一覧](../README.md) · [開発環境と検証](build.md) · [ルールを開発する](rules.md)

## プロジェクトの役割

プロジェクト名の共通部分は `ReArtWord.CSharp.SourceCodeQuality` です。

| 配置 | プロジェクト | 役割 |
| --- | --- | --- |
| `src` | `Core.Analyzers` | C#共通の設計・命名・コメント・非同期処理などを検査 |
| `src` | `MVVM.Analyzers` | CommunityToolkit.Mvvmを使うViewModelを検査 |
| `src` | `Razor.Analyzers` | Razorの処理配置と取消トークンを検査 |
| `tests` | `Core.Analyzers.Tests` | Coreの診断と設定・適用範囲を検証 |
| `tests` | `MVVM.Analyzers.Tests` | MVVMの診断と型・属性の判定を検証 |
| `tests` | `Razor.Analyzers.Tests` | Razorの行対応情報とコンポーネントの呼出しを検証 |

MVVMとRazorのビルドでは、Coreをアナライザーとして参照します。
配布パッケージには各アナライザー自身のDLLを格納します。利用先へCoreを適用するには、Coreも明示的に参照してください。

## アナライザー内部

| 配置 | 担当する処理 |
| --- | --- |
| `Application` | Roslynの構文・意味情報を使う解析、対象の選別、設定の読み取り |
| `Contracts/Diagnostics` | 診断ID、メッセージ、カテゴリ、既定の重大度 |
| `Contracts/Configuration` | Coreの独自設定名 |
| `Domain` | Roslynの解析コンテキストを必要としない判定 |

Coreの入口は `Application/Adapters` にあります。
MVVMとRazorは、入口から規則別の処理を呼び出す構成です。
診断定義ではRoslynの `DiagnosticDescriptor` を使用します。構文や意味情報の探索は解析処理側で行います。

同じ対象判定を複数の規則で使う場合は、責務に合う既存の補助クラスへまとめてください。
たとえばprivateフィールドの適用範囲と有効な命名設定は、`PrivateFieldNamingPolicy` が共有しています。

## 共通設定とパッケージ

| ファイル | 管理する内容 |
| --- | --- |
| `ReArtWord.CSharp.SourceCodeQuality.Analyzers.slnx` | 6プロジェクトのソリューション構成 |
| `Directory.Build.props` | ビルド・品質検査・バージョン・ライセンス・共通参照 |
| `Directory.Packages.props` | NuGet依存パッケージのバージョン |
| `.editorconfig` | このリポジトリの書式と診断設定 |
| 各 `AnalyzerReleases.Shipped.md` | 公開版に含める診断ID・カテゴリ・既定値 |
| 各 `AnalyzerReleases.Unshipped.md` | 次の版で追加・変更・削除する診断 |
| 各 `README.md` | NuGetパッケージに同梱する概要と導入案内 |

## テストと生成コード

Coreの規則には、テストも含めて検査するものと、テストを除外するものがあります。
Core自身の解析テストは、テスト用のソース例を検査対象と混同しないように除外しています。
通常の.NETアナライザーは、テストプロジェクトでも使用します。

MVVMは生成コードを除外します。
Razorは `.razor` 内の利用者コードを生成C#から調べるため、生成コードの解析を有効にし、元のファイルとの行対応情報で区別します。
対象範囲を変更する場合は、[各ルールの説明](../README.md#利用する)と回帰テストも更新してください。
