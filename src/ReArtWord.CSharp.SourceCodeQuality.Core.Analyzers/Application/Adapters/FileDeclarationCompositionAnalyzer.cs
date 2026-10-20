using System.Collections.Concurrent;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Filtering;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Contracts.Diagnostics;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// ファイル内の型宣言構成（class/struct/record）を検証するAnalyzerです。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class FileDeclarationCompositionAnalyzer : DiagnosticAnalyzer
{
    /// <summary>
    /// サポートする診断定義を取得します。
    /// </summary>
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(
            DiagnosticCatalog.FileClassStructDeclarationLimitRule,
            DiagnosticCatalog.RecordOnlyFileDeclarationLimitRule,
            DiagnosticCatalog.MixedFileRecordExternalReferenceRule);

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
        context.RegisterCompilationStartAction(InitializeCompilationAnalysis);
    }

    /// <summary>
    /// コンパイル開始時に解析処理を登録します。
    /// </summary>
    /// <param name="context">コンパイル開始コンテキストです。</param>
    private static void InitializeCompilationAnalysis(CompilationStartAnalysisContext context)
    {
        ConcurrentDictionary<SyntaxTree, FileCompositionSnapshot> compositionSnapshots = new();
        ConcurrentDictionary<INamedTypeSymbol, MixedRecordInfo> mixedRecordTargets =
            new(SymbolEqualityComparer.Default);
        ConcurrentBag<RecordReferenceInfo> recordReferences = [];

        context.RegisterSyntaxNodeAction(
            nodeContext => AnalyzeCompilationUnit(nodeContext, compositionSnapshots),
            SyntaxKind.CompilationUnit);

        context.RegisterSymbolAction(
            symbolContext => CollectMixedRecordTarget(symbolContext, compositionSnapshots, mixedRecordTargets),
            SymbolKind.NamedType);

        context.RegisterSyntaxNodeAction(
            nodeContext => CollectRecordReference(nodeContext, recordReferences),
            SyntaxKind.IdentifierName);

        context.RegisterCompilationEndAction(endContext =>
            ReportMixedRecordExternalReferenceViolations(endContext, mixedRecordTargets, recordReferences));
    }

    /// <summary>
    /// CompilationUnit単位で宣言数制約を検証します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="compositionSnapshots">ファイル構成スナップショットのキャッシュです。</param>
    private static void AnalyzeCompilationUnit(
        SyntaxNodeAnalysisContext context,
        ConcurrentDictionary<SyntaxTree, FileCompositionSnapshot> compositionSnapshots)
    {
        if (context.Node is not CompilationUnitSyntax compilationUnit)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(compilationUnit.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        FileCompositionSnapshot snapshot = FileDeclarationCompositionAnalyzerHelper.GetOrCreateCompositionSnapshot(
            compilationUnit.SyntaxTree,
            compositionSnapshots,
            context.CancellationToken);

        if (snapshot.ClassOrStructCount > 1 && snapshot.ClassStructViolationLocation is not null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.FileClassStructDeclarationLimitRule,
                    snapshot.ClassStructViolationLocation,
                    snapshot.FileName,
                    snapshot.ClassOrStructCount));
        }

        if (snapshot.ClassOrStructCount == 0
            && snapshot.RecordCount > 1
            && snapshot.RecordOnlyViolationLocation is not null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.RecordOnlyFileDeclarationLimitRule,
                    snapshot.RecordOnlyViolationLocation,
                    snapshot.FileName,
                    snapshot.RecordCount));
        }
    }

    /// <summary>
    /// mixed file内recordを外部参照禁止対象として収集します。
    /// </summary>
    /// <param name="context">シンボル解析コンテキストです。</param>
    /// <param name="compositionSnapshots">ファイル構成スナップショットのキャッシュです。</param>
    /// <param name="mixedRecordTargets">収集先です。</param>
    private static void CollectMixedRecordTarget(
        SymbolAnalysisContext context,
        ConcurrentDictionary<SyntaxTree, FileCompositionSnapshot> compositionSnapshots,
        ConcurrentDictionary<INamedTypeSymbol, MixedRecordInfo> mixedRecordTargets)
    {
        if (context.Symbol is not INamedTypeSymbol typeSymbol || typeSymbol.IsImplicitlyDeclared)
        {
            return;
        }

        foreach (SyntaxReference syntaxReference in typeSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxReference.GetSyntax(context.CancellationToken) is not RecordDeclarationSyntax recordDeclaration)
            {
                continue;
            }

            SyntaxTree syntaxTree = recordDeclaration.SyntaxTree;
            if (!ProductionCodeFilter.IsProductionCode(syntaxTree, context.Compilation))
            {
                continue;
            }

            FileCompositionSnapshot snapshot = FileDeclarationCompositionAnalyzerHelper.GetOrCreateCompositionSnapshot(
                syntaxTree,
                compositionSnapshots,
                context.CancellationToken);

            if (!snapshot.IsMixedFile)
            {
                continue;
            }

            mixedRecordTargets.TryAdd(
                typeSymbol.OriginalDefinition,
                new MixedRecordInfo(
                    typeSymbol.OriginalDefinition,
                    recordDeclaration.Identifier.GetLocation(),
                    syntaxTree,
                    recordDeclaration.Identifier.Text));
            break;
        }
    }

    /// <summary>
    /// record参照候補を収集します。
    /// </summary>
    /// <param name="context">構文ノード解析コンテキストです。</param>
    /// <param name="recordReferences">収集先です。</param>
    private static void CollectRecordReference(
        SyntaxNodeAnalysisContext context,
        ConcurrentBag<RecordReferenceInfo> recordReferences)
    {
        if (context.Node is not NameSyntax nameSyntax)
        {
            return;
        }

        if (!ProductionCodeFilter.IsProductionCode(nameSyntax.SyntaxTree, context.SemanticModel.Compilation))
        {
            return;
        }

        INamedTypeSymbol? namedTypeSymbol =
            FileDeclarationCompositionAnalyzerHelper.TryGetReferencedNamedTypeSymbol(context, nameSyntax);
        if (namedTypeSymbol is null)
        {
            return;
        }

        recordReferences.Add(new RecordReferenceInfo(namedTypeSymbol.OriginalDefinition, nameSyntax.SyntaxTree));
    }

    /// <summary>
    /// mixed file内recordの外部参照違反を報告します。
    /// </summary>
    /// <param name="context">コンパイル終了コンテキストです。</param>
    /// <param name="mixedRecordTargets">mixed file内recordの対象集合です。</param>
    /// <param name="recordReferences">収集済み参照情報です。</param>
    private static void ReportMixedRecordExternalReferenceViolations(
        CompilationAnalysisContext context,
        IReadOnlyDictionary<INamedTypeSymbol, MixedRecordInfo> mixedRecordTargets,
        IEnumerable<RecordReferenceInfo> recordReferences)
    {
        if (mixedRecordTargets.Count == 0)
        {
            return;
        }

        foreach (RecordReferenceInfo reference in recordReferences)
        {
            if (!mixedRecordTargets.TryGetValue(reference.Symbol, out MixedRecordInfo? target))
            {
                continue;
            }

            if (ReferenceEquals(target.SyntaxTree, reference.ReferenceSyntaxTree))
            {
                continue;
            }

            target.HasExternalReference = true;
        }

        foreach (MixedRecordInfo target in mixedRecordTargets.Values)
        {
            if (!target.HasExternalReference)
            {
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticCatalog.MixedFileRecordExternalReferenceRule,
                    target.ReportLocation,
                    target.RecordName));
        }
    }
}
