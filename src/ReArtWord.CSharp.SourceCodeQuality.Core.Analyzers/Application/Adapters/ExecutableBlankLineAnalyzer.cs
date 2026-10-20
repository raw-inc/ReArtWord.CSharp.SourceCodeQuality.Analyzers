using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 実行ブロック内の連続空行を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExecutableBlankLineAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.ExecutableBlankLineRule);

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
        context.RegisterSyntaxNodeAction(AnalyzeBlock, SyntaxKind.Block);
    }

    /// <summary>
    /// ブロックを解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeBlock(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not BlockSyntax block)
        {
            return;
        }

        if (!IsExecutableRootBlock(block))
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(block.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        SourceText sourceText = block.SyntaxTree.GetText(context.CancellationToken);
        List<LineRange> excludedRanges = CollectNestedExecutableBlockRanges(block, sourceText);
        AnalyzeBlankLines(context, sourceText, block, excludedRanges);
    }

    /// <summary>
    /// 連続空行を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="block">解析対象ブロックです。</param>
    /// <param name="excludedRanges">入れ子実行ブロックの除外行範囲です。</param>
    private static void AnalyzeBlankLines(
        SyntaxNodeAnalysisContext context,
        SourceText sourceText,
        BlockSyntax block,
        List<LineRange> excludedRanges)
    {
        int startLine = sourceText.Lines.GetLineFromPosition(block.OpenBraceToken.SpanStart).LineNumber + 1;
        int endLine = sourceText.Lines.GetLineFromPosition(block.CloseBraceToken.SpanStart).LineNumber - 1;
        if (endLine < startLine)
        {
            return;
        }

        int blankLineStreak = 0;
        for (int lineNumber = startLine; lineNumber <= endLine; lineNumber++)
        {
            if (IsExcludedLine(excludedRanges, lineNumber))
            {
                blankLineStreak = 0;
                continue;
            }

            if (!IsBlankLine(sourceText, lineNumber))
            {
                blankLineStreak = 0;
                continue;
            }

            blankLineStreak++;
            if (blankLineStreak != 2)
            {
                continue;
            }

            TextLine line = sourceText.Lines[lineNumber];
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.ExecutableBlankLineRule,
                    Location.Create(block.SyntaxTree, new TextSpan(line.Start, 0))));
        }
    }

    /// <summary>
    /// 入れ子の実行ブロック行範囲を収集します。
    /// </summary>
    /// <param name="block">親ブロックです。</param>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <returns>除外行範囲一覧です。</returns>
    private static List<LineRange> CollectNestedExecutableBlockRanges(
        BlockSyntax block,
        SourceText sourceText)
    {
        List<LineRange> ranges = [];
        foreach (BlockSyntax nestedBlock in block.DescendantNodes().OfType<BlockSyntax>())
        {
            if (!IsExecutableRootBlock(nestedBlock))
            {
                continue;
            }

            int startLine = sourceText.Lines.GetLineFromPosition(nestedBlock.OpenBraceToken.SpanStart).LineNumber;
            int endLine = sourceText.Lines.GetLineFromPosition(nestedBlock.CloseBraceToken.SpanStart).LineNumber;
            ranges.Add(new LineRange(startLine, endLine));
        }

        return ranges;
    }

    /// <summary>
    /// 実行単位の最上位ブロックか判定します。
    /// </summary>
    /// <param name="block">判定対象ブロックです。</param>
    /// <returns>実行単位の最上位ブロックの場合は <see langword="true" /> を返します。</returns>
    private static bool IsExecutableRootBlock(BlockSyntax block)
    {
        return block.Parent is BaseMethodDeclarationSyntax
            or AccessorDeclarationSyntax
            or LocalFunctionStatementSyntax
            or AnonymousFunctionExpressionSyntax;
    }

    /// <summary>
    /// 対象行が除外範囲内か判定します。
    /// </summary>
    /// <param name="ranges">除外行範囲一覧です。</param>
    /// <param name="lineNumber">行番号です。</param>
    /// <returns>除外範囲内の場合は <see langword="true" /> を返します。</returns>
    private static bool IsExcludedLine(List<LineRange> ranges, int lineNumber)
    {
        foreach (LineRange range in ranges)
        {
            if (range.Contains(lineNumber))
            {
                return true;
            }
        }

        return false;
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

    /// <summary>
    /// 行範囲です。
    /// </summary>
    private readonly struct LineRange
    {
        /// <summary>
        /// 行範囲を初期化します。
        /// </summary>
        /// <param name="startLine">開始行番号です。</param>
        /// <param name="endLine">終了行番号です。</param>
        public LineRange(int startLine, int endLine)
        {
            StartLine = startLine;
            EndLine = endLine;
        }

        /// <summary>
        /// 開始行番号を取得します。
        /// </summary>
        public int StartLine { get; }

        /// <summary>
        /// 終了行番号を取得します。
        /// </summary>
        public int EndLine { get; }

        /// <summary>
        /// 指定行が範囲内か判定します。
        /// </summary>
        /// <param name="lineNumber">行番号です。</param>
        /// <returns>範囲内の場合は <see langword="true" /> を返します。</returns>
        public bool Contains(int lineNumber)
        {
            return StartLine <= lineNumber && lineNumber <= EndLine;
        }
    }
}
