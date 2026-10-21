using Microsoft.CodeAnalysis;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

/// <summary>
/// ReArtWord.SourceCodeQualityカスタムAnalyzerの診断定義です。
/// </summary>
internal static partial class DiagnosticCatalog
{
    /// <summary>
    /// クラス行数制限の診断IDです。
    /// </summary>
    public const string ClassLengthRuleId = "RAW101";

    /// <summary>
    /// summary必須の診断IDです。
    /// </summary>
    public const string SummaryRequiredRuleId = "RAW201";

    /// <summary>
    /// summary日本語必須の診断IDです。
    /// </summary>
    public const string SummaryJapaneseRuleId = "RAW202";

    /// <summary>
    /// NoWarn禁止ルールの診断IDです。
    /// </summary>
    public const string NoWarnGovernanceRuleId = "RAW301";

    /// <summary>
    /// SuppressMessageのJustification必須ルールの診断IDです。
    /// </summary>
    public const string SuppressJustificationRuleId = "RAW310";

    /// <summary>
    /// SuppressMessageの理由コメント必須ルールの診断IDです。
    /// </summary>
    public const string SuppressReasonCommentRuleId = "RAW311";

    /// <summary>
    /// コメント前空行ルール（通常時）の診断IDです。
    /// </summary>
    public const string CommentBlankLineRequiredRuleId = "RAW401";

    /// <summary>
    /// コメント前空行ルール（インデント増加直後）の診断IDです。
    /// </summary>
    public const string CommentBlankLineForbiddenAfterIndentRuleId = "RAW402";

    /// <summary>
    /// const利用抑制ルールの診断IDです。
    /// </summary>
    public const string ConstUsageRecommendationRuleId = "RAW410";

    /// <summary>
    /// awaitのConfigureAwait推奨ルールの診断IDです。
    /// </summary>
    public const string AwaitConfigureAwaitRecommendationRuleId = "RAW411";

    /// <summary>
    /// var使用条件ルールの診断IDです。
    /// </summary>
    public const string VarUsageEligibilityRuleId = "RAW420";

    /// <summary>
    /// varプリミティブ型制限ルールの診断IDです。
    /// </summary>
    public const string VarPrimitiveRestrictionRuleId = "RAW421";

    /// <summary>
    /// lock文禁止ルールの診断IDです。
    /// </summary>
    public const string LockStatementRestrictionRuleId = "RAW501";

    /// <summary>
    /// フィールドprivate必須ルールの診断IDです。
    /// </summary>
    public const string FieldPrivateRuleId = "RAW102";

    /// <summary>
    /// メンバー間空行ルールの診断IDです。
    /// </summary>
    public const string MemberSpacingRuleId = "RAW430";

    /// <summary>
    /// メンバー終端後の空行違反messageです。
    /// </summary>
    public const string MemberSpacingMessage =
        "メンバー終端の '}' と次のコメントまたは宣言の間には空行を1行だけ入れてください。";

    /// <summary>
    /// Attribute list境界の空行違反messageです。
    /// </summary>
    public const string AttributeListSpacingMessage =
        "Attribute list同士の間、および最後のAttribute listと修飾対象宣言の間には空行を入れないでください。";

    /// <summary>
    /// 実行ブロック内連続空行禁止ルールの診断IDです。
    /// </summary>
    public const string ExecutableBlankLineRuleId = "RAW431";

    /// <summary>
    /// struct等値性実装整合ルールの診断IDです。
    /// </summary>
    public const string StructEqualityContractRuleId = "RAW110";

    /// <summary>
    /// データ型ToString必須ルールの診断IDです。
    /// </summary>
    public const string DataTypeToStringRuleId = "RAW111";

    /// <summary>
    /// while(true)禁止ルールの診断IDです。
    /// </summary>
    public const string WhileTrueProhibitedRuleId = "RAW440";

    /// <summary>
    /// ループ文波括弧必須ルールの診断IDです。
    /// </summary>
    public const string LoopBraceRequiredRuleId = "RAW441";

