namespace AtsChecker.Core;

public static class HeadingLines
{
    private static readonly string[] Experience =
    {
        "experience", "work experience", "professional experience",
        "employment", "employment history", "work history"
    };

    private static readonly string[] Education = { "education", "academic background" };

    private static readonly string[] Skills =
    {
        "skills", "technical skills", "core skills", "core competencies", "competencies"
    };

    public static string? FirstMatch(string text, string kind)
    {
        var synonyms = kind switch
        {
            "experience" => Experience,
            "education" => Education,
            _ => Skills
        };

        foreach (var line in text.Split('\n'))
        {
            var trimmed = line.Trim();
            var normalized = trimmed.ToLowerInvariant();
            if (normalized.EndsWith(':'))
                normalized = normalized[..^1];
            if (synonyms.Contains(normalized, StringComparer.Ordinal))
                return trimmed;
        }

        return null;
    }
}
