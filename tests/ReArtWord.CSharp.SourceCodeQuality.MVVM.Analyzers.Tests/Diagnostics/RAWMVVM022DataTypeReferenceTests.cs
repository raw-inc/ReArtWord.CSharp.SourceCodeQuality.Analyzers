using Microsoft.CodeAnalysis;
using ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Application;

namespace ReArtWord.CSharp.SourceCodeQuality.MVVM.Analyzers.Tests.Diagnostics;

/// <summary>
/// データ型や画面名に似た型を画面参照と誤認しないことを検証します。
/// </summary>
public sealed class RAWMVVM022DataTypeReferenceTests
{
    /// <summary>
    /// DataViewの引数を、完全修飾名や別名でも許可します。
    /// </summary>
    /// <param name="typeName">引数に指定する型名です。</param>
    /// <returns>検証完了を表すタスクです。</returns>
    [Theory]
    [InlineData("System.Data.DataView")]
    [InlineData("global::System.Data.DataView")]
    [InlineData("Rows")]
    public async Task RAWMVVM022_DataViewの引数を検出しない(string typeName)
    {
        string source = "using Rows = System.Data.DataView;\n"
            + MvvmAnalyzerTestSources.WithToolkitStubs($$"""
                public sealed class OrdersViewModel : ObservableObject
                {
                    public static int CountRows({{typeName}} rows) => rows.Count;
                }
                """);

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source).ConfigureAwait(true);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM022");
    }

    /// <summary>
    /// データ型をフィールド、プロパティ、戻り値、配列、型引数に使っても許可します。
    /// </summary>
    /// <param name="typeName">データ型の完全修飾名です。</param>
    /// <returns>検証完了を表すタスクです。</returns>
    [Theory]
    [InlineData("System.Data.DataView")]
    [InlineData("System.ComponentModel.ICollectionView")]
    [InlineData("System.Windows.Data.CollectionView")]
    public async Task RAWMVVM022_データ型の各参照を検出しない(string typeName)
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs($$"""
            public sealed class OrdersViewModel : ObservableObject
            {
                private {{typeName}}? rows;

                public {{typeName}}? Rows => rows;

                public {{typeName}}? GetRows() => rows;

                public void SetRows({{typeName}}[] items) => rows = items[0];

                public List<{{typeName}}> CreateList() => new List<{{typeName}}>();

                public Type GetRowType() => typeof({{typeName}});
            }
            """) + """

            namespace System.ComponentModel
            {
                public interface ICollectionView { }
            }
            namespace System.Windows.Data
            {
                public class CollectionView : System.ComponentModel.ICollectionView { }
            }
            """;

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source).ConfigureAwait(true);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM022");
    }

    /// <summary>
    /// 名前や配置が画面型に似ていても、UIの基底型を継承しない型は許可します。
    /// </summary>
    /// <param name="typeNamespace">型を宣言する名前空間です。</param>
    /// <param name="typeName">画面型に似た型名です。</param>
    /// <returns>検証完了を表すタスクです。</returns>
    [Theory]
    [InlineData("Models", "DataView")]
    [InlineData("Models", "TimeWindow")]
    [InlineData("Models", "ResultPage")]
    [InlineData("Models", "UserControl")]
    [InlineData("Models", "FlowControl")]
    [InlineData("Models", "MessageBox")]
    [InlineData("Models", "ContentPage")]
    [InlineData("Views", "Customer")]
    [InlineData("Controls", "Customer")]
    [InlineData("Pages", "Customer")]
    [InlineData("Models", "DependencyObject")]
    [InlineData("Models", "Element")]
    [InlineData("Models", "AvaloniaObject")]
    public async Task RAWMVVM022_名前だけが画面型に似た型を検出しない(string typeNamespace, string typeName)
    {
        string source = MvvmAnalyzerTestSources.WithToolkitStubs($$"""
            namespace {{typeNamespace}}
            {
                public class {{typeName}} { }
                public sealed class DerivedData : {{typeName}} { }
            }
            public sealed class OrdersViewModel : ObservableObject
            {
                public {{typeNamespace}}.{{typeName}}? GetData() => null;

                public {{typeNamespace}}.DerivedData? GetDerivedData() => null;
            }
            """);

        IReadOnlyCollection<Diagnostic> diagnostics = await AnalyzeAsync(source).ConfigureAwait(true);

        Assert.DoesNotContain(diagnostics, static diagnostic => diagnostic.Id == "RAWMVVM022");
    }

    /// <summary>
    /// MVVMアナライザーの診断を取得します。
    /// </summary>
    /// <param name="source">解析対象ソースです。</param>
    /// <returns>診断一覧を表すタスクです。</returns>
    private static Task<IReadOnlyCollection<Diagnostic>> AnalyzeAsync(string source)
    {
        return AnalyzerTestHelper.AnalyzeAsync(
            source,
            new MvvmToolkitUsageAnalyzer(),
            "/repo/src/ReArtWord.App/Target.cs",
            "ReArtWord.App",
            AnalyzerTestHelper.CreateToolkitPackageFiles());
    }
}
