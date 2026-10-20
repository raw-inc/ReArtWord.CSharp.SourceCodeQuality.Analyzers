using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

/// <summary>
/// 追加品質ルールに関する診断定義です。
/// </summary>
internal static partial class DiagnosticCatalog
{
    /// <summary>
    /// コレクションreadonly推奨ルールの診断IDです。
    /// </summary>
    public const string CollectionReadonlyRecommendationRuleId = "RAW170";

    /// <summary>
    /// DateTime利用抑制ルールの診断IDです。
    /// </summary>
    public const string DateTimeUsageRecommendationRuleId = "RAW171";

    /// <summary>
    /// LibraryImport利用推奨ルールの診断IDです。
    /// </summary>
    public const string LibraryImportRecommendationRuleId = "RAW172";

    /// <summary>
    /// returns必須ルールの診断IDです。
    /// </summary>
    public const string ReturnDocumentationRequiredRuleId = "RAW203";

    /// <summary>
    /// ArgumentNullException.ThrowIfNull必須ルールの診断IDです。
    /// </summary>
    public const string ArgumentNullThrowIfNullRuleId = "RAW340";

    /// <summary>
    /// ArgumentException ThrowIf文字列検証必須ルールの診断IDです。
    /// </summary>
    public const string ArgumentExceptionThrowIfStringRuleId = "RAW341";

    /// <summary>
    /// ArgumentOutOfRangeException ThrowIf必須ルールの診断IDです。
    /// </summary>
    public const string ArgumentOutOfRangeThrowIfRuleId = "RAW342";

    /// <summary>
    /// null比較演算子禁止ルールの診断IDです。
    /// </summary>
    public const string NullComparisonOperatorRuleId = "RAW460";

    /// <summary>
    /// 文字列比較Culture指定必須ルールの診断IDです。
    /// </summary>
    public const string StringComparisonCultureRuleId = "RAW461";

    /// <summary>
    /// 変数名アンダースコア抑制ルールの診断IDです。
    /// </summary>
    public const string VariableUnderscoreNamingRuleId = "RAW612";

    /// <summary>
    /// メソッド名アンダースコア抑制ルールの診断IDです。
    /// </summary>
    public const string MethodUnderscoreNamingRuleId = "RAW613";

    /// <summary>
    /// 引数名アンダースコア抑制ルールの診断IDです。
    /// </summary>
    public const string ParameterUnderscoreNamingRuleId = "RAW614";

    /// <summary>
    /// enum Eプレフィックス推奨ルールの診断IDです。
    /// </summary>
    public const string EnumPrefixRecommendationRuleId = "RAW615";

    /// <summary>
    /// interface Iプレフィックス必須ルールの診断IDです。
    /// </summary>
    public const string InterfacePrefixRequiredRuleId = "RAW616";

    /// <summary>
    /// 型PascalCase必須ルールの診断IDです。
    /// </summary>
    public const string TypePascalCaseRuleId = "RAW617";

