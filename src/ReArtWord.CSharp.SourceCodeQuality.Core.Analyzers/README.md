# ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers

C#の設計、命名、XMLコメント、例外処理、書式、非同期処理を検査します。
Coreには、日本語のXMLコメントや独自の書式・命名規則も含まれます。プロジェクトの方針に合わせて診断を設定してください。

## 導入する

利用先の `.csproj` の `ItemGroup` に追加してください。
中央パッケージ管理を使う場合は、バージョンを `Directory.Packages.props` に記述します。
XMLコメントの検査には、`.csproj` または `Directory.Build.props` の `GenerateDocumentationFile` を `true` にしてください。

```xml
<PackageReference Include="ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers"
                  Version="1.0.0"
                  PrivateAssets="all"
                  IncludeAssets="runtime; build; native; contentfiles; analyzers; buildtransitive" />
<AdditionalFiles Include="$(MSBuildProjectFullPath)" />
```

ソース参照・DLL参照を含む手順は[導入する](https://github.com/raw-inc/ReArtWord.CSharp.SourceCodeQuality.Analyzers/blob/main/docs/guides/installation.md)を参照してください。
v1.0.0は.NET SDK 10.0.402で検証しています。

## ルールと設定

- [Coreのルール一覧](https://github.com/raw-inc/ReArtWord.CSharp.SourceCodeQuality.Analyzers/blob/main/docs/rules/core.md)
- [検査を設定する](https://github.com/raw-inc/ReArtWord.CSharp.SourceCodeQuality.Analyzers/blob/main/docs/guides/configuration.md)
- [困ったとき](https://github.com/raw-inc/ReArtWord.CSharp.SourceCodeQuality.Analyzers/blob/main/docs/guides/troubleshooting.md)

## ライセンス

本パッケージは **LGPL-3.0-or-later** で提供します。

Copyright (C) 2026 raw inc. and Tomonari Hayashi

`LICENSE`、`licenses/GPL-3.0.txt`、`NOTICE`、`THIRD_PARTY_NOTICES.md` を同梱しています。
