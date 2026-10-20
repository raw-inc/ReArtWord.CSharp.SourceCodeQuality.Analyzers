# 問い合わせ・不具合報告

[README](README.md) · [ドキュメント一覧](docs/README.md)

利用方法の確認には、まず[導入手順](docs/guides/installation.md)、[設定方法](docs/guides/configuration.md)、[困ったとき](docs/guides/troubleshooting.md)を参照してください。
解消しない場合や改善案がある場合は、[GitHub Issues](https://github.com/raw-inc/ReArtWord.CSharp.SourceCodeQuality.Analyzers/issues)へ投稿してください。

## 不具合を報告する

次の情報を添えてください。

- 対象パッケージとバージョン。
- 診断IDとメッセージ全文。
- OS、.NET SDK、使用しているIDEとそのバージョン。
- 最小の再現コードと、関係する `.editorconfig`・プロジェクト設定。
- 期待した結果と、実際の結果。
- 実行したコマンド。IDEだけで起きる場合は、その操作手順。

再現プロジェクトから、認証情報、個人情報、公開できないソースを除いてください。
誤検出の場合は「許可したいコード」、検出漏れの場合は「診断してほしいコード」があると確認しやすくなります。

## 改善を提案する

追加したい規則や文書の変更は、利用場面と具体例を添えてください。
新しい規則では、検出する例と、似ていても許可したい例の両方があると、適用範囲を検討できます。

セキュリティに関する報告は [SECURITY](SECURITY.md) を参照してください。
