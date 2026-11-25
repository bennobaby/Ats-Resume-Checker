namespace AtsChecker.Core;

public static class KeywordMatcher
{
    public static KeywordMatch Match(string? resumeText, string? jobDescription, IReadOnlyList<string>? skills = null)
    {
        if (string.IsNullOrWhiteSpace(resumeText))
            return Skip("Keyword match was skipped because the resume had no text.");
        if (string.IsNullOrWhiteSpace(jobDescription))
            return Skip("Keyword match was skipped because the job description was empty.");

        var jobTokens = TextNormalizer.RawTokens(jobDescription);
        var selected = Select(jobTokens);
        if (selected.Count == 0)
            return Skip("No keywords could be extracted from the job description.");

        var resumeTokens = TextNormalizer.RawTokens(resumeText);
        var resumeWords = resumeTokens.Where(TextNormalizer.IsKeyword)
            .Select(TextNormalizer.Canonical)
            .ToHashSet(StringComparer.Ordinal);
        var resumePhrases = Phrases(resumeTokens);
        AddSkills(skills, resumeWords, resumePhrases);

        var found = new List<string>();
        var missing = new List<string>();
        foreach (var term in selected)
        {
            var hit = term.Contains(' ')
                ? resumePhrases.Contains(CanonicalPhrase(term))
                : resumeWords.Contains(TextNormalizer.Canonical(term));
            if (hit)
                found.Add(term);
            else
                missing.Add(term);
        }

        return new KeywordMatch(
            true,
            ScoreMath.PercentFromRatio(found.Count, selected.Count),
            found,
            missing,
            null);
    }

    private static HashSet<string> Phrases(IReadOnlyList<string> tokens)
    {
        var phrases = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < tokens.Count - 1; i++)
        {
            if (TextNormalizer.IsKeyword(tokens[i]) && TextNormalizer.IsKeyword(tokens[i + 1]))
                phrases.Add(CanonicalPhrase(tokens[i] + " " + tokens[i + 1]));
        }

        return phrases;
    }

    private static string CanonicalPhrase(string phrase)
    {
        return string.Join(' ', phrase.Split(' ').Select(TextNormalizer.Canonical));
    }

    private static void AddSkills(IReadOnlyList<string>? skills, HashSet<string> words, HashSet<string> phrases)
    {
        if (skills is null)
            return;

        foreach (var skill in skills)
        {
            var tokens = TextNormalizer.RawTokens(skill).Where(TextNormalizer.IsKeyword).ToArray();
            foreach (var token in tokens)
                words.Add(TextNormalizer.Canonical(token));
            if (tokens.Length >= 2)
                phrases.Add(string.Join(' ', tokens.Select(TextNormalizer.Canonical)));
        }
    }

    private static KeywordMatch Skip(string reason)
    {
        return new KeywordMatch(false, null, Array.Empty<string>(), Array.Empty<string>(), reason);
    }

    private static List<string> Select(IReadOnlyList<string> tokens)
    {
        var unigrams = new List<string>();
        var seenUnigrams = new HashSet<string>(StringComparer.Ordinal);
        foreach (var token in tokens.Where(TextNormalizer.IsKeyword))
        {
            if (seenUnigrams.Add(token))
                unigrams.Add(token);
        }

        var phrases = new List<string>();
        var seenPhrases = new HashSet<string>(StringComparer.Ordinal);
        var phraseFrequency = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < tokens.Count - 1; i++)
        {
            if (!TextNormalizer.IsKeyword(tokens[i]) || !TextNormalizer.IsKeyword(tokens[i + 1]))
                continue;

            var phrase = tokens[i] + " " + tokens[i + 1];
            phraseFrequency[phrase] = phraseFrequency.GetValueOrDefault(phrase) + 1;
            if (seenPhrases.Add(phrase))
                phrases.Add(phrase);
        }

        if (unigrams.Count + phrases.Count <= 40)
            return unigrams.Concat(phrases).ToList();

        var unigramFrequency = tokens.Where(TextNormalizer.IsKeyword)
            .GroupBy(token => token, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.Count(), StringComparer.Ordinal);

        return phrases.Select(phrase => (Term: phrase, IsPhrase: true, Frequency: phraseFrequency[phrase]))
            .Concat(unigrams.Select(term => (Term: term, IsPhrase: false, Frequency: unigramFrequency[term])))
            .OrderByDescending(item => item.IsPhrase)
            .ThenByDescending(item => item.Frequency)
            .ThenBy(item => item.Term, StringComparer.Ordinal)
            .Take(40)
            .Select(item => item.Term)
            .ToList();
    }
}
