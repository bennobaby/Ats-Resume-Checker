namespace AtsChecker.Core;

public static class TextNormalizer
{
    public static IReadOnlyList<string> Tokens(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return Array.Empty<string>();

        var buffer = new char[text.Length];
        for (var i = 0; i < text.Length; i++)
            buffer[i] = char.IsLetterOrDigit(text[i]) ? char.ToLowerInvariant(text[i]) : ' ';

        var raw = new string(buffer).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var tokens = new List<string>(raw.Length);
        foreach (var token in raw)
        {
            if (Stopwords.Words.Contains(token))
                continue;
            if (token.Length < 4 && token != "net")
                continue;
            tokens.Add(token);
        }

        return tokens;
    }
}
