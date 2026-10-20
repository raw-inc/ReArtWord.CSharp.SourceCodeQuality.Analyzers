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
/// コメント直前の空行ルールを検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CommentSpacingAnalyzer : DiagnosticAnalyzer
{
    private static readonly Regex OpenBraceLineRegex = new(
        "\\{\\s*(//.*)?$",
        RegexOptions.Compiled);

    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.CommentBlankLineRequiredRule,
            DiagnosticCatalog.CommentBlankLineForbiddenAfterIndentRule);

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
        Dictionary<int, int> commentStartPositions = CollectStandaloneCommentStartPositions(compilationUnit, sourceText);
        if (commentStartPositions.Count == 0)
        {
            return;
        }

        HashSet<int> commentLines = CollectStandaloneCommentLines(compilationUnit, sourceText);
        foreach (KeyValuePair<int, int> pair in commentStartPositions.OrderBy(static x => x.Key))
        {
            int lineNumber = pair.Key;
            if (lineNumber > 0 && commentLines.Contains(lineNumber - 1))
            {
                continue;
            }

            EvaluateCommentSpacing(context, sourceText, lineNumber, pair.Value);
        }
    }

    /// <summary>
    /// コメント行の空行ルールを評価します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="lineNumber">コメント開始行番号です。</param>
    /// <param name="commentStart">コメント開始位置です。</param>
    private static void EvaluateCommentSpacing(
        SyntaxNodeAnalysisContext context,
        SourceText sourceText,
        int lineNumber,
        int commentStart)
    {
        int blankLineCount = 0;
        int previousNonBlankLine = lineNumber - 1;
        while (previousNonBlankLine >= 0 && IsBlankLine(sourceText, previousNonBlankLine))
        {
            blankLineCount++;
            previousNonBlankLine--;
        }

        if (previousNonBlankLine < 0)
        {
            return;
        }

        string previousLine = sourceText.Lines[previousNonBlankLine].ToString().TrimEnd();
        bool isAfterIndentIncrement = OpenBraceLineRegex.IsMatch(previousLine);
        Location location = Location.Create(context.Node.SyntaxTree, new TextSpan(commentStart, 1));

        if (isAfterIndentIncrement)
        {
            if (blankLineCount == 0)
            {
                return;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.CommentBlankLineForbiddenAfterIndentRule,
                    location));
            return;
        }

        if (blankLineCount == 1)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.CommentBlankLineRequiredRule,
                location));
    }

    /// <summary>
    /// スタンドアロンコメント開始位置を行単位で収集します。
    /// </summary>
    /// <param name="compilationUnit">コンパイル単位です。</param>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <returns>キー:行番号、値:行内開始位置です。</returns>
    private static Dictionary<int, int> CollectStandaloneCommentStartPositions(
        CompilationUnitSyntax compilationUnit,
        SourceText sourceText)
    {
        Dictionary<int, int> positions = [];

        foreach (int commentStart in EnumerateCommentTrivia(compilationUnit).Select(static trivia => trivia.SpanStart))
        {
            if (!IsStandaloneComment(sourceText, commentStart))
            {
                continue;
            }

            int lineNumber = sourceText.Lines.GetLineFromPosition(commentStart).LineNumber;
            if (positions.TryGetValue(lineNumber, out int existingStart) && existingStart <= commentStart)
            {
                continue;
            }

            positions[lineNumber] = commentStart;
        }

        return positions;
    }

    /// <summary>
    /// スタンドアロンコメントが存在する行番号集合を収集します。
    /// </summary>
    /// <param name="compilationUnit">コンパイル単位です。</param>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <returns>コメント行番号集合です。</returns>
    private static HashSet<int> CollectStandaloneCommentLines(CompilationUnitSyntax compilationUnit, SourceText sourceText)
    {
        HashSet<int> lines = [];
        foreach (SyntaxTrivia trivia in EnumerateCommentTrivia(compilationUnit))
        {
            if (!IsStandaloneComment(sourceText, trivia.SpanStart))
            {
                continue;
            }

            LinePositionSpan lineSpan = sourceText.Lines.GetLinePositionSpan(trivia.Span);
            for (int index = lineSpan.Start.Line; index <= lineSpan.End.Line; index++)
            {
                lines.Add(index);
            }
        }

        return lines;
    }

    /// <summary>
    /// コメントTrivia列を列挙します。
    /// </summary>
    /// <param name="compilationUnit">コンパイル単位です。</param>
    /// <returns>コメントTrivia列です。</returns>
    private static IEnumerable<SyntaxTrivia> EnumerateCommentTrivia(CompilationUnitSyntax compilationUnit)
    {
        foreach (SyntaxTrivia trivia in compilationUnit.DescendantTrivia(descendIntoTrivia: true))
        {
            if (!IsCommentTrivia(trivia))
            {
                continue;
            }

            yield return trivia;
        }
    }

    /// <summary>
    /// コメントTriviaかどうか判定します。
    /// </summary>
    /// <param name="trivia">対象Triviaです。</param>
    /// <returns>コメントTriviaの場合は <see langword="true" /> を返します。</returns>
    private static bool IsCommentTrivia(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia)
            || trivia.IsKind(SyntaxKind.MultiLineCommentTrivia)
            || trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)
            || trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia)
            || trivia.IsKind(SyntaxKind.DocumentationCommentExteriorTrivia);
    }

    /// <summary>
    /// スタンドアロンコメントかどうか判定します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="commentStart">コメント開始位置です。</param>
    /// <returns>スタンドアロンコメントの場合は <see langword="true" /> を返します。</returns>
    private static bool IsStandaloneComment(SourceText sourceText, int commentStart)
    {
        TextLine line = sourceText.Lines.GetLineFromPosition(commentStart);
        if (commentStart <= line.Start)
        {
            return true;
        }

        string beforeComment = sourceText.ToString(TextSpan.FromBounds(line.Start, commentStart));
        return string.IsNullOrWhiteSpace(beforeComment);
    }

    /// <summary>
    /// 対象行が空行かどうか判定します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="lineNumber">行番号です。</param>
    /// <returns>空行の場合は <see langword="true" /> を返します。</returns>
    private static bool IsBlankLine(SourceText sourceText, int lineNumber)
    {
        return string.IsNullOrWhiteSpace(sourceText.Lines[lineNumber].ToString());
    }
}
