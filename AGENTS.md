# 作業方針

## このリポジトリ

ReArtWord.CSharp.SourceCodeQuality.Analyzersは、Core・MVVM・Razorの3種類のRoslynアナライザーを提供します。
ソリューションは `ReArtWord.CSharp.SourceCodeQuality.Analyzers.slnx` のみを使用します。
文書の入口は `README.md` と `docs/README.md` です。

## 作業の進め方

1. 作業ツリーと利用者の変更を確認する。
2. 対象のルール、設定、実装、テストを読む。構成は `docs/development/architecture.md`、検証手順は `docs/development/build.md` を参照する。
3. 実装前に、変更していない状態で `dotnet build ReArtWord.CSharp.SourceCodeQuality.Analyzers.slnx -warnaserror` を確認する。
4. 変更に対応するテストを用意し、意味のある編集の区切りでwarnings-as-errorsのビルドを行う。
5. 関連するテストと、必要に応じた利用先での検証を行う。
6. 現行の規則、設定、リリース情報、ライセンス表記を必要な範囲で更新する。
7. 実行コマンド・結果・未確認範囲を報告する。必要な検証ログは `build-logs/` に保存する。

SubAgentや別セッションは、利用者の明示指示または明確な意図がある場合に使用する。
共同作業では担当範囲を定め、他の担当者の変更を取り消さない。
コミットメッセージは既存履歴の規則を確認して記述する。

## 実装

- YAGNIとSOLID、とくに単一責任を守る。必要な動作を省略せず、仮想の拡張のためだけの抽象化を増やさない。
- 可読性、保守性、実際の処理負荷のバランスを取る。
- 解析処理は `Application`、診断・設定の契約は `Contracts`、Roslynに依存しない判定は `Domain` の既存構成に合わせる。
- Core以外のアナライザー本体には、Coreをアナライザー参照として適用する。
- JSONには `System.Text.Json`、日時には `DateTimeOffset` を使用する。
- 1クラス350行以内、日本語のXMLコメントを維持する。
- `NoWarn` へ新しい抑制IDを追加しない。
- 抑制が必要な場合は `SuppressMessage` を使い、直前に「抑制理由」を含むコメント、属性に `Justification` を記載する。
- テストは観測できる診断と境界を検証する。ソース文字列の照合や、テストを呼ぶだけの入口を検証の代わりにしない。

## 文書とリリース

- 実装・文書・XMLコメントは自然な日本語で記述する。操作手順と仕様説明の視点を揃える。
- 文書には現在の使い方、設定、検査内容、必要な制約を記載する。移行経緯、旧名称の説明、作業日誌を追加しない。
- 文書の重複を避け、詳細なルール一覧は `docs/rules`、設定方法は `docs/guides/configuration.md` にまとめる。
- 公開済みの診断記録は `AnalyzerReleases.Shipped.md` に保持し、次版の変更は `AnalyzerReleases.Unshipped.md` に記載する。
- パッケージの生成・検証と、NuGetやGitHub Releaseへの公開を区別する。
- ライセンスや依存関係を変更した場合は `NOTICE`、`THIRD_PARTY_NOTICES.md`、パッケージ内容を確認する。
