using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// TODO系コメント運用とNotImplementedExceptionコメント運用を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommentDirectiveGovernanceAnalyzer : DiagnosticAnalyzer
{
    private static readonly Regex DirectiveTokenRegex = new(
        "\\b(TODO|FIXME|FYI)\\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    private static readonly Regex JapaneseTextRegex = new(
        "[ぁ-んァ-ヶ一-龯々〆ヵヶ]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.CommentDirectiveReasonRequiredRule,
            DiagnosticCatalog.NotImplementedDirectiveRequiredRule);

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
        context.RegisterSyntaxNodeAction(AnalyzeCompilationUnit, SyntaxKind.CompilationUnit);
        context.RegisterSyntaxNodeAction(AnalyzeThrowStatement, SyntaxKind.ThrowStatement);
        context.RegisterSyntaxNodeAction(AnalyzeThrowExpression, SyntaxKind.ThrowExpression);
    }

    /// <summary>
    /// コンパイル単位のコメントを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeCompilationUnit(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not CompilationUnitSyntax compilationUnit)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(compilationUnit.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        foreach (SyntaxTrivia trivia in compilationUnit.DescendantTrivia(descendIntoTrivia: true))
        {
            if (!IsTargetCommentTrivia(trivia))
            {
                continue;
            }

            AnalyzeDirectiveReason(context, trivia);
        }
    }

    /// <summary>
    /// throw文を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ThrowStatementSyntax throwStatement)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(throwStatement.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        AnalyzeNotImplementedThrow(context, throwStatement.Expression, throwStatement.ThrowKeyword);
    }

    /// <summary>
    /// throw式を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeThrowExpression(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ThrowExpressionSyntax throwExpression)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(throwExpression.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        AnalyzeNotImplementedThrow(context, throwExpression.Expression, throwExpression.ThrowKeyword);
    }

    /// <summary>
    /// TODO/FIXME/FYIコメントの理由記載有無を検証します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="trivia">コメントTriviaです。</param>
    private static void AnalyzeDirectiveReason(SyntaxNodeAnalysisContext context, SyntaxTrivia trivia)
    {
        string commentText = trivia.ToString();
        foreach (Match match in DirectiveTokenRegex.Matches(commentText))
        {
            if (HasJapaneseReasonAfterDirective(commentText, match))
            {
                continue;
            }

            TextSpan span = new(trivia.SpanStart + match.Index, match.Length);
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.CommentDirectiveReasonRequiredRule,
                    Location.Create(context.Node.SyntaxTree, span),
                    match.Value.ToUpperInvariant()));
        }
    }

    /// <summary>
    /// NotImplementedExceptionのコメント有無を検証します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="thrownExpression">throw対象式です。</param>
    /// <param name="throwKeyword">throwキーワードです。</param>
    private static void AnalyzeNotImplementedThrow(
        SyntaxNodeAnalysisContext context,
        ExpressionSyntax? thrownExpression,
        SyntaxToken throwKeyword)
    {
        if (thrownExpression is null)
        {
            return;
        }

        if (!IsNotImplementedExceptionType(context, thrownExpression))
        {
            return;
        }

        if (HasDirectiveComment(throwKeyword.LeadingTrivia))
        {
            return;
        }

        SyntaxToken previousToken = throwKeyword.GetPreviousToken();
        if (previousToken != default && HasDirectiveComment(previousToken.TrailingTrivia))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.NotImplementedDirectiveRequiredRule,
                GetNotImplementedExceptionLocation(thrownExpression)));
    }

    /// <summary>
    /// NotImplementedException利用箇所の診断位置を取得します。
    /// </summary>
    /// <param name="thrownExpression">throw対象式です。</param>
    /// <returns>診断位置です。</returns>
    private static Location GetNotImplementedExceptionLocation(ExpressionSyntax thrownExpression)
    {
        return thrownExpression switch
        {
            ObjectCreationExpressionSyntax objectCreation => objectCreation.Type.GetLocation(),
            _ => thrownExpression.GetLocation(),
        };
    }

    /// <summary>
    /// コメントTriviaが対象種別か判定します。
    /// </summary>
    /// <param name="trivia">判定対象Triviaです。</param>
    /// <returns>対象コメントの場合は <see langword="true" /> を返します。</returns>
    private static bool IsTargetCommentTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
            || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia);
    }

    /// <summary>
    /// 例外型がNotImplementedExceptionか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="expression">throw対象式です。</param>
    /// <returns>NotImplementedExceptionの場合は <see langword="true" /> を返します。</returns>
    private static bool IsNotImplementedExceptionType(SyntaxNodeAnalysisContext context, ExpressionSyntax expression)
    {
        ITypeSymbol? thrownType = context.SemanticModel.GetTypeInfo(expression, context.CancellationToken).Type;
        return thrownType?.ToDisplayString().Equals("System.NotImplementedException", StringComparison.Ordinal) == true;
    }

    /// <summary>
    /// Trivia集合にTODO/FIXME/FYIコメントが含まれるか判定します。
    /// </summary>
    /// <param name="triviaList">判定対象Trivia集合です。</param>
    /// <returns>対象コメントが含まれる場合は <see langword="true" /> を返します。</returns>
    private static bool HasDirectiveComment(SyntaxTriviaList triviaList)
    {
        foreach (SyntaxTrivia trivia in triviaList)
        {
            if (!IsTargetCommentTrivia(trivia))
            {
                continue;
            }

            if (DirectiveTokenRegex.IsMatch(trivia.ToString()))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// TODO/FIXME/FYIのPrefix後に日本語理由があるか判定します。
    /// </summary>
    /// <param name="commentText">コメント全文です。</param>
    /// <param name="directiveMatch">TODO/FIXME/FYIの一致結果です。</param>
    /// <returns>日本語理由がある場合は <see langword="true" /> を返します。</returns>
    private static bool HasJapaneseReasonAfterDirective(string commentText, Match directiveMatch)
    {
        string remaining = commentText.Substring(directiveMatch.Index + directiveMatch.Length).TrimStart();
        if (!remaining.StartsWith(":", StringComparison.Ordinal)
            && !remaining.StartsWith("：", StringComparison.Ordinal))
        {
            return false;
        }

        string reasonText = remaining.Substring(1);
        return JapaneseTextRegex.IsMatch(reasonText);
    }
}
