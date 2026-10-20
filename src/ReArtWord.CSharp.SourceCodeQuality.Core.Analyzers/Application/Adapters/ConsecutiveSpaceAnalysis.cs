using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 隣接トークン間の連続Spaceを解析します。
/// </summary>
internal static class ConsecutiveSpaceAnalysis
{
    /// <summary>
    /// 隣接トークン間の連続Spaceを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="compilationUnit">コンパイル単位です。</param>
    /// <param name="sourceText">ソーステキストです。</param>
    public static void Analyze(
        SyntaxNodeAnalysisContext context,
        CompilationUnitSyntax compilationUnit,
        SourceText sourceText)
    {
        SyntaxToken previousToken = default;
        foreach (SyntaxToken currentToken in compilationUnit.DescendantTokens(descendIntoTrivia: false))
        {
            if (SpacingSyntaxHelper.IsMissingOrEndOfFile(currentToken))
            {
                continue;
            }

            if (!SpacingSyntaxHelper.IsMissingOrEndOfFile(previousToken))
            {
                AnalyzePair(context, sourceText, previousToken, currentToken);
            }

            previousToken = currentToken;
        }
    }

    /// <summary>
    /// 2トークン間の連続Spaceを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="previousToken">前トークンです。</param>
    /// <param name="currentToken">現在トークンです。</param>
    private static void AnalyzePair(
        SyntaxNodeAnalysisContext context,
        SourceText sourceText,
        SyntaxToken previousToken,
        SyntaxToken currentToken)
    {
        int gapStart = previousToken.Span.End;
        int gapEnd = currentToken.SpanStart;
        if (gapEnd <= gapStart || !SpacingSyntaxHelper.IsSameLine(sourceText, gapStart, gapEnd))
        {
            return;
        }

        string gapText = sourceText.ToString(TextSpan.FromBounds(gapStart, gapEnd));
        int doubleSpaceIndex = gapText.IndexOf("  ", StringComparison.Ordinal);
        if (doubleSpaceIndex < 0 || gapText.IndexOf('\t') >= 0 || !SpacingSyntaxHelper.IsWhitespaceOnly(gapText))
        {
            return;
        }

        SyntaxTree? syntaxTree = previousToken.SyntaxTree;
        if (syntaxTree is null)
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.ConsecutiveSpaceProhibitedRule,
                Location.Create(syntaxTree, new TextSpan(gapStart + doubleSpaceIndex, 2))));
    }
}
