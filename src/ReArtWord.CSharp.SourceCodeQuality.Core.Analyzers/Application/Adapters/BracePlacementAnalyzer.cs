using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// ブロック開き波括弧の配置ルールを検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BracePlacementAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.BlockOpenBraceNewLineRule);

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
        context.RegisterSyntaxNodeAction(
            AnalyzeOpenBracePlacement,
            SyntaxKind.Block,
            SyntaxKind.SwitchStatement,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.NamespaceDeclaration,
            SyntaxKind.AccessorList);
    }

    /// <summary>
    /// 開き波括弧の配置を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeOpenBracePlacement(SyntaxNodeAnalysisContext context)
    {
        SyntaxToken openBraceToken = GetOpenBraceToken(context.Node);
        if (!openBraceToken.IsKind(SyntaxKind.OpenBraceToken) || openBraceToken.IsMissing)
        {
            return;
        }

        SyntaxTree? syntaxTree = openBraceToken.SyntaxTree;
        if (syntaxTree is null)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(syntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (!HasNonWhitespaceBeforeOpenBraceOnLine(openBraceToken, context.CancellationToken))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.BlockOpenBraceNewLineRule,
                openBraceToken.GetLocation()));
    }

    /// <summary>
    /// 対象構文の開き波括弧トークンを取得します。
    /// </summary>
    /// <param name="node">解析対象ノードです。</param>
    /// <returns>開き波括弧トークンです。</returns>
    private static SyntaxToken GetOpenBraceToken(SyntaxNode node)
    {
        return node switch
        {
            BlockSyntax block => block.OpenBraceToken,
            SwitchStatementSyntax switchStatement => switchStatement.OpenBraceToken,
            BaseTypeDeclarationSyntax typeDeclaration => typeDeclaration.OpenBraceToken,
            NamespaceDeclarationSyntax namespaceDeclaration => namespaceDeclaration.OpenBraceToken,
            AccessorListSyntax accessorList => accessorList.OpenBraceToken,
            _ => default,
        };
    }

    /// <summary>
    /// 開き波括弧の同一行前方に空白以外の文字があるか判定します。
    /// </summary>
    /// <param name="openBraceToken">開き波括弧トークンです。</param>
    /// <param name="cancellationToken">キャンセル通知トークンです。</param>
    /// <returns>空白以外の文字がある場合は <see langword="true" /> を返します。</returns>
    private static bool HasNonWhitespaceBeforeOpenBraceOnLine(
        SyntaxToken openBraceToken,
        CancellationToken cancellationToken)
    {
        SyntaxTree? syntaxTree = openBraceToken.SyntaxTree;
        if (syntaxTree is null)
        {
            return false;
        }

        SourceText sourceText = syntaxTree.GetText(cancellationToken);
        TextLine line = sourceText.Lines.GetLineFromPosition(openBraceToken.SpanStart);

        for (int position = line.Start; position < openBraceToken.SpanStart; position++)
        {
            if (!char.IsWhiteSpace(sourceText[position]))
            {
                return true;
            }
        }

        return false;
    }
}
