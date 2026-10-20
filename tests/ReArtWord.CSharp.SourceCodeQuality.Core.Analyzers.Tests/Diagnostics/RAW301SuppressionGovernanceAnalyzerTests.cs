using System.Diagnostics.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

// CA2007抑制理由: xUnitテストではConfigureAwait(false)がxUnit1030違反となるためテスト同期コンテキストを優先します。
[SuppressMessage(
    "Reliability",
    "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
    Justification = "xUnit1030回避のため、テストではConfigureAwaitを付与しない方針です。")]
public sealed class RAW301SuppressionGovernanceAnalyzerTests
{
    [Fact]
    public async Task RAW301_未許可NoWarnを検出する()
    {
        const string source = """
            /// <summary>テスト対象です。</summary>
            public sealed class Target
            {
            }
            """;

        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><NoWarn>$(NoWarn);CA2007</NoWarn></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW301");
    }

    [Fact]
    public async Task RAW301_許可済みNoWarnは検出しない()
    {
        const string source = """
            /// <summary>テスト対象です。</summary>
            public sealed class Target
            {
            }
            """;

        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><NoWarn>$(NoWarn);CS1591;CA1707</NoWarn></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW301");
    }

    [Fact]
    public async Task RAW301_props設定で許可IDを追加できる()
    {
        const string source = """
            /// <summary>テスト対象です。</summary>
            public sealed class Target
            {
            }
            """;

        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW301AllowedNoWarnIds>CA2007</RAW301AllowedNoWarnIds><NoWarn>$(NoWarn);CA2007</NoWarn></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW301");
    }

    [Fact]
    public async Task RAW301_英字接尾辞付き診断IDも検出する()
    {
        const string source = """
            /// <summary>テスト対象です。</summary>
            public sealed class Target
            {
            }
            """;

        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><NoWarn>$(NoWarn);RAW120A</NoWarn></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW301");
    }

    [Fact]
    public async Task RAW301_英字接尾辞付き許可IDを追加できる()
    {
        const string source = """
            /// <summary>テスト対象です。</summary>
            public sealed class Target
            {
            }
            """;

        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW301AllowedNoWarnIds>RAW120A</RAW301AllowedNoWarnIds><NoWarn>$(NoWarn);RAW120A</NoWarn></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW301");
    }

    [Fact]
    public async Task RAW301_旧props設定キーは読み取らない()
    {
        const string source = """
            /// <summary>テスト対象です。</summary>
            public sealed class Target
            {
            }
            """;

        string oldPropertyName = "RAW" + "004AllowedNoWarnIds";
        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = $"<Project><PropertyGroup><{oldPropertyName}>CA2007</{oldPropertyName}><NoWarn>$(NoWarn);CA2007</NoWarn></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW301");
    }

    [Fact]
    public async Task RAW301_csproj設定で許可IDを追加できる()
    {
        const string source = """
            /// <summary>テスト対象です。</summary>
            public sealed class Target
            {
            }
            """;

        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><NoWarn>$(NoWarn);CA2007</NoWarn></PropertyGroup></Project>",
            ["/repo/src/ReArtWord.SourceCodeQuality.Domain/ReArtWord.SourceCodeQuality.Domain.csproj"] = "<Project><PropertyGroup><RAW301AllowedNoWarnIds>CA2007</RAW301AllowedNoWarnIds></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW301");
    }

    [Fact]
    public async Task RAW301_propsとcsprojの許可IDは結合される()
    {
        const string source = """
            /// <summary>テスト対象です。</summary>
            public sealed class Target
            {
            }
            """;

        IReadOnlyDictionary<string, string> additionalFiles = new Dictionary<string, string>
        {
            ["/repo/Directory.Build.props"] = "<Project><PropertyGroup><RAW301AllowedNoWarnIds>CA2007</RAW301AllowedNoWarnIds><NoWarn>$(NoWarn);CA2007;CA1305</NoWarn></PropertyGroup></Project>",
            ["/repo/src/ReArtWord.SourceCodeQuality.Domain/ReArtWord.SourceCodeQuality.Domain.csproj"] = "<Project><PropertyGroup><RAW301AllowedNoWarnIds>CA1305</RAW301AllowedNoWarnIds></PropertyGroup></Project>",
        };

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain",
                additionalFiles);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW301");
    }

    [Fact]
    public async Task RAW310_Justification不足を検出する()
    {
        const string source = """
            using System.Diagnostics.CodeAnalysis;

            // 抑制理由: xUnitの非同期制約のため。
            [SuppressMessage("Reliability", "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください")]
            /// <summary>テスト対象です。</summary>
            public sealed class Target
            {
                /// <summary>初期化します。</summary>
                public Target()
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW310");
    }

    [Fact]
    public async Task RAW311_抑制理由コメント不足を検出する()
    {
        const string source = """
            using System.Diagnostics.CodeAnalysis;

            [SuppressMessage(
                "Reliability",
                "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
                Justification = "xUnit1030回避のためです。")]
            /// <summary>テスト対象です。</summary>
            public sealed class Target
            {
                /// <summary>初期化します。</summary>
                public Target()
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.Contains(diagnostics, static diagnostic => diagnostic.Id == "RAW311");
    }

    [Fact]
    public async Task RAW310とRAW311_理由付き抑制では検出しない()
    {
        const string source = """
            using System.Diagnostics.CodeAnalysis;

            // 抑制理由: xUnitの非同期制約のため。
            [SuppressMessage(
                "Reliability",
                "CA2007:待機中のタスクでの ConfigureAwait の呼び出しを検討してください",
                Justification = "xUnit1030回避のためです。")]
            /// <summary>テスト対象です。</summary>
            public sealed class Target
            {
                /// <summary>初期化します。</summary>
                public Target()
                {
                }
            }
            """;

        IReadOnlyCollection<Microsoft.CodeAnalysis.Diagnostic> diagnostics =
            await AnalyzerTestHelper.AnalyzeAsync(
                source,
                new SuppressionGovernanceAnalyzer(),
                "/repo/src/Target.cs",
                "ReArtWord.SourceCodeQuality.Domain");

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW310");
        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAW311");
    }
}