    /// <summary>
    /// if/else波括弧必須ルールの診断IDです。
    /// </summary>
    public const string IfElseBraceRequiredRuleId = "RAW442";

    /// <summary>
    /// for無条件ループ禁止ルールの診断IDです。
    /// </summary>
    public const string ForInfiniteLoopProhibitedRuleId = "RAW443";

    /// <summary>
    /// false条件プリプロセッサ化ルールの診断IDです。
    /// </summary>
    public const string ConstantFalseConditionRuleId = "RAW444";

    /// <summary>
    /// privateフィールド命名（アンダースコア付きlowerCamelCase）ルールの診断IDです。
    /// </summary>
    public const string PrivateFieldNamingWithUnderscoreRuleId = "RAW120B";

    /// <summary>
    /// privateフィールド命名（アンダースコア無しlowerCamelCase）ルールの診断IDです。
    /// </summary>
    public const string PrivateFieldNamingWithoutUnderscoreRuleId = "RAW120A";

    /// <summary>
    /// privateフィールド命名規則の競合を報告する診断IDです。
    /// </summary>
    public const string PrivateFieldNamingConflictRuleId = "RAW120C";

    /// <summary>
    /// partial class 宣言数上限ルールの診断IDです。
    /// </summary>
    public const string PartialClassDeclarationLimitRuleId = "RAW130";

    /// <summary>
    /// 明示Dispose呼び出し禁止ルールの診断IDです。
    /// </summary>
    public const string ExplicitDisposeInvocationRuleId = "RAW140";

    /// <summary>
    /// IDisposableローカル変数のusing必須ルールの診断IDです。
    /// </summary>
    public const string DisposableLocalUsingRequiredRuleId = "RAW141";

    /// <summary>
    /// ファイル内class/struct宣言数制限ルールの診断IDです。
    /// </summary>
    public const string FileClassStructDeclarationLimitRuleId = "RAW150";

    /// <summary>
    /// record-onlyファイルのrecord宣言数制限ルールの診断IDです。
    /// </summary>
    public const string RecordOnlyFileDeclarationLimitRuleId = "RAW151";

    /// <summary>
    /// mixed fileのrecord外部参照禁止ルールの診断IDです。
    /// </summary>
    public const string MixedFileRecordExternalReferenceRuleId = "RAW152";

    /// <summary>
    /// asyncメソッド接尾辞ルールの診断IDです。
    /// </summary>
    public const string AsyncMethodSuffixRuleId = "RAW601";

    /// <summary>
    /// unsafeメソッド接尾辞ルールの診断IDです。
    /// </summary>
    public const string UnsafeMethodSuffixRuleId = "RAW610";

    /// <summary>
    /// unsafe asyncメソッド接尾辞ルールの診断IDです。
    /// </summary>
    public const string UnsafeAsyncMethodSuffixRuleId = "RAW611";

    /// <summary>
    /// TODO/FIXME/FYIコメント理由必須ルールの診断IDです。
    /// </summary>
    public const string CommentDirectiveReasonRequiredRuleId = "RAW320";

    /// <summary>
    /// NotImplementedExceptionコメント必須ルールの診断IDです。
    /// </summary>
    public const string NotImplementedDirectiveRequiredRuleId = "RAW321";

    /// <summary>
    /// FALSEプリプロセッサ直前TODO/FIXME必須ルールの診断IDです。
    /// </summary>
    public const string FalsePreprocessorDirectiveCommentRuleId = "RAW322";

    /// <summary>
    /// else-if連鎖のswitch化ルールの診断IDです。
    /// </summary>
    public const string ElseIfChainSwitchRecommendationRuleId = "RAW450";

    /// <summary>
    /// 比較演算子方向ルールの診断IDです。
    /// </summary>
    public const string ComparisonOperatorDirectionRuleId = "RAW451";

    /// <summary>
    /// ブロック開き波括弧改行ルールの診断IDです。
    /// </summary>
    public const string BlockOpenBraceNewLineRuleId = "RAW452";

