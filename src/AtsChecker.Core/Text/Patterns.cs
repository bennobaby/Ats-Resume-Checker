using System.Text.RegularExpressions;

namespace AtsChecker.Core;

public static class Patterns
{
    public static readonly Regex Email = new(
        "[A-Za-z0-9._%+\\-]+@[A-Za-z0-9.\\-]+\\.[A-Za-z]{2,}",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static readonly Regex Phone = new(
        @"(?<!\d)\+?(?:\d[ \t().-]{0,3}){9,14}\d(?!\d)",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);
}
