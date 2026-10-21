# 開発環境と検証

[ドキュメント一覧](../README.md) · [ソース構成](architecture.md) · [ルールを開発する](rules.md) · [リリースする](releases.md)

以下のコマンドは、リポジトリのルートで実行してください。
ソリューションは `ReArtWord.CSharp.SourceCodeQuality.Analyzers.slnx` です。

## 必要なツール

| ツール | 用途 |
| --- | --- |
| .NET 10 SDK | C#のビルド、テスト、NuGetパッケージ生成 |
| Git | ソース管理 |

アナライザー本体は `netstandard2.0`、テストは `net10.0` を対象とします。
v1.0.1の検証には.NET SDK 10.0.400を使用しています。
依存パッケージのバージョンは `Directory.Packages.props` で管理します。

## ビルドする

```powershell
dotnet restore ReArtWord.CSharp.SourceCodeQuality.Analyzers.slnx
dotnet build ReArtWord.CSharp.SourceCodeQuality.Analyzers.slnx -c Release --no-restore -warnaserror
```

MVVMとRazorの実装には、同じソリューションのCoreアナライザーを適用します。
標準の.NETアナライザーと、Roslynアナライザー開発用の検査も有効です。
警告を無効にしてビルドの成功条件を変えるのではなく、診断内容に応じて実装を修正してください。

## テストする

```powershell
dotnet test ReArtWord.CSharp.SourceCodeQuality.Analyzers.slnx -c Release --no-build --no-restore
```

規則を修正している間は、対応する診断IDでテストを絞れます。

```powershell
dotnet test tests/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.csproj -c Release --filter "FullyQualifiedName~RAW120"
```

テストは、C#のコンパイルへアナライザーを適用し、診断ID・位置・対象外のコードを確認します。
設定や複数規則の関係を変更した場合は、それらを同時に適用するテストも実行してください。

## 実際の利用先で確認する

アナライザーの読み込みやNuGetの構成を変更した場合は、[生成したパッケージ](releases.md#配布パッケージを生成する)を別のプロジェクトへ追加して確認してください。

1. 違反する最小のコードで、対象の診断が出ることを確認してください。
2. コードを修正し、その診断が解消することを確認してください。
3. 設定を変える機能では、`.editorconfig` や `AdditionalFiles` を使う実際のビルドでも確認してください。
4. Razorでは、手書きのC#によるテストに加え、Razor SDKが生成するコードで確認してください。

ローカルフィードを使う場合は、利用先のNuGet設定に、生成した `artifacts/packages` フォルダーを登録してください。
一回の復元だけで指定する場合は、`dotnet restore` の `--source` にそのフォルダーのパスを渡せます。
CommunityToolkit.Mvvmなども復元する場合は、通常使用するフィードも `--source` で併せて指定してください。
パッケージを同じバージョンで作り直して検証する場合は、検証プロジェクト専用の `RestorePackagesPath` を指定すると、以前のキャッシュと区別できます。

## 検証結果を残す

実行コマンド、使用したSDK、成功・失敗、未確認の範囲を変更の報告に記載してください。
必要なビルドログは `build-logs/` に保存します。
作業日誌や移行経緯の文書を追加する必要はありません。
