using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// クラス内メンバー終端とAttribute list境界の空行ルールを検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class MemberSpacingAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.MemberSpacingRule);

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
        context.RegisterSyntaxNodeAction(AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        context.RegisterSyntaxNodeAction(AttributeListSpacingAnalysis.Analyze, SyntaxKind.AttributeList);
    }

    /// <summary>
    /// クラス宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not ClassDeclarationSyntax classDeclaration)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(classDeclaration.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        ImmutableArray<TargetMember> targetMembers = CollectTargetMembers(classDeclaration);
        if (targetMembers.IsEmpty)
        {
            return;
        }

        SourceText sourceText = classDeclaration.SyntaxTree.GetText(context.CancellationToken);
        foreach (TargetMember targetMember in targetMembers)
        {
            if (targetMember.Index >= classDeclaration.Members.Count - 1)
            {
                continue;
            }

            MemberDeclarationSyntax nextMember = classDeclaration.Members[targetMember.Index + 1];
            if (!TryGetAnchorLine(sourceText, targetMember.CloseBraceToken, nextMember, out int anchorLine))
            {
                continue;
            }

            int closeBraceLine = sourceText.Lines.GetLineFromPosition(targetMember.CloseBraceToken.SpanStart).LineNumber;
            int blankLineCount = CountBlankLinesBetween(sourceText, closeBraceLine, anchorLine);
            if (blankLineCount == 1)
            {
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.MemberSpacingRule,
                    CreateSpacingDiagnosticLocation(
                        sourceText,
                        classDeclaration.SyntaxTree,
                        closeBraceLine,
                        anchorLine,
                        blankLineCount),
                    DiagnosticCatalog.MemberSpacingMessage));
        }
    }

    /// <summary>
    /// メンバー間空行違反の診断位置を生成します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="syntaxTree">構文ツリーです。</param>
    /// <param name="fromLine">開始行番号です。</param>
    /// <param name="toLine">終了行番号です。</param>
    /// <param name="blankLineCount">空行数です。</param>
    /// <returns>診断位置です。</returns>
    private static Location CreateSpacingDiagnosticLocation(
        SourceText sourceText,
        SyntaxTree syntaxTree,
        int fromLine,
        int toLine,
        int blankLineCount)
    {
        int targetLineNumber = blankLineCount == 0
            ? toLine
            : GetSecondBlankLineNumber(sourceText, fromLine, toLine) ?? toLine;

        int targetPosition = sourceText.Lines[targetLineNumber].Start;
        return Location.Create(syntaxTree, new TextSpan(targetPosition, 0));
    }

    /// <summary>
    /// 2つ目の空行番号を取得します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="fromLine">開始行番号です。</param>
    /// <param name="toLine">終了行番号です。</param>
    /// <returns>2つ目の空行番号です。</returns>
    private static int? GetSecondBlankLineNumber(SourceText sourceText, int fromLine, int toLine)
    {
        int blankLineCount = 0;
        for (int line = fromLine + 1; line < toLine; line++)
        {
            if (!string.IsNullOrWhiteSpace(sourceText.Lines[line].ToString()))
            {
                continue;
            }

            blankLineCount++;
            if (blankLineCount == 2)
            {
                return line;
            }
        }

        return null;
    }

    /// <summary>
    /// 解析対象メンバーを収集します。
    /// </summary>
    /// <param name="classDeclaration">クラス宣言です。</param>
    /// <returns>対象メンバー一覧です。</returns>
    private static ImmutableArray<TargetMember> CollectTargetMembers(ClassDeclarationSyntax classDeclaration)
    {
        ImmutableArray<TargetMember>.Builder builder = ImmutableArray.CreateBuilder<TargetMember>();
        for (int index = 0; index < classDeclaration.Members.Count; index++)
        {
            MemberDeclarationSyntax member = classDeclaration.Members[index];
            if (!TryGetCloseBraceToken(member, out SyntaxToken closeBraceToken))
            {
                continue;
            }

            builder.Add(new TargetMember(index, closeBraceToken));
        }

        return builder.ToImmutable();
    }

    /// <summary>
    /// メンバー終端の閉じ波括弧トークンを取得します。
    /// </summary>
    /// <param name="member">対象メンバーです。</param>
    /// <param name="closeBraceToken">閉じ波括弧トークンです。</param>
    /// <returns>取得成功時は <see langword="true" /> を返します。</returns>
    private static bool TryGetCloseBraceToken(MemberDeclarationSyntax member, out SyntaxToken closeBraceToken)
    {
        closeBraceToken = default;

        switch (member)
        {
            case MethodDeclarationSyntax { Body: not null } methodDeclaration:
                closeBraceToken = methodDeclaration.Body.CloseBraceToken;
                return true;

            case ConstructorDeclarationSyntax { Body: not null } constructorDeclaration:
                closeBraceToken = constructorDeclaration.Body.CloseBraceToken;
                return true;

            case PropertyDeclarationSyntax { AccessorList: not null } propertyDeclaration:
                closeBraceToken = propertyDeclaration.AccessorList.CloseBraceToken;
                return true;

            default:
                return false;
        }
    }

    /// <summary>
    /// 比較対象となる次要素の行番号を取得します。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="closeBraceToken">現在メンバーの終端括弧です。</param>
    /// <param name="nextMember">次メンバーです。</param>
    /// <param name="anchorLine">比較対象行番号です。</param>
    /// <returns>取得成功時は <see langword="true" /> を返します。</returns>
    private static bool TryGetAnchorLine(
        SourceText sourceText,
        SyntaxToken closeBraceToken,
        MemberDeclarationSyntax nextMember,
        out int anchorLine)
    {
        anchorLine = 0;

        int closeBraceEnd = closeBraceToken.Span.End;
        int? commentStart = GetLeadingCommentStart(nextMember, closeBraceEnd);
        if (commentStart.HasValue)
        {
            anchorLine = sourceText.Lines.GetLineFromPosition(commentStart.Value).LineNumber;
            return true;
        }

        SyntaxToken nextToken = nextMember.GetFirstToken();
        if (nextToken == default)
        {
            return false;
        }

        anchorLine = sourceText.Lines.GetLineFromPosition(nextToken.SpanStart).LineNumber;
        return true;
    }

    /// <summary>
    /// 次メンバー先頭のコメント開始位置を取得します。
    /// </summary>
    /// <param name="nextMember">次メンバーです。</param>
    /// <param name="minimumPosition">探索下限位置です。</param>
    /// <returns>開始位置です。存在しない場合は <see langword="null" /> を返します。</returns>
    private static int? GetLeadingCommentStart(MemberDeclarationSyntax nextMember, int minimumPosition)
    {
        foreach (SyntaxTrivia trivia in nextMember.GetLeadingTrivia())
        {
            if (trivia.SpanStart < minimumPosition)
            {
                continue;
            }

            if (!IsCommentTrivia(trivia))
            {
                continue;
            }

            return trivia.SpanStart;
        }

        return null;
    }

    /// <summary>
    /// コメントTriviaか判定します。
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
    /// 2行の間に存在する空行数を数えます。
    /// </summary>
    /// <param name="sourceText">ソーステキストです。</param>
    /// <param name="fromLine">開始行番号（終端括弧行）です。</param>
    /// <param name="toLine">終了行番号（コメントまたは宣言行）です。</param>
    /// <returns>空行数です。</returns>
    private static int CountBlankLinesBetween(SourceText sourceText, int fromLine, int toLine)
    {
        if (toLine <= fromLine)
        {
            return 0;
        }

        int blankLineCount = 0;
        for (int line = fromLine + 1; line < toLine; line++)
        {
            if (!string.IsNullOrWhiteSpace(sourceText.Lines[line].ToString()))
            {
                continue;
            }

            blankLineCount++;
        }

        return blankLineCount;
    }

    /// <summary>
    /// 対象メンバー情報です。
    /// </summary>
    private readonly struct TargetMember
    {
        /// <summary>
        /// 対象メンバー情報を初期化します。
        /// </summary>
        /// <param name="index">クラス内インデックスです。</param>
        /// <param name="closeBraceToken">終端括弧トークンです。</param>
        public TargetMember(int index, SyntaxToken closeBraceToken)
        {
            Index = index;
            CloseBraceToken = closeBraceToken;
        }

        /// <summary>
        /// クラス内インデックスを取得します。
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 終端括弧トークンを取得します。
        /// </summary>
        public SyntaxToken CloseBraceToken { get; }
    }
}
