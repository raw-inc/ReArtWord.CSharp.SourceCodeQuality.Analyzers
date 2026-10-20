using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW452BracePlacementAnalyzerTests
{
    [Fact]
    public async Task RAW452_ifブロックを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(bool a, bool b)
                {
                    if (a == b) {
                        a = false;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new BracePlacementAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW452");
    }

    [Fact]
    public async Task RAW452_elseブロックを検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(bool condition)
                {
                    if (condition)
                    {
                        condition = false;
                    }
                    else {
                        condition = true;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new BracePlacementAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW452");
    }

    [Fact]
    public async Task RAW452_制御構文ブロックを検出する()
    {
        const string source = """
            using System;
            using System.Collections.Generic;
            using System.IO;

            public sealed class Target
            {
                public void Run(IEnumerable<int> values, object gate, int value)
                {
                    for (int i = 0; i < 1; i++) {
                        value = i;
                    }

                    foreach (int item in values) {
                        value = item;
                    }

                    while (value < 10) {
                        value++;
                    }

                    do {
                        value--;
                    }
                    while (0 < value);

                    try {
                        value = 1;
                    }
                    catch (InvalidOperationException) {
                        value = 2;
                    }
                    finally {
                        value = 3;
                    }

                    using (MemoryStream stream = new()) {
                        value = (int)stream.Length;
                    }

                    lock (gate) {
                        value++;
                    }

                    switch (value) {
                        case 0:
                            value = 1;
                            break;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new BracePlacementAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.True(diagnostics.Count(static diagnostic => diagnostic.Id == "RAW452") >= 10);
    }

    [Fact]
    public async Task RAW452_宣言ブロックを検出する()
    {
        const string source = """
            namespace Sample {
                public sealed class Target {
                    public int Value { get; }

                    public void Run() {
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new BracePlacementAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.True(diagnostics.Count(static diagnostic => diagnostic.Id == "RAW452") >= 4);
    }

    [Fact]
    public async Task RAW452_tests配下でも検出する()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(bool condition)
                {
                    if (condition) {
                        condition = false;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new BracePlacementAnalyzer(),
                "/repo/tests/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain.Tests");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW452");
    }

    [Fact]
    public async Task RAW452_次行配置は非検出()
    {
        const string source = """
            namespace Sample
            {
                public sealed class Target
                {
                    public int Value
                    {
                        get;
                    }

                    public void Run(bool condition, int value)
                    {
                        if (condition)
                        {
                            value++;
                        }

                        switch (value)
                        {
                            case 0:
                                value = 1;
                                break;
                        }
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new BracePlacementAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW452");
    }

    [Fact]
    public async Task RAW452_初期化子とswitch式は対象外()
    {
        const string source = """
            using System;
            using System.Collections.Generic;

            public sealed class Target
            {
                public object Create(int value)
                {
                    int[] numbers = { 1, 2 };
                    Dictionary<int, string> map = new() { [1] = "one" };
                    var anonymous = new { Name = "A" };
                    object boxed = value switch { 0 => anonymous, _ => map };
                    return new TargetData { Values = numbers, Boxed = boxed };
                }

                private sealed class TargetData
                {
                    public int[] Values
                    {
                        get;
                        set;
                    } = Array.Empty<int>();

                    public object Boxed
                    {
                        get;
                        set;
                    } = new object();
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new BracePlacementAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW452");
    }

    [Fact]
    public async Task RAW452_Analyzer自己テストは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(bool condition)
                {
                    if (condition) {
                        condition = false;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new BracePlacementAnalyzer(),
                "/repo/tests/ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests/Target.cs",
                "ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW452");
    }

    [Fact]
    public async Task RAW452_生成コードは非検出()
    {
        const string source = """
            public sealed class Target
            {
                public void Run(bool condition)
                {
                    if (condition) {
                        condition = false;
                    }
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new BracePlacementAnalyzer(),
                "/repo/src/Target.g.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW452");
    }
}