    /// <summary>
    /// 条件部stringハードコーディング警告ルールの診断IDです。
    /// </summary>
    public const string ConditionalStringHardcodingRuleId = "RAW453";

    /// <summary>
    /// 空白Tab禁止ルールの診断IDです。
    /// </summary>
    public const string WhitespaceTabProhibitedRuleId = "RAW454";

    /// <summary>
    /// 三項演算子条件数上限ルールの診断IDです。
    /// </summary>
    public const string TernaryConditionCountLimitRuleId = "RAW455";

    /// <summary>
    /// 三項演算子フォーマットルールの診断IDです。
    /// </summary>
    public const string TernaryOperatorFormatRuleId = "RAW456";

    /// <summary>
    /// トークン間連続Space禁止ルールの診断IDです。
    /// </summary>
    public const string ConsecutiveSpaceProhibitedRuleId = "RAW457";

    /// <summary>
    /// 演算子左右Space必須ルールの診断IDです。
    /// </summary>
    public const string OperatorSpacingRuleId = "RAW458";

    /// <summary>
    /// カンマ区切りSpaceルールの診断IDです。
    /// </summary>
    public const string CommaSeparatorSpacingRuleId = "RAW459";

    /// <summary>
    /// 非同期メソッドCancellationToken必須ルールの診断IDです。
    /// </summary>
    public const string AsyncCancellationTokenRequiredRuleId = "RAW510";

    /// <summary>
    /// fire-and-forget禁止ルールの診断IDです。
    /// </summary>
    public const string FireAndForgetProhibitedRuleId = "RAW511";

    /// <summary>
    /// IAsyncEnumerable推奨ルールの診断IDです。
    /// </summary>
    public const string AsyncEnumerableRecommendationRuleId = "RAW160";

