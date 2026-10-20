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
/// summary必須と日本語記述を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class SummaryDocumentationAnalyzer : DiagnosticAnalyzer
{
    private static readonly Regex JapaneseRegex = new(
        "[\\p{IsHiragana}\\p{IsKatakana}\\p{IsCJKUnifiedIdeographs}]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.SummaryRequiredRule, DiagnosticCatalog.SummaryJapaneseRule);

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
            AnalyzeTypeDeclaration,
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.RecordDeclaration,
            SyntaxKind.RecordStructDeclaration,
            SyntaxKind.EnumDeclaration);

        context.RegisterSyntaxNodeAction(
            AnalyzeMethodDeclaration,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration);
    }

    /// <summary>
    /// 型宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeTypeDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax declaration)
        {
            return;
        }

        EvaluateSummaryRules(context, declaration, "型");
    }

    /// <summary>
    /// メソッド宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MemberDeclarationSyntax declaration)
        {
            return;
        }

        EvaluateSummaryRules(context, declaration, "メソッド");
    }

    /// <summary>
    /// summary関連ルールを評価します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="declaration">宣言ノードです。</param>
    /// <param name="kindName">宣言種別名です。</param>
    private static void EvaluateSummaryRules(SyntaxNodeAnalysisContext context, MemberDeclarationSyntax declaration, string kindName)
    {
        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(declaration.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (HasGeneratedCodeAttribute(context, declaration))
        {
            return;
        }

        string symbolName = GetSymbolName(context, declaration);
        SummaryDocumentation summary = ExtractSummary(declaration);

        if (string.IsNullOrWhiteSpace(summary.Text))
        {
            Diagnostic requiredDiagnostic = Diagnostic.Create(
                DiagnosticCatalog.SummaryRequiredRule,
                GetDeclarationIdentifierLocation(declaration),
                kindName,
                symbolName);

            context.ReportDiagnostic(requiredDiagnostic);
            return;
        }

        if (JapaneseRegex.IsMatch(summary.Text))
        {
            return;
        }

        Diagnostic japaneseDiagnostic = Diagnostic.Create(
            DiagnosticCatalog.SummaryJapaneseRule,
            summary.TextLocation ?? summary.TagLocation ?? GetDeclarationIdentifierLocation(declaration),
            kindName,
            symbolName);

        context.ReportDiagnostic(japaneseDiagnostic);
    }

    /// <summary>
    /// 宣言へ生成コード属性が付与されているか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="declaration">宣言ノードです。</param>
    /// <returns>生成コード属性付きの場合は <see langword="true" /> を返します。</returns>
    private static bool HasGeneratedCodeAttribute(SyntaxNodeAnalysisContext context, MemberDeclarationSyntax declaration)
    {
        ISymbol? symbol = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);
        if (symbol is null)
        {
            return false;
        }

        return symbol.GetAttributes().Any(attribute =>
            attribute.AttributeClass?.ToDisplayString().Contains("GeneratedCode", StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// 宣言の表示名を取得します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="declaration">宣言ノードです。</param>
    /// <returns>診断表示用の宣言名です。</returns>
    private static string GetSymbolName(SyntaxNodeAnalysisContext context, MemberDeclarationSyntax declaration)
    {
        ISymbol? symbol = context.SemanticModel.GetDeclaredSymbol(declaration, context.CancellationToken);
        return symbol?.Name ?? "不明";
    }

    /// <summary>
    /// summary要素の本文を抽出します。
    /// </summary>
    /// <param name="declaration">宣言ノードです。</param>
    /// <returns>summary本文です。存在しない場合は <see langword="null" /> を返します。</returns>
    private static SummaryDocumentation ExtractSummary(MemberDeclarationSyntax declaration)
    {
        foreach (SyntaxTrivia trivia in declaration.GetLeadingTrivia())
        {
            if (trivia.GetStructure() is not DocumentationCommentTriviaSyntax commentTrivia)
            {
                continue;
            }

            foreach (XmlElementSyntax element in commentTrivia.Content.OfType<XmlElementSyntax>())
            {
                if (!IsSummaryElement(element))
                {
                    continue;
                }

                return new SummaryDocumentation(
                    ExtractText(element).Trim(),
                    GetFirstSummaryTextLocation(element),
                    element.StartTag.Name.GetLocation());
            }
        }

        return default;
    }

    /// <summary>
    /// 宣言識別子の診断位置を取得します。
    /// </summary>
    /// <param name="declaration">宣言ノードです。</param>
    /// <returns>診断位置です。</returns>
    private static Location GetDeclarationIdentifierLocation(MemberDeclarationSyntax declaration)
    {
        return declaration switch
        {
            BaseTypeDeclarationSyntax typeDeclaration => typeDeclaration.Identifier.GetLocation(),
            MethodDeclarationSyntax methodDeclaration => methodDeclaration.Identifier.GetLocation(),
            ConstructorDeclarationSyntax constructorDeclaration => constructorDeclaration.Identifier.GetLocation(),
            _ => declaration.GetLocation(),
        };
    }

    /// <summary>
    /// summary要素かどうかを判定します。
    /// </summary>
    /// <param name="element">XML要素です。</param>
    /// <returns>summary要素の場合は <see langword="true" /> を返します。</returns>
    private static bool IsSummaryElement(XmlElementSyntax element)
    {
        return element.StartTag.Name.LocalName.Text.Equals("summary", StringComparison.Ordinal);
    }

    /// <summary>
    /// XML要素本文を文字列化します。
    /// </summary>
    /// <param name="element">対象要素です。</param>
    /// <returns>文字列化結果です。</returns>
    private static string ExtractText(XmlElementSyntax element)
    {
        StringBuilder builder = new();

        foreach (XmlNodeSyntax node in element.Content)
        {
            if (node is XmlTextSyntax textNode)
            {
                foreach (SyntaxToken token in textNode.TextTokens)
                {
                    builder.Append(token.ValueText);
                }
            }
            else if (node is XmlCDataSectionSyntax cdata)
            {
                builder.Append(cdata.TextTokens.ToFullString());
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// summary本文の先頭非空白テキスト位置を取得します。
    /// </summary>
    /// <param name="element">summary要素です。</param>
    /// <returns>本文位置です。</returns>
    private static Location? GetFirstSummaryTextLocation(XmlElementSyntax element)
    {
        foreach (XmlTextSyntax textNode in element.Content.OfType<XmlTextSyntax>())
        {
            foreach (SyntaxToken token in textNode.TextTokens)
            {
                string tokenText = token.Text;
                for (int index = 0; index < tokenText.Length; index++)
                {
                    if (char.IsWhiteSpace(tokenText[index]))
                    {
                        continue;
                    }

                    int start = token.SpanStart + index;
                    return Location.Create(element.SyntaxTree, new TextSpan(start, token.Span.End - start));
                }
            }
        }

        return null;
    }

    /// <summary>
    /// summary情報です。
    /// </summary>
    private readonly struct SummaryDocumentation
    {
        /// <summary>
        /// summary情報を初期化します。
        /// </summary>
        /// <param name="text">summary本文です。</param>
        /// <param name="textLocation">本文位置です。</param>
        /// <param name="tagLocation">summaryタグ位置です。</param>
        public SummaryDocumentation(string text, Location? textLocation, Location? tagLocation)
        {
            Text = text;
            TextLocation = textLocation;
            TagLocation = tagLocation;
        }

        /// <summary>
        /// summary本文を取得します。
        /// </summary>
        public string? Text { get; }

        /// <summary>
        /// 本文位置を取得します。
        /// </summary>
        public Location? TextLocation { get; }

        /// <summary>
        /// summaryタグ位置を取得します。
        /// </summary>
        public Location? TagLocation { get; }
    }
}
