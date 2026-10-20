# Third-party notices

本リポジトリのビルド・解析・テストで使用する依存ライブラリの一覧です。
各ライブラリには、本体とは別にそれぞれのライセンス条件と著作権表示が適用されます。
アナライザーの配布パッケージには、自身のDLLとライセンス関連ファイルを格納し、Roslynやテスト用ライブラリのDLLは同梱しません。

本体の適用条件は [NOTICE](NOTICE) を参照してください。
以下は、解決されたNuGetパッケージのメタデータと、パッケージ内のLICENSE・NOTICEに基づく表記です。

## ビルドと解析

### Microsoft.CodeAnalysis.Analyzers 3.11.0

- 用途：アナライザー実装の検査（直接参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[Microsoft.CodeAnalysis.Analyzers](https://github.com/dotnet/roslyn-analyzers)
- パッケージ内の関連ファイル：`ThirdPartyNotices.txt`

### Microsoft.CodeAnalysis.CSharp 4.13.0

- 用途：C#の構文・意味解析（直接参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[Microsoft.CodeAnalysis.CSharp](https://github.com/dotnet/roslyn)
- パッケージ内の関連ファイル：`ThirdPartyNotices.rtf`

### Microsoft.CodeAnalysis.Common 4.13.0

- 用途：Roslynの解析基盤（依存パッケージを通じた参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[Microsoft.CodeAnalysis.Common](https://github.com/dotnet/roslyn)
- パッケージ内の関連ファイル：`ThirdPartyNotices.rtf`

### Microsoft.CodeAnalysis.NetAnalyzers 10.0.100

- 用途：.NETの静的解析（直接参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[Microsoft.CodeAnalysis.NetAnalyzers](https://github.com/dotnet/dotnet)
- パッケージ内の関連ファイル：`ThirdPartyNotices.txt`

### NETStandard.Library 2.0.3

- 用途：netstandard2.0の参照アセンブリ（SDKによる参照）
- 著作者：Microsoft
- 著作権表記：.NET Foundation and Contributors
- ライセンス：[MIT](https://github.com/dotnet/standard/blob/master/LICENSE.TXT)
- 配布元：[NETStandard.Library](https://dot.net/)
- パッケージ内の関連ファイル：`LICENSE.TXT` / `THIRD-PARTY-NOTICES.TXT`

### Microsoft.NETCore.Platforms 1.1.0

- 用途：NETStandard.Libraryが参照するプラットフォーム情報（依存パッケージを通じた参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[Microsoft .NET Library License](http://go.microsoft.com/fwlink/?LinkId=329770)
- 配布元：[Microsoft.NETCore.Platforms](https://dot.net/)
- パッケージ内の関連ファイル：`dotnet_library_license.txt` / `ThirdPartyNotices.txt`

### System.Buffers 4.5.1

- 用途：Roslynが使用する.NETの基本機能（依存パッケージを通じた参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT)
- 配布元：[System.Buffers](https://dot.net/)
- パッケージ内の関連ファイル：`LICENSE.TXT` / `THIRD-PARTY-NOTICES.TXT`

### System.Collections.Immutable 8.0.0

- 用途：Roslynの解析基盤（依存パッケージを通じた参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[System.Collections.Immutable](https://github.com/dotnet/runtime)
- パッケージ内の関連ファイル：`LICENSE.TXT` / `THIRD-PARTY-NOTICES.TXT`

### System.Memory 4.5.5

- 用途：Roslynが使用する.NETの基本機能（依存パッケージを通じた参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT)
- 配布元：[System.Memory](https://dot.net/)
- パッケージ内の関連ファイル：`LICENSE.TXT` / `THIRD-PARTY-NOTICES.TXT`

### System.Numerics.Vectors 4.5.0

- 用途：Roslynが使用する.NETの基本機能（依存パッケージを通じた参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT)
- 配布元：[System.Numerics.Vectors](https://dot.net/)
- パッケージ内の関連ファイル：`LICENSE.TXT` / `THIRD-PARTY-NOTICES.TXT`

### System.Reflection.Metadata 8.0.0

- 用途：Roslynの解析基盤（依存パッケージを通じた参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[System.Reflection.Metadata](https://github.com/dotnet/runtime)
- パッケージ内の関連ファイル：`LICENSE.TXT` / `THIRD-PARTY-NOTICES.TXT`

### System.Runtime.CompilerServices.Unsafe 6.0.0

- 用途：Roslynが使用する.NETの基本機能（依存パッケージを通じた参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[System.Runtime.CompilerServices.Unsafe](https://github.com/dotnet/runtime)
- パッケージ内の関連ファイル：`LICENSE.TXT` / `THIRD-PARTY-NOTICES.TXT`

### System.Text.Encoding.CodePages 7.0.0

- 用途：Roslynの解析基盤（依存パッケージを通じた参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[System.Text.Encoding.CodePages](https://github.com/dotnet/runtime)
- パッケージ内の関連ファイル：`LICENSE.TXT` / `THIRD-PARTY-NOTICES.TXT`

### System.Threading.Tasks.Extensions 4.5.4

- 用途：Roslynが使用する.NETの基本機能（依存パッケージを通じた参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://github.com/dotnet/corefx/blob/master/LICENSE.TXT)
- 配布元：[System.Threading.Tasks.Extensions](https://dot.net/)
- パッケージ内の関連ファイル：`LICENSE.TXT` / `THIRD-PARTY-NOTICES.TXT`

## テストとカバレッジ

### Microsoft.NET.Test.Sdk 17.14.1

- 用途：テストプロジェクト（直接参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[Microsoft.NET.Test.Sdk](https://github.com/microsoft/vstest)
- パッケージ内の関連ファイル：なし

### Microsoft.CodeCoverage 17.14.1

- 用途：テストとカバレッジの計測（依存パッケージを通じた参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[Microsoft.CodeCoverage](https://github.com/microsoft/vstest)
- パッケージ内の関連ファイル：`ThirdPartyNotices.txt` / `build/netstandard2.0/ThirdPartyNotices.txt`

### Microsoft.TestPlatform.ObjectModel 17.14.1

- 用途：テスト基盤（依存パッケージを通じた参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[Microsoft.TestPlatform.ObjectModel](https://github.com/microsoft/vstest)
- パッケージ内の関連ファイル：なし

### Microsoft.TestPlatform.TestHost 17.14.1

- 用途：テスト基盤（依存パッケージを通じた参照）
- 著作者：Microsoft
- 著作権表記：© Microsoft Corporation. All rights reserved.
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[Microsoft.TestPlatform.TestHost](https://github.com/microsoft/vstest)
- パッケージ内の関連ファイル：`ThirdPartyNotices.txt`

### coverlet.collector 6.0.4

- 用途：テストプロジェクト（直接参照）
- 著作者：tonerdo
- 著作権表記：パッケージに記載なし
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[coverlet.collector](https://github.com/coverlet-coverage/coverlet)
- パッケージ内の関連ファイル：なし

### Newtonsoft.Json 13.0.3

- 用途：テスト基盤（依存パッケージを通じた参照）
- 著作者：James Newton-King
- 著作権表記：Copyright © James Newton-King 2008
- ライセンス：[MIT](https://licenses.nuget.org/MIT)
- 配布元：[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
- パッケージ内の関連ファイル：`LICENSE.md`

### xunit 2.9.3

- 用途：テストプロジェクト（直接参照）
- 著作者：jnewkirk, bradwilson
- 著作権表記：Copyright (C) .NET Foundation
- ライセンス：[Apache-2.0](https://licenses.nuget.org/Apache-2.0)
- 配布元：[xunit](https://github.com/xunit/xunit)
- パッケージ内の関連ファイル：なし

### xunit.runner.visualstudio 3.1.4

- 用途：テストランナー（直接参照）
- 著作者：jnewkirk, bradwilson
- 著作権表記：Copyright (C) .NET Foundation
- ライセンス：[Apache-2.0](https://licenses.nuget.org/Apache-2.0)
- 配布元：[xunit.runner.visualstudio](https://github.com/xunit/visualstudio.xunit)
- パッケージ内の関連ファイル：なし

### xunit.abstractions 2.0.3

- 用途：xUnitの実行基盤（依存パッケージを通じた参照）
- 著作者：James Newkirk, Brad Wilson
- 著作権表記：パッケージに記載なし
- ライセンス：[パッケージが示すライセンスURL](https://raw.githubusercontent.com/xunit/xunit/master/license.txt)
- 配布元：[xunit.abstractions](https://github.com/xunit/xunit)
- パッケージ内の関連ファイル：なし

### xunit.analyzers 1.18.0

- 用途：xUnitテストの静的解析（依存パッケージを通じた参照）
- 著作者：jnewkirk, bradwilson, marcind
- 著作権表記：Copyright (C) .NET Foundation
- ライセンス：[Apache-2.0](https://licenses.nuget.org/Apache-2.0)
- 配布元：[xunit.analyzers](https://github.com/xunit/xunit.analyzers)
- パッケージ内の関連ファイル：なし

### xunit.assert 2.9.3

- 用途：xUnitの実行基盤（依存パッケージを通じた参照）
- 著作者：jnewkirk, bradwilson
- 著作権表記：Copyright (C) .NET Foundation
- ライセンス：[Apache-2.0](https://licenses.nuget.org/Apache-2.0)
- 配布元：[xunit.assert](https://github.com/xunit/xunit)
- パッケージ内の関連ファイル：なし

### xunit.core 2.9.3

- 用途：xUnitの実行基盤（依存パッケージを通じた参照）
- 著作者：jnewkirk, bradwilson
- 著作権表記：Copyright (C) .NET Foundation
- ライセンス：[Apache-2.0](https://licenses.nuget.org/Apache-2.0)
- 配布元：[xunit.core](https://github.com/xunit/xunit)
- パッケージ内の関連ファイル：なし

### xunit.extensibility.core 2.9.3

- 用途：xUnitの実行基盤（依存パッケージを通じた参照）
- 著作者：jnewkirk, bradwilson
- 著作権表記：Copyright (C) .NET Foundation
- ライセンス：[Apache-2.0](https://licenses.nuget.org/Apache-2.0)
- 配布元：[xunit.extensibility.core](https://github.com/xunit/xunit)
- パッケージ内の関連ファイル：なし

### xunit.extensibility.execution 2.9.3

- 用途：xUnitの実行基盤（依存パッケージを通じた参照）
- 著作者：jnewkirk, bradwilson
- 著作権表記：Copyright (C) .NET Foundation
- ライセンス：[Apache-2.0](https://licenses.nuget.org/Apache-2.0)
- 配布元：[xunit.extensibility.execution](https://github.com/xunit/xunit)
- パッケージ内の関連ファイル：なし

## 依存関係を更新するとき

リポジトリのルートで解決結果を確認し、変更したパッケージのメタデータと同梱表記を見直してください。

```powershell
dotnet list ReArtWord.CSharp.SourceCodeQuality.Analyzers.slnx package --include-transitive
```

配布物の内容と利用先での確認手順は、[リリースする](docs/development/releases.md)にまとめています。
