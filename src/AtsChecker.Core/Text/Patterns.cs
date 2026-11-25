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

    public static readonly Regex MonthYear = new(
        @"\b(?:jan(?:uary)?|feb(?:ruary)?|mar(?:ch)?|apr(?:il)?|may|jun(?:e)?|jul(?:y)?|aug(?:ust)?|sep(?:t(?:ember)?)?|oct(?:ober)?|nov(?:ember)?|dec(?:ember)?)\.?\s+(?:19|20)\d{2}\b|\b(?:0?[1-9]|1[0-2])[/-](?:19|20)\d{2}\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static readonly Regex SpacedLetters = new(
        @"\b(?:[A-Za-z]\s+){3,}[A-Za-z]\b",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);
}