    /// <summary>
    /// クラス行数制限の診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ClassLengthRule = new(
        ClassLengthRuleId,
        "クラス行数制限違反",
        "クラス '{0}' は {1} 行です。1クラス{2}行以下にしてください。責務分離が適切に行われていない可能性が高いです。",
        "設計",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "本番C#クラスは既定350行以内に制限します. 上限はRAW101MaxClassLineCountで設定できます. テストコードと生成コードは除外します.");

    /// <summary>
    /// summary必須の診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor SummaryRequiredRule = new(
        SummaryRequiredRuleId,
        "summary不足",
        "{0} '{1}' に summary ドキュメントコメントが必要です。",
        "ドキュメント",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "本番C#の全型・全メソッド（private/ctor含む）にsummaryコメントを必須化します.");

    /// <summary>
    /// summary日本語必須の診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor SummaryJapaneseRule = new(
        SummaryJapaneseRuleId,
        "summary日本語不足",
        "{0} '{1}' の summary は日本語を含めて記述してください。",
        "ドキュメント",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "summaryコメントは日本語を含む必要があります.");

    /// <summary>
    /// NoWarn禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor NoWarnGovernanceRule = new(
        NoWarnGovernanceRuleId,
        "NoWarn禁止違反",
        "NoWarnへ未許可ID '{0}' を追加しないでください。必要時はSuppressMessage属性で抑制してください。",
        "運用",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "NoWarnへの診断ID追加を禁止し、抑制は属性で理由明記する運用を強制します. 許可IDはRAW301AllowedNoWarnIdsで追加できます.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    /// <summary>
    /// SuppressMessageのJustification必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor SuppressJustificationRule = new(
        SuppressJustificationRuleId,
        "SuppressMessage理由不足",
        "SuppressMessageには非空のJustificationを記述してください。",
        "運用",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "SuppressMessage属性には抑制理由をJustificationへ記述する必要があります.");

    /// <summary>
    /// SuppressMessageの理由コメント必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor SuppressReasonCommentRule = new(
        SuppressReasonCommentRuleId,
        "SuppressMessageコメント不足",
        "SuppressMessageの直前に「抑制理由」を含むコメントを記述してください。",
        "運用",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "SuppressMessage属性の直前には抑制理由コメントが必要です.");

    /// <summary>
    /// コメント前空行ルール（通常時）の診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor CommentBlankLineRequiredRule = new(
        CommentBlankLineRequiredRuleId,
        "コメント前空行不足",
        "コメントの直前には1行分の空行を入れてください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "インデント増加直後を除き、コメントの直前にはちょうど1行の空行が必要です.");

    /// <summary>
    /// コメント前空行ルール（インデント増加直後）の診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor CommentBlankLineForbiddenAfterIndentRule = new(
        CommentBlankLineForbiddenAfterIndentRuleId,
        "コメント前空行過剰",
        "インデント増加直後のコメント直前には空行を入れないでください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "直前の非空行が'{'で終わる場合、コメント直前の空行は禁止です.");

    /// <summary>
    /// const利用抑制ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ConstUsageRecommendationRule = new(
        ConstUsageRecommendationRuleId,
        "const利用の見直し推奨",
        "const宣言 '{0}' は将来変更に備えて {1} の利用を検討してください。",
        "可読性",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "constはコンパイル時定数です. 将来変更の可能性がある場合はreadonlyまたは通常変数の利用を推奨します.");

    /// <summary>
    /// awaitのConfigureAwait推奨ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor AwaitConfigureAwaitRecommendationRule = new(
        AwaitConfigureAwaitRecommendationRuleId,
        "ConfigureAwait明示推奨",
        "await対象 '{0}' では ConfigureAwait(...) の明示を検討してください。",
        "可読性",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Task/ValueTaskをawaitする場合はコンテキスト捕捉要否を明示するためConfigureAwait指定を推奨します.");

    /// <summary>
    /// var使用条件ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor VarUsageEligibilityRule = new(
        VarUsageEligibilityRuleId,
        "var使用条件違反",
        "varは型が自明、または匿名型（LINQ匿名型を含む）の場合のみ使用できます。対象変数: '{0}'。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "varは型が自明な初期化または匿名型（LINQ匿名型を含む）に限定します.");

    /// <summary>
    /// varプリミティブ型制限ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor VarPrimitiveRestrictionRule = new(
        VarPrimitiveRestrictionRuleId,
        "varプリミティブ型制限違反",
        "varはboolを除くプリミティブ型で使用できません。対象変数: '{0}'、推論型: '{1}'。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "varはboolを除くプリミティブ型で禁止し、型付き静的メンバー例外のみ許可します.");

    /// <summary>
    /// lock文禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor LockStatementRestrictionRule = new(
        LockStatementRestrictionRuleId,
        "lock文禁止",
        "lock文は禁止です。処理の性質や共有資源の扱いに応じて、適切な排他制御・借用方法を検討してください（例: Interlockedによる更新、SemaphoreSlimによる同時実行数の制御など）。",
        "並行性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: ".NET8以上でlock文を検出し、同期・非同期の別や共有資源の利用範囲に応じた排他制御・借用方法の検討を促します.");

    /// <summary>
    /// フィールドprivate必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor FieldPrivateRule = new(
        FieldPrivateRuleId,
        "フィールドのprivate化必須",
        "フィールド '{0}' はprivateにしてください。カプセル化により予期しない変更を防止できます。",
        "設計",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: ".NET6以上ではclass/structのフィールドを原則privateに制限し、カプセル化を強制します.");

    /// <summary>
    /// メンバー間空行ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor MemberSpacingRule = new(
        MemberSpacingRuleId,
        "メンバー間およびAttribute境界空行ルール違反",
        "{0}",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "クラス内のメソッド/コンストラクタ/プロパティ終端後を空行1行に固定し、対象宣言のAttribute list間および最後のlistから宣言までを空行0行に固定します.");

    /// <summary>
    /// 実行ブロック内連続空行禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ExecutableBlankLineRule = new(
        ExecutableBlankLineRuleId,
        "実行ブロック内連続空行禁止",
        "実行ブロック内で空行を2つ以上連続させないでください。読みづらくなるので、行間を潰すのではなく適切な粒度で1行にしてください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "実行ブロック内では空行を2つ以上連続させず、処理粒度を見直して空行1行に整理します.");

    /// <summary>
    /// struct等値性実装整合ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor StructEqualityContractRule = new(
        StructEqualityContractRuleId,
        "struct等値性実装不足",
        "struct '{0}' は等値性実装の整合性を保つため、次をすべて実装してください: {1}。",
        "設計",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "structでEquals/GetHashCode/演算子のいずれかを実装した場合は全て実装して整合性を保つ必要があります.");

    /// <summary>
    /// データ型ToString必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor DataTypeToStringRule = new(
        DataTypeToStringRuleId,
        "データ型ToString不足",
        "具体処理を持たないデータ型 '{0}' では ToString() のオーバーライドが必須です。",
        "設計",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "データ保持中心の型はデバッグ性とログ可読性のためToStringを明示実装する必要があります.");

    /// <summary>
    /// while(true)禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor WhileTrueProhibitedRule = new(
        WhileTrueProhibitedRuleId,
        "while(true)禁止",
        "while(true) は使用禁止です。終了条件を明示してください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "無条件ループは意図が不明瞭になるため禁止し、明示的な終了条件を要求します.");

    /// <summary>
    /// ループ文波括弧必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor LoopBraceRequiredRule = new(
        LoopBraceRequiredRuleId,
        "ループ文波括弧必須",
        "for/foreach/while の本体は1行省略せず波括弧で囲んでください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "for/foreach/whileの単文省略を禁止し、メンテナンス時の誤読を防止します.");

    /// <summary>
    /// if/else波括弧必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor IfElseBraceRequiredRule = new(
        IfElseBraceRequiredRuleId,
        "if/else波括弧必須",
        "if/else の本体は1行省略せず波括弧で囲んでください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "if/elseの単文省略を禁止し、条件分岐変更時のバグ混入を防止します.");

    /// <summary>
    /// for無条件ループ禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ForInfiniteLoopProhibitedRule = new(
        ForInfiniteLoopProhibitedRuleId,
        "for無条件ループ禁止",
        "for 文で条件を省略または定数 true にしないでください。終了条件を明示してください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "for文の条件省略とコンパイル時定数true条件を禁止し、明示的な終了条件を要求します.");

    /// <summary>
    /// false条件プリプロセッサ化ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ConstantFalseConditionRule = new(
        ConstantFalseConditionRuleId,
        "false条件のプリプロセッサ化必須",
        "条件が常に false です。無効化コードは if/while で残さず #if FALSE などのプリプロセッサ条件へ移してください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "if/else-if/whileのコンパイル時定数false条件を禁止し、#if FALSEなどで無効化意図を明示することを要求します.");

    /// <summary>
    /// privateフィールド命名（アンダースコア付きlowerCamelCase）ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor PrivateFieldNamingWithUnderscoreRule = new(
        PrivateFieldNamingWithUnderscoreRuleId,
        "privateフィールド命名違反（アンダースコア付き）",
        "privateフィールド '{0}' は '_lowerCamelCase' 形式で命名してください。",
        "設計",
        DiagnosticSeverity.Error,
        isEnabledByDefault: false,
        description: "privateフィールド名を先頭アンダースコア付きlowerCamelCaseへ統一します. 既定では無効です.");

    /// <summary>
    /// privateフィールド命名（アンダースコア無しlowerCamelCase）ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor PrivateFieldNamingWithoutUnderscoreRule = new(
        PrivateFieldNamingWithoutUnderscoreRuleId,
        "privateフィールド命名違反（アンダースコア無し）",
        "privateフィールド '{0}' は 'lowerCamelCase' 形式で命名してください。",
        "設計",
        DiagnosticSeverity.Error,
        isEnabledByDefault: false,
        description: "privateフィールド名をアンダースコア無しlowerCamelCaseへ統一します. 既定では無効です.");

    /// <summary>
    /// privateフィールド命名規則の競合を報告する診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor PrivateFieldNamingConflictRule = new(
        PrivateFieldNamingConflictRuleId,
        "privateフィールド命名規則の設定が競合",
        "RAW120A と RAW120B は同時に有効にできません。いずれか一方を有効にし、もう一方の severity を none にしてください。",
        "設計",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "相反するprivateフィールド命名規則の同時指定を検出します. フィールド名を変更せず、規則の設定を見直してください.");

    /// <summary>
    /// partial class 宣言数上限ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor PartialClassDeclarationLimitRule = new(
        PartialClassDeclarationLimitRuleId,
        "partial class 宣言数制限違反",
        "partial class '{0}' は {1} 個に分割されています。partial classは{2}つまでとしてください。責務分離が適切に行われていない可能性が高いです。",
        "設計",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "手動実装ソースのpartial classは型ごとに既定4宣言までとし、上限はRAW130MaxPartialClassDeclarationCountで設定できます.");

    /// <summary>
    /// 明示Dispose呼び出し禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ExplicitDisposeInvocationRule = new(
        ExplicitDisposeInvocationRuleId,
        "明示Dispose呼び出し禁止",
        "通常メソッド内で '{0}' の明示呼び出しは使用しないでください。using句またはusing宣言で破棄を管理してください。",
        "設計",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: ".NET6以上のIDisposable/IAsyncDisposable実装クラスでは、通常メソッド内の明示Dispose呼び出しを禁止します.");

    /// <summary>
    /// IDisposableローカル変数のusing必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor DisposableLocalUsingRequiredRule = new(
        DisposableLocalUsingRequiredRuleId,
        "IDisposableローカルのusing必須",
        "ローカル変数 '{0}' は IDisposable/IAsyncDisposable 型です。using句またはusing宣言（await using含む）で宣言してください。",
        "設計",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: ".NET6以上のIDisposable/IAsyncDisposable実装クラスでは、通常メソッド内のIDisposableローカル宣言にusingを必須化します.");

    /// <summary>
    /// ファイル内class/struct宣言数制限ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor FileClassStructDeclarationLimitRule = new(
        FileClassStructDeclarationLimitRuleId,
        "ファイル内class/struct宣言数制限違反",
        "ファイル '{0}' では class/struct（record除く）は1つまでです。現在: {1} 個です。",
        "設計",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "1ファイルに宣言できるトップレベルclass/struct（record除く）は1つまでです.");

    /// <summary>
    /// record-onlyファイルのrecord宣言数制限ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor RecordOnlyFileDeclarationLimitRule = new(
        RecordOnlyFileDeclarationLimitRuleId,
        "record-onlyファイルのrecord宣言数制限違反",
        "recordのみで構成されたファイル '{0}' では record は1つまでです。現在: {1} 個です。",
        "設計",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "class/structを含まないrecord-onlyファイルでは、トップレベルrecordを1つまでに制限します.");

    /// <summary>
    /// mixed fileのrecord外部参照禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor MixedFileRecordExternalReferenceRule = new(
        MixedFileRecordExternalReferenceRuleId,
        "mixed fileのrecord外部参照禁止違反",
        "class/struct と同居する record '{0}' はファイル外から参照できません。1ファイル1recordへ分割してください。",
        "設計",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "class/structと同一ファイルにあるrecordは、そのファイル外からの参照を禁止します.",
        customTags: WellKnownDiagnosticTags.CompilationEnd);

    /// <summary>
    /// asyncメソッド接尾辞ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor AsyncMethodSuffixRule = new(
        AsyncMethodSuffixRuleId,
        "asyncメソッド接尾辞違反",
        "asyncメソッド '{0}' には 'Async' 接尾辞を付与してください。",
        "命名",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "async修飾子付きメソッドはAsync接尾辞で命名する必要があります.");

    /// <summary>
    /// unsafeメソッド接尾辞ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor UnsafeMethodSuffixRule = new(
        UnsafeMethodSuffixRuleId,
        "unsafeメソッド接尾辞推奨",
        "unsafe対象メソッド '{0}' には 'Unsafe' 接尾辞を付与してください。安全な実装である場合のみ SuppressMessage による抑制を検討してください。",
        "命名",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "unsafeメソッド、unsafeブロック含有メソッド、unsafe型内メソッドにはUnsafe接尾辞を推奨します.");

    /// <summary>
    /// unsafe asyncメソッド接尾辞ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor UnsafeAsyncMethodSuffixRule = new(
        UnsafeAsyncMethodSuffixRuleId,
        "unsafe asyncメソッド接尾辞推奨",
        "asyncかつunsafe対象のメソッド '{0}' には 'UnsafeAsync' 接尾辞を付与してください。安全な実装である場合のみ SuppressMessage による抑制を検討してください。",
        "命名",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "asyncかつunsafe対象メソッドはUnsafeAsync接尾辞で命名することを推奨します.");

    /// <summary>
    /// TODO/FIXME/FYIコメント理由必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor CommentDirectiveReasonRequiredRule = new(
        CommentDirectiveReasonRequiredRuleId,
        "TODO/FIXME/FYIコメント理由不足",
        "{0} コメントには Prefix 後に日本語の理由を記述してください。",
        "運用",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "TODO/FIXME/FYIコメントでは、TODO: のようなPrefixに続けて日本語の理由説明を必須化します.");

    /// <summary>
    /// NotImplementedExceptionコメント必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor NotImplementedDirectiveRequiredRule = new(
        NotImplementedDirectiveRequiredRuleId,
        "NotImplementedExceptionコメント不足",
        "NotImplementedException を使用する箇所には TODO/FIXME/FYI コメントを直近へ記述してください。",
        "運用",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "NotImplementedException を使用する場合は、実装予定や対応方針を TODO/FIXME/FYI コメントで明示する必要があります.");

    /// <summary>
    /// FALSEプリプロセッサ直前TODO/FIXME必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor FalsePreprocessorDirectiveCommentRule = new(
        FalsePreprocessorDirectiveCommentRuleId,
        "FALSEプリプロセッサコメント不足",
        "#if FALSE などで全環境無効にする直前には TODO または FIXME コメントで対応方針を記述してください。",
        "運用",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "#if FALSE、#if (FALSE)、#if false、#if (false) の直前には TODO または FIXME コメントが必要です.");

    /// <summary>
    /// else-if連鎖のswitch化ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ElseIfChainSwitchRecommendationRule = new(
        ElseIfChainSwitchRecommendationRuleId,
        "else-if連鎖のswitch化必須",
        "else if が {0} 個連鎖しています。switch 文または switch 式へ置き換えてください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "else if が3個以上連鎖する条件分岐は可読性低下を招くため、switch文またはswitch式へ置換を必須化します.");

    /// <summary>
    /// 比較演算子方向ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ComparisonOperatorDirectionRule = new(
        ComparisonOperatorDirectionRuleId,
        "比較演算子方向原則禁止",
        "比較演算子 '{0}' は原則禁止です。特別な理由がない限り、左ほど小さい比較方向へ統一して認知負荷を下げてください。",
        "可読性",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "二項比較演算子 > と >= の使用を避けてください。左ほど小さく右ほど大きい方向へそろえ、直感的な読みやすさを保ちます.");

    /// <summary>
    /// ブロック開き波括弧改行ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor BlockOpenBraceNewLineRule = new(
        BlockOpenBraceNewLineRuleId,
        "ブロック開き波括弧改行必須",
        "開き波括弧 '{' は前の構文と同じ行に置かず、次行に配置してください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "C#ブロックの開き波括弧は前の構文と同じ行に置かず、次行に配置する必要があります.");

    /// <summary>
    /// 条件部stringハードコーディング警告ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ConditionalStringHardcodingRule = new(
        ConditionalStringHardcodingRuleId,
        "条件部stringハードコーディング警告",
        "条件部でstringを直接記述しないでください。定数または設定値へ分離してください。",
        "可読性",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "if/else-if/while/do/for/三項演算子の条件部ではstringリテラルと補間文字列の直接記述を警告します.");

    /// <summary>
    /// 空白Tab禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor WhitespaceTabProhibitedRule = new(
        WhitespaceTabProhibitedRuleId,
        "空白Tab禁止",
        "空白にはTabを使用せず、Spaceで統一してください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "C#ソースの空白triviaに含まれるTabを禁止し、文字列リテラル内とコメント内のTabは対象外にします.");

    /// <summary>
    /// 三項演算子条件数上限ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor TernaryConditionCountLimitRule = new(
        TernaryConditionCountLimitRuleId,
        "三項演算子条件数上限違反",
        "三項演算子は {1} 条件未満にしてください。現在: {0} 条件です。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "ネストした三項演算子の条件数がRAW455TernaryConditionCountThreshold以上の場合に禁止します.");

    /// <summary>
    /// 三項演算子フォーマットルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor TernaryOperatorFormatRule = new(
        TernaryOperatorFormatRuleId,
        "三項演算子フォーマット違反",
        "三項演算子は条件式、'?'、真側式を同一行に置き、対応する ':' を '?' と同一列へ配置してください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "三項演算子では条件式末尾、?、真側式先頭を同一行に置き、対応する:を対応する?と同一列へ置き、:行の前方を空白のみにする必要があります.");

    /// <summary>
    /// トークン間連続Space禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor ConsecutiveSpaceProhibitedRule = new(
        ConsecutiveSpaceProhibitedRuleId,
        "トークン間連続Space禁止",
        "同一行のC#トークン間ではSpaceを2つ以上連続させないでください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "行頭インデント、改行、文字列本文、コメント本文を除き、同一行のC#トークン間にある連続Spaceを禁止します.");

    /// <summary>
    /// 演算子左右Space必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor OperatorSpacingRule = new(
        OperatorSpacingRuleId,
        "演算子左右Space必須",
        "演算子 '{0}' の左右にはSpaceを1つ入れてください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "代入、二項演算子、ラムダ/矢印の同一行側にはSpaceを1つ入れる必要があります.");

    /// <summary>
    /// カンマ区切りSpaceルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor CommaSeparatorSpacingRule = new(
        CommaSeparatorSpacingRuleId,
        "カンマ区切りSpace違反",
        "カンマ区切りは 'A, B' 形式で記述してください。",
        "可読性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "区切り用途のカンマは直前Spaceなし、同一行の次要素がある場合は直後Space1つへ統一します.");

    /// <summary>
    /// 非同期メソッドCancellationToken必須ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor AsyncCancellationTokenRequiredRule = new(
        AsyncCancellationTokenRequiredRuleId,
        "非同期メソッドCancellationToken不足",
        "Task/ValueTask メソッド '{0}' の最終引数は CancellationToken かつ '= default' を必須にしてください。",
        "並行性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Task/ValueTask を返す通常メソッドでは、最終引数に CancellationToken と default 値指定を必須化します.");

    /// <summary>
    /// fire-and-forget禁止ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor FireAndForgetProhibitedRule = new(
        FireAndForgetProhibitedRuleId,
        "fire-and-forget禁止",
        "Task/ValueTask の fire-and-forget は禁止です。await するか呼び出し元へ返却してください。",
        "並行性",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Task/ValueTask の discard 代入や未await単独呼び出しを禁止し、例外逸失や完了待機漏れを防止します.");

    /// <summary>
    /// IAsyncEnumerable推奨ルールの診断定義です。
    /// </summary>
    public static readonly DiagnosticDescriptor AsyncEnumerableRecommendationRule = new(
        AsyncEnumerableRecommendationRuleId,
        "IAsyncEnumerable利用推奨",
        "public async メソッド '{0}' は Enumerable系戻り値を返しています。IAsyncEnumerable の利用を検討してください。",
        "設計",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: ".NET6以上では、public async かつ Enumerable系戻り値の Task/ValueTask メソッドで IAsyncEnumerable の利用を推奨します.");
}
