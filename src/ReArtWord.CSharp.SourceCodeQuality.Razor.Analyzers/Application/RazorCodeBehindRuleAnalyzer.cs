using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Razor.Analyzers.Application;

/// <summary>
/// Razor に記述されたメンバー宣言を検証します。
/// </summary>
internal static class RazorCodeBehindRuleAnalyzer
{
    /// <summary>
    /// 元の画面に行マッピングされたメンバーの識別子へ診断します。
    /// </summary>
    /// <param name="context">構文ノードの解析コンテキストです。</param>
    public static void AnalyzeMember(SyntaxNodeAnalysisContext context)
    {
        // @inject は型名と識別子だけが Razor に対応し、宣言の先頭は生成コードに残ります。
        SyntaxNodeOrToken declarationStart = context.Node.ChildNodesAndTokens()
            .FirstOrDefault(part => part.AsNode() is not AttributeListSyntax);
        Location? declarationLocation = declarationStart.GetLocation();
        string razorExtension = ".razor";
        if (declarationLocation is null
            || !declarationLocation.GetMappedLineSpan().Path.EndsWith(razorExtension, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        SyntaxToken identifier = context.Node switch
        {
            MethodDeclarationSyntax method => method.Identifier,
            PropertyDeclarationSyntax property => property.Identifier,
            FieldDeclarationSyntax field => field.Declaration.Variables.FirstOrDefault()?.Identifier ?? default,
            EventDeclarationSyntax eventDeclaration => eventDeclaration.Identifier,
            EventFieldDeclarationSyntax eventField => eventField.Declaration.Variables.FirstOrDefault()?.Identifier ?? default,
            ConstructorDeclarationSyntax constructor => constructor.Identifier,
            TypeDeclarationSyntax type => type.Identifier,
            _ => default,
        };

        if (identifier.RawKind == 0 || identifier.IsMissing)
        {
            return;
        }

        Location location = identifier.GetLocation();
        if (location.GetMappedLineSpan().Path.EndsWith(razorExtension, StringComparison.OrdinalIgnoreCase))
        {
            context.ReportDiagnostic(Diagnostic.Create(RazorDiagnosticCatalog.CodeBehindMemberRule, location));
        }
    }
}
