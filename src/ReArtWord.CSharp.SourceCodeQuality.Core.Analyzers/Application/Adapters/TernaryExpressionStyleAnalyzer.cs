using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Configuration;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 三項演算子の条件数とフォーマットを検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class TernaryExpressionStyleAnalyzer : DiagnosticAnalyzer
{
    private const int DefaultTernaryConditionCountThreshold = 5;

    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.TernaryConditionCountLimitRule,
            DiagnosticCatalog.TernaryOperatorFormatRule);

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
        context.RegisterCompilationStartAction(static compilationContext =>
        {
            int threshold = AnalyzerConfigurationReader.GetTernaryConditionCountThreshold(
                compilationContext.Options,
                DefaultTernaryConditionCountThreshold,
                compilationContext.CancellationToken);

            compilationContext.RegisterSyntaxNodeAction(
                nodeContext => AnalyzeConditionalExpression(nodeContext, threshold),
                SyntaxKind.ConditionalExpression);
        });
    }

    /// <summary>
    /// 三項演算子を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="threshold">条件数の診断閾値です。</param>
    private static void AnalyzeConditionalExpression(SyntaxNodeAnalysisContext context, int threshold)
    {
        if (context.Node is not ConditionalExpressionSyntax conditionalExpression)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(conditionalExpression.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        AnalyzeTernaryFormat(context, conditionalExpression);

        if (HasAncestorConditionalExpression(conditionalExpression))
        {
            return;
        }

        int conditionCount = CountConditionalExpressions(conditionalExpression);
        if (conditionCount < threshold)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.TernaryConditionCountLimitRule,
                conditionalExpression.QuestionToken.GetLocation(),
                conditionCount,
                threshold));
    }

    /// <summary>
    /// 三項演算子フォーマットを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="conditionalExpression">三項演算子式です。</param>
    private static void AnalyzeTernaryFormat(
        SyntaxNodeAnalysisContext context,
        ConditionalExpressionSyntax conditionalExpression)
    {
        SyntaxToken questionToken = conditionalExpression.QuestionToken;
        SyntaxToken colonToken = conditionalExpression.ColonToken;
        if (questionToken.IsMissing || colonToken.IsMissing)
        {
            return;
        }

        SourceText sourceText = conditionalExpression.SyntaxTree.GetText(context.CancellationToken);
        foreach (Location location in GetTernaryFormatDiagnosticLocations(
            sourceText,
            conditionalExpression,
            questionToken,
            colonToken))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.TernaryOperatorFormatRule,
                    location));
        }
    }

    /// <summary>
    /// 三項演算子フォーマット違反の診断位置を取得します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="conditionalExpression">三項演算子式です。</param>
    /// <param name="questionToken">疑問符トークンです。</param>
    /// <param name="colonToken">コロントークンです。</param>
    /// <returns>診断位置一覧です。</returns>
    private static ImmutableArray<Location> GetTernaryFormatDiagnosticLocations(
        SourceText sourceText,
        ConditionalExpressionSyntax conditionalExpression,
        SyntaxToken questionToken,
        SyntaxToken colonToken)
    {
        ImmutableArray<Location>.Builder locations = ImmutableArray.CreateBuilder<Location>();
        LinePosition questionPosition = sourceText.Lines.GetLinePosition(questionToken.SpanStart);
        LinePosition colonPosition = sourceText.Lines.GetLinePosition(colonToken.SpanStart);
        if (questionPosition.Line == colonPosition.Line)
        {
            locations.Add(questionToken.GetLocation());
            return locations.ToImmutable();
        }

        if (!IsConditionAndWhenTrueOnQuestionLine(sourceText, conditionalExpression, questionPosition.Line))
        {
            locations.Add(questionToken.GetLocation());
        }

        if (questionPosition.Character != colonPosition.Character)
        {
            locations.Add(colonToken.GetLocation());
            return locations.ToImmutable();
        }

        if (HasNonWhitespaceBeforeTokenOnLine(sourceText, colonToken))
        {
            locations.Add(colonToken.GetLocation());
            return locations.ToImmutable();
        }

        return locations.ToImmutable();
    }

    /// <summary>
    /// 条件式末尾、疑問符、真側式先頭が同一行にあるか判定します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="conditionalExpression">三項演算子式です。</param>
    /// <param name="questionLine">疑問符の行番号です。</param>
    /// <returns>同一行にある場合は <see langword="true" /> を返します。</returns>
    private static bool IsConditionAndWhenTrueOnQuestionLine(
        SourceText sourceText,
        ConditionalExpressionSyntax conditionalExpression,
        int questionLine)
    {
        SyntaxToken conditionLastToken = conditionalExpression.Condition.GetLastToken();
        SyntaxToken whenTrueFirstToken = conditionalExpression.WhenTrue.GetFirstToken();
        if (IsUnavailableToken(conditionLastToken) || IsUnavailableToken(whenTrueFirstToken))
        {
            return true;
        }

        int conditionLastLine = GetTokenEndLine(sourceText, conditionLastToken);
        int whenTrueFirstLine = sourceText.Lines.GetLinePosition(whenTrueFirstToken.SpanStart).Line;
        return conditionLastLine == questionLine && whenTrueFirstLine == questionLine;
    }

    /// <summary>
    /// フォーマット判定に使えないトークンか判定します。
    /// </summary>
    /// <param name="token">対象トークンです。</param>
    /// <returns>利用できない場合は <see langword="true" /> を返します。</returns>
    private static bool IsUnavailableToken(SyntaxToken token)
    {
        return token.RawKind == 0 || token.IsMissing;
    }

    /// <summary>
    /// トークン終端の行番号を取得します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="token">対象トークンです。</param>
    /// <returns>トークン終端の行番号です。</returns>
    private static int GetTokenEndLine(SourceText sourceText, SyntaxToken token)
    {
        int tokenEndPosition = Math.Max(token.SpanStart, token.Span.End - 1);
        return sourceText.Lines.GetLinePosition(tokenEndPosition).Line;
    }

    /// <summary>
    /// トークンと同一行の前方に空白以外の文字があるか判定します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="token">対象トークンです。</param>
    /// <returns>空白以外の文字がある場合は <see langword="true" /> を返します。</returns>
    private static bool HasNonWhitespaceBeforeTokenOnLine(SourceText sourceText, SyntaxToken token)
    {
        TextLine line = sourceText.Lines.GetLineFromPosition(token.SpanStart);
        for (int position = line.Start; position < token.SpanStart; position++)
        {
            if (!char.IsWhiteSpace(sourceText[position]))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 祖先に三項演算子があるか判定します。
    /// </summary>
    /// <param name="conditionalExpression">三項演算子式です。</param>
    /// <returns>祖先に三項演算子がある場合は <see langword="true" /> を返します。</returns>
    private static bool HasAncestorConditionalExpression(ConditionalExpressionSyntax conditionalExpression)
    {
        return conditionalExpression.Ancestors().OfType<ConditionalExpressionSyntax>().Any();
    }

    /// <summary>
    /// 三項演算子式の条件数をカウントします。
    /// </summary>
    /// <param name="conditionalExpression">根となる三項演算子式です。</param>
    /// <returns>三項演算子の条件数です。</returns>
    private static int CountConditionalExpressions(ConditionalExpressionSyntax conditionalExpression)
    {
        return 1 + conditionalExpression.DescendantNodes().OfType<ConditionalExpressionSyntax>().Count();
    }
}
