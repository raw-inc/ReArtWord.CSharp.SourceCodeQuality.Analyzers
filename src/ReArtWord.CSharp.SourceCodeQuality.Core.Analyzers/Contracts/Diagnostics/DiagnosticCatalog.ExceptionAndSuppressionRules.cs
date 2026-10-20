using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

/// <summary>
/// 例外throwと警告抑制運用に関する診断定義です。
/// </summary>
internal static partial class DiagnosticCatalog
{
    /// <summary>
    /// throw;再スロー警告ルールの診断IDです。
    /// </summary>
    public const string BareRethrowWarningRuleId = "RAW330";

    /// <summary>
    /// 汎用Exception throw禁止ルールの診断IDです。
    /// </summary>
    public const string GenericExceptionThrowRuleId = "RAW331";

    /// <summary>
    /// catch変数再スロー禁止ルールの診断IDです。
    /// </summary>
    public const string CapturedExceptionThrowRuleId = "RAW332";

    /// <summary>
    /// catch内例外throw禁止ルールの診断IDです。
    /// </summary>
    public const string CatchBlockThrowRuleId = "RAW333";

    /// <summary>
    /// pragma warning disable禁止ルールの診断IDです。
    /// </summary>
    public const string PragmaWarningDisableRuleId = "RAW334";

    /// <summary>
    /// throw;再スロー警告ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor BareRethrowWarningRule = new(
        BareRethrowWarningRuleId,
        "例外再スロー警告",
        "`throw;` による再スローは特別な理由がある場合に限定してください。本当に再スローが必要な場合はSuppressMessageで本警告を抑制し、理由を明記してください。",
        "運用",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "catch内のthrow;再スローは特別な理由がある場合のみ許可し、必要時はSuppressMessageで理由を明記します.");

    /// <summary>
    /// 汎用Exception throw禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor GenericExceptionThrowRule = new(
        GenericExceptionThrowRuleId,
        "汎用Exception throw禁止",
        "汎用Exceptionを直接throwしないでください。目的に合う具体例外型を使用してください。",
        "運用",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "System.Exceptionを直接生成してthrowすることを禁止し、具体的な例外型の利用を要求します.");

    /// <summary>
    /// catch変数再スロー禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor CapturedExceptionThrowRule = new(
        CapturedExceptionThrowRuleId,
        "catch変数再スロー禁止",
        "catch変数 '{0}' を `throw {0};` で再スローしないでください。原則として再スローを避け、不可避な場合はスタックトレースを保持する `throw;` の使用を検討してください。",
        "運用",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "catch変数をthrow ex形式で再スローすることを禁止し、再スロー回避またはthrow;の検討を促します.");

    /// <summary>
    /// catch内例外throw禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor CatchBlockThrowRule = new(
        CatchBlockThrowRuleId,
        "catch内例外throw禁止",
        "catchブロック内で例外をthrowしないでください。例外処理方針を見直してください。",
        "運用",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "catchブロック内の式付きthrowを禁止し、例外の握り直しや再throwによる意図不明瞭化を防止します.");

    /// <summary>
    /// pragma warning disable禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor PragmaWarningDisableRule = new(
        PragmaWarningDisableRuleId,
        "#pragma warning disable禁止",
        "#pragma warning disable による警告抑制は禁止です。必要時はSuppressMessage属性で理由を明記してください。",
        "運用",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "警告抑制はSuppressMessage属性に限定し、#pragma warning disableによる抑制を禁止します.");
}
