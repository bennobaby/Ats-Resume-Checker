namespace AtsChecker.Core;

public static class TextNormalizer
{
    public static IReadOnlyList<string> Tokens(string? text)
    {
        return RawTokens(text).Where(IsKeyword).ToArray();
    }

    public static IReadOnlyList<string> RawTokens(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<string>();

        var tokens = new List<string>();
        var index = 0;
        while (index < text.Length)
        {
            if (!char.IsLetterOrDigit(text[index]))
            {
                index++;
                continue;
            }

            var start = index;
            while (index < text.Length && char.IsLetterOrDigit(text[index]))
                index++;

            if (index < text.Length && text[index] == '#')
                index++;
            else if (index + 1 < text.Length && text[index] == '+' && text[index + 1] == '+')
                index += 2;

            tokens.Add(text.Substring(start, index - start).ToLowerInvariant());
        }

        return tokens;
    }

    public static string Canonical(string token)
    {
        return Aliases.TryGetValue(token, out var canonical) ? canonical : token;
    }

    public static bool IsKeyword(string token)
    {
        if (Stopwords.Words.Contains(token))
            return false;

        return token.Length >= 4 || token is "net" or "js" or "k8s" or "c#" or "c++" or "f#";
    }

    private static readonly Dictionary<string, string> Aliases = new(StringComparer.Ordinal)
    {
        ["js"] = "javascript",
        ["javascript"] = "javascript",
        ["k8s"] = "kubernetes",
        ["kubernetes"] = "kubernetes",
        ["postgres"] = "postgresql",
        ["postgresql"] = "postgresql"
    };
}
