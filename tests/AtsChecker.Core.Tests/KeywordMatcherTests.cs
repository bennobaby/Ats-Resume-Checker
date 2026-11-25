namespace AtsChecker.Core.Tests;

public class KeywordMatcherTests
{
    [Fact]
    public void Match_skips_when_resume_has_no_text()
    {
        var match = KeywordMatcher.Match("   ", "python developer");
        Assert.False(match.WasRun);
        Assert.Null(match.ScorePercent);
        Assert.Empty(match.Found);
        Assert.Empty(match.Missing);
        Assert.Equal("Keyword match was skipped because the resume had no text.", match.SkipReason);
    }

    [Fact]
    public void Match_skips_when_job_description_is_empty()
    {
        var match = KeywordMatcher.Match("python developer", "  ");
        Assert.False(match.WasRun);
        Assert.Equal("Keyword match was skipped because the job description was empty.", match.SkipReason);
    }

    [Fact]
    public void Match_skips_when_no_candidates_remain()
    {
        var match = KeywordMatcher.Match("python developer", "a an the");
        Assert.False(match.WasRun);
        Assert.Equal("No keywords could be extracted from the job description.", match.SkipReason);
    }

    [Fact]
    public void Aliases_and_symbol_tokens_match()
    {
        var match = KeywordMatcher.Match(
            "js developer using k8s and postgres, plus C#",
            "javascript developer kubernetes postgresql c#");

        Assert.Contains("javascript", match.Found);
        Assert.Contains("javascript developer", match.Found);
        Assert.Contains("kubernetes", match.Found);
        Assert.Contains("postgresql", match.Found);
        Assert.Contains("c#", match.Found);
        Assert.DoesNotContain("java", match.Found);
    }

    [Fact]
    public void A_listed_skill_counts_when_the_body_uses_the_alias()
    {
        var match = KeywordMatcher.Match("built services", "javascript", new[] { "JS" });
        Assert.Contains("javascript", match.Found);
    }

    [Fact]
    public void Unigram_does_not_match_inside_a_longer_token()
    {
        var match = KeywordMatcher.Match("javascript engineer", "java");
        Assert.DoesNotContain("java", match.Found);
        Assert.Contains("java", match.Missing);
    }

    [Fact]
    public void Filler_does_not_become_a_phrase()
    {
        var match = KeywordMatcher.Match(
            "python developer for internal tools",
            "Looking for a python developer who writes clear internal tools.");

        var terms = match.Found.Concat(match.Missing).ToArray();
        Assert.DoesNotContain("looking python", terms);
        Assert.Contains("python developer", match.Found);
        Assert.Contains("internal tools", match.Found);
        Assert.DoesNotContain("developer internal", terms);
    }

    [Fact]
    public void Phrase_matches_only_when_tokens_are_adjacent()
    {
        var match = KeywordMatcher.Match("python language scala", "python scala");
        Assert.Equal(67, match.ScorePercent);
        Assert.Equal(new[] { "python", "scala" }, match.Found);
        Assert.Equal(new[] { "python scala" }, match.Missing);
    }

    [Fact]
    public void Cap_keeps_forty_phrases_and_no_unigrams()
    {
        var words = Enumerable.Range(0, 42).Select(i => $"term{i:00}").ToArray();
        var text = string.Join(" ", words);
        var match = KeywordMatcher.Match(text, text);
        var kept = match.Found.Concat(match.Missing).ToArray();
        Assert.Equal(40, kept.Length);
        Assert.All(kept, term => Assert.Contains(' ', term));
        Assert.Contains("term00 term01", kept);
        Assert.DoesNotContain("term40 term41", kept);
    }
}
