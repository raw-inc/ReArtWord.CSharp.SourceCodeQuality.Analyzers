# リリースする

[ドキュメント一覧](../README.md) · [開発環境と検証](build.md) · [ルールを開発する](rules.md)

Core、MVVM、Razorは、`Directory.Build.props` の `Version` を共通のパッケージバージョンとして使用します。
v1.0.0のルールは、各プロジェクトの `AnalyzerReleases.Shipped.md` に記録しています。

## 診断のリリース情報を更新する

開発中の追加・変更・削除は、該当プロジェクトの `AnalyzerReleases.Unshipped.md` に記載してください。

| 変更 | 記載する節 |
| --- | --- |
| 新しい診断IDを追加 | `New Rules` |
| 公開済み診断のカテゴリ・既定の重大度・既定の有効状態を変更 | `Changed Rules` |
| 公開済み診断を削除 | `Removed Rules` |

リリースを確定するときは、次の順で整理してください。

1. `Directory.Build.props` の `Version` をリリースする版へ揃えてください。
2. 各 `Shipped` に `## Release x.y.z` を追加してください。
3. `Unshipped` の各節とエントリを、その版の節へ移してください。
4. `Unshipped` を次版用の空の状態にしてください。
5. ビルドし、診断定義とリリース情報の整合性を確認してください。

既定で無効な規則は `Disabled` として記録します。`Shipped` への移動によって規則を有効化しないでください。
公開済みの版の記録は保持し、次版の変更を過去の節へ書き足さないでください。

## 配布パッケージを生成する

リポジトリのルートで実行してください。

```powershell
dotnet restore ReArtWord.CSharp.SourceCodeQuality.Analyzers.slnx
dotnet build ReArtWord.CSharp.SourceCodeQuality.Analyzers.slnx -c Release --no-restore -warnaserror
dotnet test ReArtWord.CSharp.SourceCodeQuality.Analyzers.slnx -c Release --no-build --no-restore
dotnet pack ReArtWord.CSharp.SourceCodeQuality.Analyzers.slnx -c Release --no-build --no-restore -warnaserror --output artifacts/packages
```

`artifacts/packages` に、Core・MVVM・Razorそれぞれの `.nupkg` が生成されます。
生成後は、次の内容を確認してください。

| 項目 | 内容 |
| --- | --- |
| アナライザー本体 | `analyzers/dotnet/cs/` に、そのパッケージのDLL |
| パッケージ説明 | `README.md` |
| 本体ライセンス | `LICENSE`、`licenses/GPL-3.0.txt`、`NOTICE` |
| 依存ライブラリの表記 | `THIRD_PARTY_NOTICES.md` |
| メタデータ | パッケージID、バージョン、著作者、ライセンス、リポジトリURL |

アナライザーのDLLを通常のライブラリ用の `lib/` に置かないでください。
RoslynのDLLはコンパイラー側が提供するため、パッケージへ同梱しません。
MVVM Toolkitは利用先が選択する依存パッケージであり、MVVMアナライザーには同梱しません。

## 公開前に確認する

生成したパッケージを別のプロジェクトへ参照させ、違反するコードと修正後のコードで診断を確認してください。
Coreの独自設定、MVVM Toolkitの参照、Razor SDKによる生成コードも、それぞれの実際の利用方法で確認します。

NuGetフィードへの送信、Gitタグ、GitHub Releaseの作成は、上記の生成・検証とは別の操作です。
公開する版と、検証したコミット・パッケージが一致していることを確認して実施してください。
