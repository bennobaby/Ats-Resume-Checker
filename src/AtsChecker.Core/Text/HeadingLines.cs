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

    private static readonly string[] Summary = { "summary", "professional summary", "profile" };

    private static readonly string[] Certifications = { "certifications", "certification", "licenses" };

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
            if (Matches(line, synonyms))
                return line.Trim();
        }

        return null;
    }

    public static string? Kind(string line)
    {
        if (Matches(line, Experience))
            return "experience";
        if (Matches(line, Education))
            return "education";
        if (Matches(line, Skills))
            return "skills";
        if (Matches(line, Summary))
            return "summary";
        if (Matches(line, Certifications))
            return "certifications";
        return null;
    }

    private static bool Matches(string line, IReadOnlyList<string> synonyms)
    {
        var normalized = line.Trim().ToLowerInvariant();
        if (normalized.EndsWith(':'))
            normalized = normalized[..^1].TrimEnd();

        foreach (var synonym in synonyms)
        {
            if (normalized == synonym)
                return true;
            if (!normalized.StartsWith(synonym, StringComparison.Ordinal))
                continue;
            if (synonym.Length >= normalized.Length)
                continue;

            var rest = normalized[synonym.Length..].Trim();
            if (rest.Length > 0 && rest.All(character => !char.IsLetter(character)))
                return true;
        }

        return false;
    }
}
