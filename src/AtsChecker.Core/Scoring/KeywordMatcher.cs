namespace AtsChecker.Core;

public static class KeywordMatcher
{
    public static KeywordMatch Match(string? resumeText, string? jobDescription)
    {
        if (string.IsNullOrWhiteSpace(resumeText))
            return Skip("Keyword match was skipped because the resume had no text.");
        if (string.IsNullOrWhiteSpace(jobDescription))
            return Skip("Keyword match was skipped because the job description was empty.");

        var jobTokens = TextNormalizer.Tokens(jobDescription);
        var selected = Select(jobTokens);
        if (selected.Count == 0)
            return Skip("No keywords could be extracted from the job description.");

        var resumeTokens = TextNormalizer.Tokens(resumeText);
        var resumeWords = resumeTokens.ToHashSet(StringComparer.Ordinal);
        var resumePhrases = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i < resumeTokens.Count - 1; i++)
            resumePhrases.Add(resumeTokens[i] + " " + resumeTokens[i + 1]);

        var found = new List<string>();
        var missing = new List<string>();
        foreach (var term in selected)
        {
            var hit = term.Contains(' ') ? resumePhrases.Contains(term) : resumeWords.Contains(term);
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

    private static KeywordMatch Skip(string reason)
    {
        return new KeywordMatch(false, null, Array.Empty<string>(), Array.Empty<string>(), reason);
    }

    private static List<string> Select(IReadOnlyList<string> tokens)
    {
        var unigrams = new List<string>();
        var seenUnigrams = new HashSet<string>(StringComparer.Ordinal);
        foreach (var token in tokens)
        {
            if (seenUnigrams.Add(token))
                unigrams.Add(token);
        }

        var phrases = new List<string>();
        var seenPhrases = new HashSet<string>(StringComparer.Ordinal);
        var phraseFrequency = new Dictionary<string, int>(StringComparer.Ordinal);
        for (var i = 0; i < tokens.Count - 1; i++)
        {
            var phrase = tokens[i] + " " + tokens[i + 1];
            phraseFrequency[phrase] = phraseFrequency.GetValueOrDefault(phrase) + 1;
            if (seenPhrases.Add(phrase))
                phrases.Add(phrase);
        }

        if (unigrams.Count + phrases.Count <= 40)
            return unigrams.Concat(phrases).ToList();

        var unigramFrequency = tokens.GroupBy(token => token, StringComparer.Ordinal)
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
