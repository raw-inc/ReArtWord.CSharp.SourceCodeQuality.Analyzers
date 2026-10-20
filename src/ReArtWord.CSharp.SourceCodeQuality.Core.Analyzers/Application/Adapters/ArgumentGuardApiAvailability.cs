using Microsoft.CodeAnalysis;
using ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Application.Configuration;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers;

/// <summary>
/// 引数guard標準APIの利用可否です。
/// </summary>
internal sealed class ArgumentGuardApiAvailability
{
    private const string ArgumentExceptionTypeName = "System.ArgumentException";
    private const string ArgumentNullExceptionTypeName = "System.ArgumentNullException";
    private const string ArgumentOutOfRangeExceptionTypeName = "System.ArgumentOutOfRangeException";

    private readonly HashSet<string> argumentExceptionMethods;
    private readonly HashSet<string> outOfRangeMethods;

    private ArgumentGuardApiAvailability(
        bool supportsThrowIfNull,
        HashSet<string> argumentExceptionMethods,
        HashSet<string> outOfRangeMethods)
    {
        SupportsThrowIfNull = supportsThrowIfNull;
        this.argumentExceptionMethods = argumentExceptionMethods;
        this.outOfRangeMethods = outOfRangeMethods;
    }

    /// <summary>
    /// ArgumentNullException.ThrowIfNullを利用できるか取得します。
    /// </summary>
    public bool SupportsThrowIfNull { get; }

    /// <summary>
    /// いずれかのguard APIを利用できるか取得します。
    /// </summary>
    public bool SupportsAny => SupportsThrowIfNull || SupportsStringGuards || SupportsRangeGuards;

    /// <summary>
    /// 文字列guard APIを利用できるか取得します。
    /// </summary>
    public bool SupportsStringGuards => argumentExceptionMethods.Count > 0;

    /// <summary>
    /// 範囲guard APIを利用できるか取得します。
    /// </summary>
    public bool SupportsRangeGuards => outOfRangeMethods.Count > 0;

    /// <summary>
    /// 利用可否を作成します。
    /// </summary>
    /// <param name="compilation">コンパイル情報です。</param>
    /// <returns>利用可否です。</returns>
    public static ArgumentGuardApiAvailability Create(Compilation compilation)
    {
        bool supportsThrowIfNull = TargetFrameworkVersionDetector.IsNet6OrGreater(compilation)
            && HasStaticMethod(compilation, ArgumentNullExceptionTypeName, "ThrowIfNull");

        HashSet<string> argumentExceptionMethods = TargetFrameworkVersionDetector.IsNet7OrGreater(compilation)
            ? GetAvailableMethods(compilation, ArgumentExceptionTypeName, "ThrowIfNullOrEmpty", "ThrowIfNullOrWhiteSpace")
            : new HashSet<string>(StringComparer.Ordinal);

        HashSet<string> outOfRangeMethods = TargetFrameworkVersionDetector.IsNet7OrGreater(compilation)
            ? GetAvailableMethods(
                compilation,
                ArgumentOutOfRangeExceptionTypeName,
                "ThrowIfEqual",
                "ThrowIfGreaterThan",
                "ThrowIfGreaterThanOrEqual",
                "ThrowIfLessThan",
                "ThrowIfLessThanOrEqual",
                "ThrowIfNegative",
                "ThrowIfNegativeOrZero",
                "ThrowIfNotEqual",
                "ThrowIfZero")
            : new HashSet<string>(StringComparer.Ordinal);

        return new ArgumentGuardApiAvailability(supportsThrowIfNull, argumentExceptionMethods, outOfRangeMethods);
    }

    /// <summary>
    /// ArgumentExceptionの指定メソッドが利用可能か判定します。
    /// </summary>
    /// <param name="methodName">メソッド名です。</param>
    /// <returns>利用可能な場合は <see langword="true" /> を返します。</returns>
    public bool HasArgumentExceptionMethod(string methodName)
    {
        return argumentExceptionMethods.Contains(methodName);
    }

    /// <summary>
    /// ArgumentOutOfRangeExceptionの指定メソッドが利用可能か判定します。
    /// </summary>
    /// <param name="methodName">メソッド名です。</param>
    /// <returns>利用可能な場合は <see langword="true" /> を返します。</returns>
    public bool HasOutOfRangeMethod(string methodName)
    {
        return outOfRangeMethods.Contains(methodName);
    }

    private static HashSet<string> GetAvailableMethods(Compilation compilation, string metadataName, params string[] methodNames)
    {
        HashSet<string> availableMethods = new(StringComparer.Ordinal);
        foreach (string methodName in methodNames)
        {
            if (HasStaticMethod(compilation, metadataName, methodName))
            {
                availableMethods.Add(methodName);
            }
        }

        return availableMethods;
    }

    private static bool HasStaticMethod(Compilation compilation, string metadataName, string methodName)
    {
        INamedTypeSymbol? typeSymbol = compilation.GetTypeByMetadataName(metadataName);
        return typeSymbol?.GetMembers(methodName).OfType<IMethodSymbol>().Any(static method => method.IsStatic) == true;
    }
}
