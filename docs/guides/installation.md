# 導入する

[ドキュメント一覧](../README.md) · [検査を設定する](configuration.md) · [困ったとき](troubleshooting.md)

アナライザーは、IDEとビルド時にC#ソースを検査します。
Coreを基本として、CommunityToolkit.Mvvm を使うプロジェクトにはMVVM、RazorコンポーネントのプロジェクトにはRazorを追加してください。

| パッケージ | 使用するプロジェクト |
| --- | --- |
| `ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers` | 共通の設計・命名・コメント・非同期処理の規則を適用するC#プロジェクト |
| `ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers` | CommunityToolkit.Mvvm を使う ViewModel があるプロジェクト |
| `ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers` | BlazorなどのRazorコンポーネントがあるプロジェクト |

v1.0.0 は .NET SDK 10.0.402 で検証しています。アナライザーの参照先は Roslyn 4.13.0、アセンブリの対象は `netstandard2.0` です。
検査対象アプリケーションの対象フレームワークとは別の条件です。特定の.NETバージョンでのみ有効になる規則は、各ルール一覧に記載しています。

## NuGetパッケージを参照する

以下はv1.0.0を参照する設定例です。NuGetフィードは、パッケージの配布元に合わせて指定してください。
ソースからパッケージを作る場合は、[配布パッケージを生成する](../development/releases.md#配布パッケージを生成する)を参照してください。

### Coreを追加する

利用先の `.csproj` に追加してください。

```xml
<PropertyGroup>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
<ItemGroup>
  <PackageReference Include="ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers"
                    Version="1.0.0"
                    PrivateAssets="all"
                    IncludeAssets="runtime; build; native; contentfiles; analyzers; buildtransitive" />
  <AdditionalFiles Include="$(MSBuildProjectFullPath)" />
</ItemGroup>
```

`GenerateDocumentationFile` はXMLコメントをコンパイラーに解析させるための設定です。CoreのXMLコメント検査を使う場合は有効にしてください。
`PrivateAssets="all"` は、利用先がパッケージを作る際にアナライザーの参照を依存先へ引き継がないための指定です。
`AdditionalFiles` は、プロジェクトファイル内の設定値と `NoWarn` の指定を検査に渡します。

### MVVMを追加する

Coreに加え、次の参照を追加してください。
`CommunityToolkit.Mvvm` は、利用先プロジェクトへ別途導入してください。アナライザーには同梱していません。

```xml
<PackageReference Include="ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers"
                  Version="1.0.0"
                  PrivateAssets="all"
                  IncludeAssets="runtime; build; native; contentfiles; analyzers; buildtransitive" />
```

### Razorを追加する

Coreに加え、次の参照を追加してください。Razorの検査にCommunityToolkit.Mvvmは必要ありません。

```xml
<PackageReference Include="ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers"
                  Version="1.0.0"
                  PrivateAssets="all"
                  IncludeAssets="runtime; build; native; contentfiles; analyzers; buildtransitive" />
```

### 中央パッケージ管理を使う場合

`Directory.Packages.props` に使用するパッケージのバージョンを指定し、`.csproj` 側の `PackageReference` から `Version` を外してください。

```xml
<PackageVersion Include="ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers" Version="1.0.0" />
<PackageVersion Include="ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers" Version="1.0.0" />
<PackageVersion Include="ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers" Version="1.0.0" />
```

## ソースを直接参照する

アナライザーを同時に開発する場合は、`ProjectReference` を使用できます。
次の例は、利用先プロジェクトの親フォルダーにアナライザーのリポジトリがある配置です。パスは実際の配置に合わせて変更してください。

```xml
<PropertyGroup>
  <AnalyzerSourceRoot>../ReArtWord.CSharp.SourceCodeQuality.Analyzers/</AnalyzerSourceRoot>
  <GenerateDocumentationFile>true</GenerateDocumentationFile>
</PropertyGroup>
<ItemGroup>
  <ProjectReference Include="$(AnalyzerSourceRoot)src/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
  <AdditionalFiles Include="$(MSBuildProjectFullPath)" />
</ItemGroup>
```

MVVMまたはRazorを使う場合は、Coreの参照を残し、同じ形式で該当プロジェクトの参照を追加してください。
`OutputItemType="Analyzer"` と `ReferenceOutputAssembly="false"` により、コンパイラーが検査用のアセンブリとして読み込みます。

## ビルド済みのDLLを参照する

[開発環境と検証](../development/build.md)の手順でReleaseビルドしたDLLを、利用先の `Analyzer` 項目へ指定してください。
Coreの場合は次の形式です。XMLコメントの検査には、この方式でも `GenerateDocumentationFile` を有効にしてください。

```xml
<ItemGroup>
  <Analyzer Include="../ReArtWord.CSharp.SourceCodeQuality.Analyzers/src/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers/bin/Release/netstandard2.0/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.dll" />
  <AdditionalFiles Include="$(MSBuildProjectFullPath)" />
</ItemGroup>
```

MVVMまたはRazorを使う場合は、CoreのDLLに加えて該当するDLLを指定してください。
同じアナライザーを、パッケージ・プロジェクト・DLLの複数の方法で重複参照しないでください。

## 設定してビルドする

導入すると、既定で有効な規則が適用されます。Coreには日本語のXMLコメントや独自の命名・書式規則も含まれます。
[検査を設定する](configuration.md)を参照し、プロジェクトの方針に合わせて適用する規則を指定してください。

利用先のプロジェクトで実行してください。

```powershell
dotnet restore
dotnet build -warnaserror
```

診断が出た場合は、[Core](../rules/core.md)・[MVVM](../rules/mvvm.md)・[Razor](../rules/razor.md)の一覧でIDと検査内容を確認できます。
