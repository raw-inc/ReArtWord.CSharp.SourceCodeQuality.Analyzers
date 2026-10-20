using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers.Contracts.Diagnostics;

/// <summary>
/// Razor コンポーネントの品質規則に対応する診断定義です。
/// </summary>
internal static class RazorDiagnosticCatalog
{
    /// <summary>
    /// Razor 内のメンバー宣言をコードビハインドへ分離する規則です。
    /// </summary>
    public static readonly DiagnosticDescriptor CodeBehindMemberRule = new(
        "RAWRAZOR001",
        "Razorの処理をコードビハインドへ分離",
        "コンポーネントのメンバーを .razor.cs 等のコードビハインドへ移してください。",
        "Razor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Razorに記述したフィールド・プロパティ・メソッド等を検出します. 表示式やHTMLの描画処理は対象外です.");

    /// <summary>
    /// コンポーネントからの取消可能な呼出しへトークン伝搬を要求する規則です。
    /// </summary>
    public static readonly DiagnosticDescriptor CancellationPropagationRule = new(
        "RAWRAZOR002",
        "画面の存続期間に対応する取消しを伝搬",
        "取消可能な呼出し '{0}' には画面の存続期間に対応するCancellationTokenを明示してください。",
        "Razor",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "コンポーネント内のCancellationToken引数の省略・default・Noneを検出します. DisposeとDisposeAsyncの解放処理は対象外です.");

    /// <summary>
    /// サポートする診断定義一覧を作成します。
    /// </summary>
    /// <returns>診断定義一覧です。</returns>
    public static ImmutableArray<DiagnosticDescriptor> CreateSupportedDiagnostics()
    {
        return ImmutableArray.Create(CodeBehindMemberRule, CancellationPropagationRule);
    }
}
