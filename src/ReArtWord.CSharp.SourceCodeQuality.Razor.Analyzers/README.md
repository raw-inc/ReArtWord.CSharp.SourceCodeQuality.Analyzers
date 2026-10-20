# ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers

Razorコンポーネントの処理配置と、取消可能な呼出しへのCancellationTokenの伝搬を検査します。
Coreと併用し、Razorコンポーネントがあるプロジェクトへ追加してください。CommunityToolkit.Mvvmは必要ありません。

## 導入する

利用先の `.csproj` の `ItemGroup` に追加してください。
中央パッケージ管理を使う場合は、バージョンを `Directory.Packages.props` に記述します。

```xml
<PackageReference Include="ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers"
                  Version="1.0.0"
                  PrivateAssets="all"
                  IncludeAssets="runtime; build; native; contentfiles; analyzers; buildtransitive" />
```

ソース参照・DLL参照を含む手順は[導入する](https://github.com/raw-inc/ReArtWord.CSharp.SourceCodeQuality.Analyzers/blob/main/docs/guides/installation.md)を参照してください。
v1.0.0は.NET SDK 10.0.402で検証しています。

## ルールと設定

- [Razorのルール一覧](https://github.com/raw-inc/ReArtWord.CSharp.SourceCodeQuality.Analyzers/blob/main/docs/rules/razor.md)
- [検査を設定する](https://github.com/raw-inc/ReArtWord.CSharp.SourceCodeQuality.Analyzers/blob/main/docs/guides/configuration.md)
- [困ったとき](https://github.com/raw-inc/ReArtWord.CSharp.SourceCodeQuality.Analyzers/blob/main/docs/guides/troubleshooting.md)

## ライセンス

本パッケージは **LGPL-3.0-or-later** で提供します。

Copyright (C) 2026 raw inc. and Tomonari Hayashi

`LICENSE`、`licenses/GPL-3.0.txt`、`NOTICE`、`THIRD_PARTY_NOTICES.md` を同梱しています。
