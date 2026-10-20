using System.Globalization;

namespace ReArtWord.CSharp.SourceCodeQuality.Core.Analyzers.Tests.Diagnostics;

/// <summary>
/// 三項演算子Analyzerテスト用のソースを生成します。
/// </summary>
internal static class TernaryExpressionTestSourceFactory
{
    /// <summary>
    /// 偽側へネストする三項演算子ソースを生成します。
    /// </summary>
    /// <param name="conditionCount">条件数です。</param>
    /// <returns>生成したソースです。</returns>
    public static string BuildFalseNestedTernarySource(int conditionCount)
    {
        List<string> expressionLines = BuildFalseNestedTernaryLines(conditionCount);
        StringBuilder builder = new();
        builder.AppendLine("public sealed class Target");
        builder.AppendLine("{");
        builder.AppendLine("    public int Run()");
        builder.AppendLine("    {");
        foreach (string line in expressionLines)
        {
            builder.AppendLine(line);
        }

        builder.AppendLine("        return result;");
        builder.AppendLine("    }");
        AppendConditionMethods(builder, conditionCount);
        builder.AppendLine("}");
        return builder.ToString();
    }

    /// <summary>
    /// 依頼例に相当する整列済み三項演算子ソースを生成します。
    /// </summary>
    /// <returns>生成したソースです。</returns>
    public static string BuildRequestedExamplesSource()
    {
        StringBuilder builder = new();
        builder.AppendLine("public sealed class Target");
        builder.AppendLine("{");
        builder.AppendLine("    public bool Run1()");
        builder.AppendLine("    {");
        AppendAlignedSingleTernary(builder, "        bool a = IsA() ", "true", "false");
        builder.AppendLine("        return a;");
        builder.AppendLine("    }");
        builder.AppendLine("    public int Run2(int b, int c, int d)");
        builder.AppendLine("    {");
        AppendTrueNestedTernary(builder);
        builder.AppendLine("        return a;");
        builder.AppendLine("    }");
        builder.AppendLine("    public int Run3(int b, int c, int d)");
        builder.AppendLine("    {");
        AppendFalseNestedExampleTernary(builder);
        builder.AppendLine("        return a;");
        builder.AppendLine("    }");
        builder.AppendLine("    private bool IsA() => true;");
        builder.AppendLine("    private bool IsHogeCP(int value) => 0 < value;");
        builder.AppendLine("    private int CP(int value) => value;");
        builder.AppendLine("}");
        return builder.ToString();
    }

    /// <summary>
    /// コロン列がずれた三項演算子ソースを生成します。
    /// </summary>
    /// <returns>生成したソースです。</returns>
    public static string BuildMisalignedColonSource()
    {
        string firstLine = "        bool result = IsC1() ? true";
        int questionColumn = firstLine.IndexOf('?', StringComparison.Ordinal);
        return string.Join(
            Environment.NewLine,
            "public sealed class Target",
            "{",
            "    public bool Run()",
            "    {",
            firstLine,
            new string(' ', questionColumn - 1) + ": false;",
            "        return result;",
            "    }",
            "    private bool IsC1() => true;",
            "}");
    }

    /// <summary>
    /// コロン行の前方に式がある三項演算子ソースを生成します。
    /// </summary>
    /// <returns>生成したソースです。</returns>
    public static string BuildColonLinePrefixSource()
    {
        string firstLine = "        bool result = IsC1() ? true";
        int questionColumn = firstLine.IndexOf('?', StringComparison.Ordinal);
        return string.Join(
            Environment.NewLine,
            "public sealed class Target",
            "{",
            "    public bool Run()",
            "    {",
            firstLine,
            new string(' ', questionColumn) + "&& IsC2() : false;",
            "        return result;",
            "    }",
            "    private bool IsC1() => true;",
            "    private bool IsC2() => true;",
            "}");
    }

    /// <summary>
    /// 偽側へネストする三項演算子行を生成します。
    /// </summary>
    /// <param name="conditionCount">条件数です。</param>
    /// <returns>生成した行リストです。</returns>
    private static List<string> BuildFalseNestedTernaryLines(int conditionCount)
    {
        List<string> lines = [];
        string firstLine = "        int result = IsC1() ? 1";
        int currentQuestionColumn = firstLine.IndexOf('?', StringComparison.Ordinal);
        lines.Add(firstLine);

        for (int index = 2; index <= conditionCount; index++)
        {
            string prefix = new string(' ', currentQuestionColumn)
                + ": IsC"
                + index.ToString(CultureInfo.InvariantCulture)
                + "() ";
            lines.Add(prefix + "? " + index.ToString(CultureInfo.InvariantCulture));
            currentQuestionColumn = prefix.Length;
        }

        lines.Add(
            new string(' ', currentQuestionColumn)
            + ": "
            + (conditionCount + 1).ToString(CultureInfo.InvariantCulture)
            + ";");
        return lines;
    }

    /// <summary>
    /// 1条件の整列済み三項演算子を追加します。
    /// </summary>
    /// <param name="builder">追加先です。</param>
    /// <param name="prefix">疑問符前の文字列です。</param>
    /// <param name="whenTrue">真側式です。</param>
    /// <param name="whenFalse">偽側式です。</param>
    private static void AppendAlignedSingleTernary(StringBuilder builder, string prefix, string whenTrue, string whenFalse)
    {
        builder.AppendLine(prefix + "? " + whenTrue);
        builder.AppendLine(new string(' ', prefix.Length) + ": " + whenFalse + ";");
    }

    /// <summary>
    /// 真側ネストの三項演算子を追加します。
    /// </summary>
    /// <param name="builder">追加先です。</param>
    private static void AppendTrueNestedTernary(StringBuilder builder)
    {
        string prefix = "        var a = IsHogeCP(b) ";
        string innerPrefix = prefix + "? IsHogeCP(c) ";
        builder.AppendLine(innerPrefix + "? CP(c)");
        builder.AppendLine(new string(' ', innerPrefix.Length) + ": CP(b)");
        builder.AppendLine(new string(' ', prefix.Length) + ": d;");
    }

    /// <summary>
    /// 偽側ネストの三項演算子を追加します。
    /// </summary>
    /// <param name="builder">追加先です。</param>
    private static void AppendFalseNestedExampleTernary(StringBuilder builder)
    {
        string prefix = "        var a = IsHogeCP(b) ";
        string innerPrefix = new string(' ', prefix.Length) + ": IsHogeCP(c) ";
        builder.AppendLine(prefix + "? CP(b)");
        builder.AppendLine(innerPrefix + "? CP(c)");
        builder.AppendLine(new string(' ', innerPrefix.Length) + ": d;");
    }

    /// <summary>
    /// 条件メソッドを追加します。
    /// </summary>
    /// <param name="builder">追加先です。</param>
    /// <param name="conditionCount">条件数です。</param>
    private static void AppendConditionMethods(StringBuilder builder, int conditionCount)
    {
        for (int index = 1; index <= conditionCount; index++)
        {
            builder.Append("    private bool IsC");
            builder.Append(index.ToString(CultureInfo.InvariantCulture));
            builder.AppendLine("() => true;");
        }
    }
}
