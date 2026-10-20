using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// <see cref="FileDeclarationCompositionAnalyzer" /> の解析補助ロジックです。
/// </summary>
internal static class FileDeclarationCompositionAnalyzerHelper
{
    /// <summary>
    /// 構文木のファイル構成スナップショットを取得します。
    /// </summary>
    /// <param name="syntaxTree">対象構文木です。</param>
    /// <param name="compositionSnapshots">スナップショットキャッシュです。</param>
    /// <param name="cancellationToken">キャンセルトークンです。</param>
    /// <returns>スナップショットです。</returns>
    public static FileCompositionSnapshot GetOrCreateCompositionSnapshot(
        SyntaxTree syntaxTree,
        ConcurrentDictionary<SyntaxTree, FileCompositionSnapshot> compositionSnapshots,
        CancellationToken cancellationToken)
    {
        if (compositionSnapshots.TryGetValue(syntaxTree, out FileCompositionSnapshot? snapshot))
        {
            return snapshot;
        }

        if (syntaxTree.GetRoot(cancellationToken) is not CompilationUnitSyntax root)
        {
            snapshot = new FileCompositionSnapshot(GetFileDisplayName(syntaxTree), 0, 0, null, null);
            compositionSnapshots.TryAdd(syntaxTree, snapshot);
            return snapshot;
        }

        List<MemberDeclarationSyntax> classOrStructDeclarations = [];
        List<RecordDeclarationSyntax> recordDeclarations = [];
        CollectTopLevelTypeDeclarations(root.Members, classOrStructDeclarations, recordDeclarations);

        snapshot = new FileCompositionSnapshot(
            GetFileDisplayName(syntaxTree),
            classOrStructDeclarations.Count,
            recordDeclarations.Count,
            classOrStructDeclarations.Count > 1 ? GetTypeIdentifierLocation(classOrStructDeclarations[1]) : null,
            classOrStructDeclarations.Count == 0 && recordDeclarations.Count > 1
                ? recordDeclarations[1].Identifier.GetLocation()
                : null);

        compositionSnapshots.TryAdd(syntaxTree, snapshot);
        return snapshot;
    }

    /// <summary>
    /// 名前構文から参照先の名前付き型シンボルを取得します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="nameSyntax">名前構文です。</param>
    /// <returns>取得した型シンボルです。取得できない場合は <see langword="null" /> を返します。</returns>
    public static INamedTypeSymbol? TryGetReferencedNamedTypeSymbol(
        SyntaxNodeAnalysisContext context,
        NameSyntax nameSyntax)
    {
        SymbolInfo symbolInfo = context.SemanticModel.GetSymbolInfo(nameSyntax, context.CancellationToken);
        ISymbol? symbol = symbolInfo.Symbol ?? symbolInfo.CandidateSymbols.FirstOrDefault();
        if (symbol is IAliasSymbol aliasSymbol)
        {
            symbol = aliasSymbol.Target;
        }

        return symbol as INamedTypeSymbol;
    }

    /// <summary>
    /// トップレベル型宣言を収集します。
    /// </summary>
    /// <param name="members">メンバー宣言です。</param>
    /// <param name="classOrStructDeclarations">class/struct宣言の出力先です。</param>
    /// <param name="recordDeclarations">record宣言の出力先です。</param>
    private static void CollectTopLevelTypeDeclarations(
        SyntaxList<MemberDeclarationSyntax> members,
        ICollection<MemberDeclarationSyntax> classOrStructDeclarations,
        ICollection<RecordDeclarationSyntax> recordDeclarations)
    {
        foreach (MemberDeclarationSyntax member in members)
        {
            switch (member)
            {
                case ClassDeclarationSyntax classDeclaration:
                    classOrStructDeclarations.Add(classDeclaration);
                    break;

                case StructDeclarationSyntax structDeclaration:
                    classOrStructDeclarations.Add(structDeclaration);
                    break;

                case RecordDeclarationSyntax recordDeclaration:
                    recordDeclarations.Add(recordDeclaration);
                    break;

                case BaseNamespaceDeclarationSyntax namespaceDeclaration:
                    CollectTopLevelTypeDeclarations(
                        namespaceDeclaration.Members,
                        classOrStructDeclarations,
                        recordDeclarations);
                    break;

                default:
                    break;
            }
        }
    }

    /// <summary>
    /// 型宣言の識別子位置を取得します。
    /// </summary>
    /// <param name="declaration">型宣言です。</param>
    /// <returns>識別子位置です。</returns>
    private static Location GetTypeIdentifierLocation(MemberDeclarationSyntax declaration)
    {
        return declaration switch
        {
            ClassDeclarationSyntax classDeclaration => classDeclaration.Identifier.GetLocation(),
            StructDeclarationSyntax structDeclaration => structDeclaration.Identifier.GetLocation(),
            _ => declaration.GetLocation(),
        };
    }

    /// <summary>
    /// ファイル表示名を取得します。
    /// </summary>
    /// <param name="syntaxTree">構文木です。</param>
    /// <returns>表示名です。</returns>
    private static string GetFileDisplayName(SyntaxTree syntaxTree)
    {
        string filePath = syntaxTree.FilePath;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return "(unknown)";
        }

        return Path.GetFileName(filePath);
    }
}
