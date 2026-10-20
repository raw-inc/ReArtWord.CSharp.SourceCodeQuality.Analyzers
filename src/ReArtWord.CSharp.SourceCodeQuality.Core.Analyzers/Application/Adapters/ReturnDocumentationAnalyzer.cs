using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 戻り値ドキュメントコメントを検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ReturnDocumentationAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(DiagnosticCatalog.ReturnDocumentationRequiredRule);

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
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    /// <summary>
    /// メソッド宣言を解析します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    private static void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not MethodDeclarationSyntax methodDeclaration)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionOrTargetTestCode(methodDeclaration.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        if (!HasReturnValue(context, methodDeclaration))
        {
            return;
        }

        if (HasNonEmptyReturnsElement(methodDeclaration))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticCatalog.ReturnDocumentationRequiredRule,
                methodDeclaration.Identifier.GetLocation(),
                methodDeclaration.Identifier.ValueText));
    }

    /// <summary>
    /// メソッドが戻り値を持つか判定します。
    /// </summary>
    /// <param name="context">解析コンテキストです。</param>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <returns>戻り値を持つ場合は <see langword="true" /> を返します。</returns>
    private static bool HasReturnValue(SyntaxNodeAnalysisContext context, MethodDeclarationSyntax methodDeclaration)
    {
        if (context.SemanticModel.GetDeclaredSymbol(methodDeclaration, context.CancellationToken) is IMethodSymbol methodSymbol)
        {
            return !methodSymbol.ReturnsVoid;
        }

        return !methodDeclaration.ReturnType.IsKind(SyntaxKind.PredefinedType)
            || methodDeclaration.ReturnType is not PredefinedTypeSyntax predefinedType
            || !predefinedType.Keyword.IsKind(SyntaxKind.VoidKeyword);
    }

    /// <summary>
    /// 非空のreturns要素があるか判定します。
    /// </summary>
    /// <param name="methodDeclaration">メソッド宣言です。</param>
    /// <returns>非空returnsが存在する場合は <see langword="true" /> を返します。</returns>
    private static bool HasNonEmptyReturnsElement(MethodDeclarationSyntax methodDeclaration)
    {
        foreach (SyntaxTrivia trivia in methodDeclaration.GetLeadingTrivia())
        {
            if (trivia.GetStructure() is not DocumentationCommentTriviaSyntax commentTrivia)
            {
                continue;
            }

            foreach (XmlElementSyntax element in commentTrivia.Content.OfType<XmlElementSyntax>())
            {
                if (!IsReturnsElement(element))
                {
                    continue;
                }

                return !string.IsNullOrWhiteSpace(ExtractText(element));
            }
        }

        return false;
    }

    /// <summary>
    /// returns要素かどうかを判定します。
    /// </summary>
    /// <param name="element">XML要素です。</param>
    /// <returns>returns要素の場合は <see langword="true" /> を返します。</returns>
    private static bool IsReturnsElement(XmlElementSyntax element)
    {
        return element.StartTag.Name.LocalName.Text.Equals("returns", StringComparison.Ordinal);
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
}
