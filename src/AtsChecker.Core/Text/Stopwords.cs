namespace AtsChecker.Core;

public static class Stopwords
{
    public static readonly HashSet<string> Words = new(StringComparer.Ordinal)
    {
        "about", "after", "also", "been", "before", "being", "both", "could",
        "does", "done", "each", "from", "have", "into", "just", "more", "most",
        "must", "only", "other", "over", "shall", "should", "some", "such",
        "than", "that", "their", "them", "then", "there", "these", "they",
        "this", "those", "very", "were", "what", "when", "where", "which",
        "with", "would", "your",
        "looking", "seeking", "writes", "writing", "clear", "strong", "using",
        "join", "team", "role", "required", "preferred", "ability", "years",
        "year", "position", "opportunity", "candidate", "candidates"
    };
}
