namespace AtsChecker.Core;

public static class FontNames
{
    private static readonly string[] Common =
    {
        "arial", "calibri", "cambria", "times", "timesnewroman", "georgia",
        "garamond", "verdana", "tahoma", "helvetica", "courier", "couriernew",
        "aptos", "carlito", "liberation", "segoe", "constantia", "palatino",
        "bookantiqua", "trebuchet", "cambriamath"
    };

    public static bool IsCommon(string? fontName)
    {
        var key = Key(fontName);
        if (key.Length == 0)
            return true;

        foreach (var common in Common)
        {
            if (key.StartsWith(common, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    public static string Display(string fontName)
    {
        var plus = fontName.LastIndexOf('+');
        return plus >= 0 && plus < fontName.Length - 1
            ? fontName[(plus + 1)..]
            : fontName;
    }

    private static string Key(string? fontName)
    {
        if (string.IsNullOrWhiteSpace(fontName))
            return "";

        var name = Display(fontName);
        return new string(name.ToLowerInvariant().Where(char.IsLetterOrDigit).ToArray());
    }
}
