using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Configuration;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Domain.Configuration;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 抑制運用ポリシー（NoWarn禁止とSuppressMessage統制）を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SuppressionGovernanceAnalyzer : DiagnosticAnalyzer
{
    private static readonly Regex NoWarnValueRegex = new(
        "<NoWarn[^>]*>(?<value>.*?)</NoWarn>",
        RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.IgnoreCase);

    private static readonly Regex TokenRegex = new(
        "(?<token>\\$\\([^)]+\\)|[A-Za-z]{1,}[0-9]{3,4}[A-Za-z]?)",
        RegexOptions.Compiled);

    private static readonly string[] DefaultAllowedNoWarnCodes =
    [
        "CS1591",
        "CA1707",
    ];

    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.NoWarnGovernanceRule,
            DiagnosticCatalog.SuppressJustificationRule,
            DiagnosticCatalog.SuppressReasonCommentRule);

    /// <summary>
    /// Analyzer初期化処理です。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterCompilationAction(AnalyzeNoWarnGovernance);
        context.RegisterSyntaxNodeAction(AnalyzeSuppressMessageAttribute, SyntaxKind.Attribute);
    }

    /// <summary>
    /// AdditionalFiles内のNoWarn運用を検証します。
    /// </summary>
    /// <param name="context">コンパイル解析コンテキストです。</param>
    private static void AnalyzeNoWarnGovernance(CompilationAnalysisContext context)
    {
        HashSet<string> allowedNoWarnCodes = AnalyzerConfigurationReader.GetAllowedNoWarnIds(
            context.Options,
            DefaultAllowedNoWarnCodes,
            context.CancellationToken);

        foreach (AdditionalText additionalFile in context.Options.AdditionalFiles)
        {
            if (!IsMetadataConfigurationFile(additionalFile.Path))
            {
                continue;
            }

            SourceText? sourceText = additionalFile.GetText(context.CancellationToken);
            if (sourceText is null)
            {
                continue;
            }

            string content = sourceText.ToString();
            foreach (NoWarnToken token in ExtractNoWarnTokens(content))
            {
                if (token.Code.StartsWith("$(", StringComparison.Ordinal))
                {
                    continue;
                }

                if (allowedNoWarnCodes.Contains(token.Code))
                {
                    continue;
                }

                Location location = CreateLocation(additionalFile.Path, sourceText, token.Start, token.Length);
                Diagnostic diagnostic = Diagnostic.Create(
                    DiagnosticCatalog.NoWarnGovernanceRule,
                    location,
                    token.Code);

                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    /// <summary>
    /// SuppressMessage属性の利用形態を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeSuppressMessageAttribute(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not AttributeSyntax attribute)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(attribute.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (!IsSuppressMessageAttribute(attribute, context.SemanticModel, context.CancellationToken))
        {
            return;
        }

        if (!HasReasonComment(attribute))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.SuppressReasonCommentRule,
                    GetAttributeNameLocation(attribute)));
        }

        if (!HasValidJustification(attribute, out Location justificationLocation))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.SuppressJustificationRule,
                    justificationLocation));
        }
    }

    /// <summary>
    /// 対象ファイルがプロジェクト設定ファイルか判定します。
    /// </summary>
    /// <param name="path">対象ファイルパスです。</param>
    /// <returns>対象ファイルの場合は <see langword="true" /> を返します。</returns>
    private static bool IsMetadataConfigurationFile(string path)
    {
        return path.EndsWith(".props", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".targets", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// NoWarn要素からトークンを抽出します。
    /// </summary>
    /// <param name="content">設定ファイル内容です。</param>
    /// <returns>抽出トークン列です。</returns>
    private static IEnumerable<NoWarnToken> ExtractNoWarnTokens(string content)
    {
        foreach (Match noWarnMatch in NoWarnValueRegex.Matches(content))
        {
            Group valueGroup = noWarnMatch.Groups["value"];
            string value = valueGroup.Value;

            foreach (Match tokenMatch in TokenRegex.Matches(value))
            {
                string code = tokenMatch.Groups["token"].Value.Trim();
                if (code.Length == 0)
                {
                    continue;
                }

                if (!code.StartsWith("$(", StringComparison.Ordinal)
                    && !DiagnosticIdPolicy.IsValidDiagnosticId(code))
                {
                    continue;
                }

                int start = valueGroup.Index + tokenMatch.Index;
                yield return new NoWarnToken(code, start, tokenMatch.Length);
            }
        }
    }

    /// <summary>
    /// 追加ファイル上の診断位置を生成します。
    /// </summary>
    /// <param name="path">ファイルパスです。</param>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="start">開始オフセットです。</param>
    /// <param name="length">長さです。</param>
    /// <returns>生成された位置情報です。</returns>
    private static Location CreateLocation(string path, SourceText sourceText, int start, int length)
    {
        TextSpan span = new(start, length);
        LinePositionSpan lineSpan = sourceText.Lines.GetLinePositionSpan(span);
        return Location.Create(path, span, lineSpan);
    }

    /// <summary>
    /// 属性がSuppressMessageかどうか判定します。
    /// </summary>
    /// <param name="attribute">属性構文です。</param>
    /// <param name="semanticModel">セマンティックモデルです。</param>
    /// <param name="cancellationToken">キャンセル制御です。</param>
    /// <returns>SuppressMessage属性の場合は <see langword="true" /> を返します。</returns>
    private static bool IsSuppressMessageAttribute(AttributeSyntax attribute, SemanticModel semanticModel, CancellationToken cancellationToken)
    {
        SymbolInfo symbolInfo = semanticModel.GetSymbolInfo(attribute, cancellationToken);
        if (symbolInfo.Symbol is IMethodSymbol constructorSymbol)
        {
            return constructorSymbol.ContainingType.ToDisplayString().Equals(
                "System.Diagnostics.CodeAnalysis.SuppressMessageAttribute",
                StringComparison.Ordinal);
        }

        return attribute.Name.ToString().EndsWith("SuppressMessage", StringComparison.Ordinal)
            || attribute.Name.ToString().EndsWith("SuppressMessageAttribute", StringComparison.Ordinal);
    }

    /// <summary>
    /// 属性直前に抑制理由コメントがあるか判定します。
    /// </summary>
    /// <param name="attribute">属性構文です。</param>
    /// <returns>抑制理由コメントがある場合は <see langword="true" /> を返します。</returns>
    private static bool HasReasonComment(AttributeSyntax attribute)
    {
        if (attribute.Parent is not AttributeListSyntax attributeList)
        {
            return false;
        }

        return attributeList.GetLeadingTrivia().Any(trivia =>
            (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            && trivia.ToString().Contains("抑制理由", StringComparison.Ordinal));
    }

    /// <summary>
    /// SuppressMessageのJustification引数が有効か判定します。
    /// </summary>
    /// <param name="attribute">属性構文です。</param>
    /// <param name="diagnosticLocation">診断位置です。</param>
    /// <returns>有効なJustificationがある場合は <see langword="true" /> を返します。</returns>
    private static bool HasValidJustification(AttributeSyntax attribute, out Location diagnosticLocation)
    {
        diagnosticLocation = GetAttributeNameLocation(attribute);
        if (attribute.ArgumentList is null)
        {
            return false;
        }

        foreach (AttributeArgumentSyntax argument in attribute.ArgumentList.Arguments)
        {
            if (!IsJustificationArgument(argument))
            {
                continue;
            }

            diagnosticLocation = argument.GetLocation();
            if (argument.Expression is not LiteralExpressionSyntax literal
                || !literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return false;
            }

            string value = literal.Token.ValueText;
            return !string.IsNullOrWhiteSpace(value);
        }

        return false;
    }

    /// <summary>
    /// 属性名の診断位置を取得します。
    /// </summary>
    /// <param name="attribute">属性構文です。</param>
    /// <returns>診断位置です。</returns>
    private static Location GetAttributeNameLocation(AttributeSyntax attribute)
    {
        return attribute.Name.GetLocation();
    }

    /// <summary>
    /// Justification引数か判定します。
    /// </summary>
    /// <param name="argument">属性引数です。</param>
    /// <returns>Justification引数の場合は <see langword="true" /> を返します。</returns>
    private static bool IsJustificationArgument(AttributeArgumentSyntax argument)
    {
        return argument.NameEquals?.Name.Identifier.Text.Equals("Justification", StringComparison.Ordinal) == true;
    }

    /// <summary>
    /// NoWarnトークン保持用の値型です。
    /// </summary>
    private readonly struct NoWarnToken
    {
        /// <summary>
        /// NoWarnトークンを初期化します。
        /// </summary>
        /// <param name="code">診断IDです。</param>
        /// <param name="start">開始位置です。</param>
        /// <param name="length">文字長です。</param>
        public NoWarnToken(string code, int start, int length)
        {
            Code = code;
            Start = start;
            Length = length;
        }

        /// <summary>
        /// 診断IDを取得します。
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// 開始位置を取得します。
        /// </summary>
        public int Start { get; }

        /// <summary>
        /// 文字長を取得します。
        /// </summary>
        public int Length { get; }
    }
}
