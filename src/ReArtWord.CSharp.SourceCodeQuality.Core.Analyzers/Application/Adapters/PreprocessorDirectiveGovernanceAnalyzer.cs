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
/// プリプロセッサディレクティブ運用を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class PreprocessorDirectiveGovernanceAnalyzer : DiagnosticAnalyzer
{
    private static readonly Regex RequiredCommentDirectiveRegex = new(
        "\\b(TODO|FIXME)\\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.FalsePreprocessorDirectiveCommentRule);

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
    }

    /// <summary>
    /// コンパイル単位を解析します。
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

        SourceText sourceText = compilationUnit.SyntaxTree.GetText(context.CancellationToken);
        bool isFalseSymbolDefined = IsFalsePreprocessorSymbolDefined(compilationUnit.SyntaxTree);
        List<SyntaxTrivia> commentTrivia = CollectCommentTrivia(compilationUnit);

        foreach (SyntaxTrivia trivia in compilationUnit.DescendantTrivia(descendIntoTrivia: true))
        {
            if (trivia.GetStructure() is not IfDirectiveTriviaSyntax ifDirective)
            {
                continue;
            }

            if (!IsTargetFalseCondition(ifDirective.Condition, isFalseSymbolDefined))
            {
                continue;
            }

            int lineNumber = sourceText.Lines.GetLineFromPosition(ifDirective.HashToken.SpanStart).LineNumber;
            if (HasRequiredCommentImmediatelyBefore(sourceText, commentTrivia, lineNumber))
            {
                continue;
            }

            TextSpan span = new(ifDirective.HashToken.SpanStart, 1);
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.FalsePreprocessorDirectiveCommentRule,
                    Location.Create(compilationUnit.SyntaxTree, span)));
        }
    }

    /// <summary>
    /// コメントTriviaを収集します。
    /// </summary>
    /// <param name="compilationUnit">コンパイル単位です。</param>
    /// <returns>コメントTrivia一覧です。</returns>
    private static List<SyntaxTrivia> CollectCommentTrivia(CompilationUnitSyntax compilationUnit)
    {
        List<SyntaxTrivia> comments = [];
        foreach (SyntaxTrivia trivia in compilationUnit.DescendantTrivia(descendIntoTrivia: true))
        {
            if (IsTargetCommentTrivia(trivia))
            {
                comments.Add(trivia);
            }
        }

        return comments;
    }

    /// <summary>
    /// FALSEプリプロセッサシンボルが定義されているか判定します。
    /// </summary>
    /// <param name="syntaxTree">構文木です。</param>
    /// <returns>FALSEシンボルが定義されている場合は <see langword="true" /> を返します。</returns>
    private static bool IsFalsePreprocessorSymbolDefined(SyntaxTree syntaxTree)
    {
        if (syntaxTree.Options is not CSharpParseOptions parseOptions)
        {
            return false;
        }

        return parseOptions.PreprocessorSymbolNames.Any(static symbol =>
            symbol.Equals("FALSE", StringComparison.Ordinal));
    }

    /// <summary>
    /// 対象のFALSE条件か判定します。
    /// </summary>
    /// <param name="condition">プリプロセッサ条件式です。</param>
    /// <param name="isFalseSymbolDefined">FALSEシンボルが定義されているかどうかです。</param>
    /// <returns>対象条件の場合は <see langword="true" /> を返します。</returns>
    private static bool IsTargetFalseCondition(ExpressionSyntax condition, bool isFalseSymbolDefined)
    {
        ExpressionSyntax unwrappedCondition = UnwrapParentheses(condition);
        if (unwrappedCondition.IsKind(SyntaxKind.FalseLiteralExpression))
        {
            return true;
        }

        return !isFalseSymbolDefined
            && unwrappedCondition is IdentifierNameSyntax identifierName
            && identifierName.Identifier.ValueText.Equals("FALSE", StringComparison.Ordinal);
    }

    /// <summary>
    /// 括弧を除いた条件式を取得します。
    /// </summary>
    /// <param name="condition">条件式です。</param>
    /// <returns>括弧を除いた条件式です。</returns>
    private static ExpressionSyntax UnwrapParentheses(ExpressionSyntax condition)
    {
        ExpressionSyntax current = condition;
        while (current is ParenthesizedExpressionSyntax parenthesizedExpression)
        {
            current = parenthesizedExpression.Expression;
        }

        return current;
    }

    /// <summary>
    /// 直前行にTODO/FIXMEコメントがあるか判定します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="commentTrivia">コメントTrivia一覧です。</param>
    /// <param name="directiveLine">対象ディレクティブ行番号です。</param>
    /// <returns>直前行に対象コメントがある場合は <see langword="true" /> を返します。</returns>
    private static bool HasRequiredCommentImmediatelyBefore(
        SourceText sourceText,
        List<SyntaxTrivia> commentTrivia,
        int directiveLine)
    {
        if (directiveLine == 0)
        {
            return false;
        }

        int expectedCommentEndLine = directiveLine - 1;
        foreach (SyntaxTrivia trivia in commentTrivia)
        {
            if (!RequiredCommentDirectiveRegex.IsMatch(trivia.ToString()))
            {
                continue;
            }

            if (IsStandaloneCommentEndingOnLine(sourceText, trivia, expectedCommentEndLine))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// コメントが指定行で終わる単独コメントか判定します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="trivia">コメントTriviaです。</param>
    /// <param name="expectedEndLine">期待する終了行番号です。</param>
    /// <returns>指定行で終わる単独コメントの場合は <see langword="true" /> を返します。</returns>
    private static bool IsStandaloneCommentEndingOnLine(
        SourceText sourceText,
        SyntaxTrivia trivia,
        int expectedEndLine)
    {
        LinePositionSpan lineSpan = sourceText.Lines.GetLinePositionSpan(trivia.Span);
        if (lineSpan.End.Line != expectedEndLine)
        {
            return false;
        }

        TextLine startLine = sourceText.Lines[lineSpan.Start.Line];
        string beforeComment = sourceText.ToString(TextSpan.FromBounds(startLine.Start, trivia.SpanStart));
        if (!string.IsNullOrWhiteSpace(beforeComment))
        {
            return false;
        }

        TextLine endLine = sourceText.Lines[lineSpan.End.Line];
        string afterComment = sourceText.ToString(TextSpan.FromBounds(trivia.Span.End, endLine.End));
        return string.IsNullOrWhiteSpace(afterComment);
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
}