    /// <summary>
    /// コレクションreadonly推奨ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor CollectionReadonlyRecommendationRule = new(
        CollectionReadonlyRecommendationRuleId,
        "コレクションreadonly推奨",
        "コレクション '{0}' は再代入を避けるため readonly、get-only、または init-only 化を検討してください。",
        "設計",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "コレクションフィールドとコレクションプロパティは再代入を避けるためreadonlyまたは読み取り専用公開を推奨します.");

    /// <summary>
    /// DateTime利用抑制ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor DateTimeUsageRecommendationRule = new(
        DateTimeUsageRecommendationRuleId,
        "DateTime利用見直し推奨",
        "DateTime型 '{0}' の利用は避け、DateTimeOffsetなど用途に合う型を検討してください。",
        "設計",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: ".NET8以上ではDateTime型利用を避け、DateTimeOffsetなどの利用を推奨します.");

    /// <summary>
    /// LibraryImport利用推奨ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor LibraryImportRecommendationRule = new(
        LibraryImportRecommendationRuleId,
        "LibraryImport利用推奨",
        ".NET10以降ではDllImportの利用を避け、LibraryImportへの移行を検討してください。",
        "設計",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: ".NET10以降のP/Invoke宣言では、互換性を確認したうえでLibraryImportの利用を推奨します.");

    /// <summary>
    /// returns必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ReturnDocumentationRequiredRule = new(
        ReturnDocumentationRequiredRuleId,
        "returns不足",
        "戻り値のあるメソッド '{0}' には returns ドキュメントコメントが必要です。",
        "ドキュメント",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "戻り値のあるメソッドではsummaryに加えてreturnsコメントを必須化します.");

    /// <summary>
    /// ArgumentNullException.ThrowIfNull必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ArgumentNullThrowIfNullRule = new(
        ArgumentNullThrowIfNullRuleId,
        "ArgumentNullException.ThrowIfNull必須",
        "ArgumentNullExceptionを直接throwせず、ArgumentNullException.ThrowIfNull(...) を使用してください。",
        "運用",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: ".NET6以上ではArgumentNullExceptionの直接throwではなくThrowIfNullの利用を必須化します.");

    /// <summary>
    /// ArgumentException ThrowIf文字列検証必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ArgumentExceptionThrowIfStringRule = new(
        ArgumentExceptionThrowIfStringRuleId,
        "ArgumentException.ThrowIf文字列検証必須",
        "文字列guardでArgumentExceptionを直接throwせず、ArgumentException.{0}(...) を使用してください。",
        "運用",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "対象TFMで標準提供される文字列guardではArgumentException.ThrowIfNullOrEmpty/ThrowIfNullOrWhiteSpaceの利用を必須化します.");

    /// <summary>
    /// ArgumentOutOfRangeException ThrowIf必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ArgumentOutOfRangeThrowIfRule = new(
        ArgumentOutOfRangeThrowIfRuleId,
        "ArgumentOutOfRangeException.ThrowIf必須",
        "範囲guardでArgumentOutOfRangeExceptionを直接throwせず、ArgumentOutOfRangeException.{0}(...) を使用してください。",
        "運用",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "対象TFMで標準提供される範囲guardではArgumentOutOfRangeException.ThrowIf系メソッドの利用を必須化します.");

    /// <summary>
    /// null比較演算子禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor NullComparisonOperatorRule = new(
        NullComparisonOperatorRuleId,
        "null比較演算子禁止",
        "nullを '{0}' で比較せず、is null、is not null、null条件演算子、null合体演算子など適切な構文を使用してください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "nullとの==または!=比較を禁止し、パターンマッチングやnull条件演算子などの利用を要求します.");

    /// <summary>
    /// 文字列比較Culture指定必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor StringComparisonCultureRule = new(
        StringComparisonCultureRuleId,
        "文字列比較方法明示必須",
        "文字列比較 '{0}' では StringComparison、StringComparer、CultureInfo、CompareOptions など比較方法を明示してください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "文字列比較ではカルチャまたは序数比較の方針を明示する必要があります.");

    /// <summary>
    /// 変数名アンダースコア抑制ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor VariableUnderscoreNamingRule = new(
        VariableUnderscoreNamingRuleId,
        "変数名アンダースコア利用警告",
        "変数名 '{0}' では、特別な理由がない限りアンダースコア利用を避けてください。",
        "命名",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "変数名ではdiscardなどの特別な理由を除きアンダースコア利用を避けることを推奨します. 専用の命名規則が有効なprivateフィールドは、その規則で検査します.");

    /// <summary>
    /// メソッド名アンダースコア抑制ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor MethodUnderscoreNamingRule = new(
        MethodUnderscoreNamingRuleId,
        "メソッド名アンダースコア利用警告",
        "メソッド名 '{0}' では、特別な理由がない限りアンダースコア利用を避けてください。",
        "命名",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "メソッド名では特別な理由を除きアンダースコア利用を避けることを推奨します.");

    /// <summary>
    /// 引数名アンダースコア抑制ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ParameterUnderscoreNamingRule = new(
        ParameterUnderscoreNamingRuleId,
        "引数名アンダースコア利用警告",
        "引数名 '{0}' では、特別な理由がない限りアンダースコア利用を避けてください。",
        "命名",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "引数名ではdiscardなどの特別な理由を除きアンダースコア利用を避けることを推奨します.");

    /// <summary>
    /// enum Eプレフィックス推奨ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor EnumPrefixRecommendationRule = new(
        EnumPrefixRecommendationRuleId,
        "enum Eプレフィックス推奨",
        "enum '{0}' には E プレフィックス付与を検討してください。",
        "命名",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "enum型にはEプレフィックス付与を推奨します.");

    /// <summary>
    /// interface Iプレフィックス必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor InterfacePrefixRequiredRule = new(
        InterfacePrefixRequiredRuleId,
        "interface Iプレフィックス必須",
        "interface '{0}' には I プレフィックスを付与してください。",
        "命名",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "interface型名にはIプレフィックスを必須化します.");

    /// <summary>
    /// 型PascalCase必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor TypePascalCaseRule = new(
        TypePascalCaseRuleId,
        "型名PascalCase必須",
        "型名 '{0}' は UpperCamelCase(PascalCase) で定義してください。",
        "命名",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "class、struct、interface、record、enumの型名はPascalCaseで定義する必要があります.");
}
