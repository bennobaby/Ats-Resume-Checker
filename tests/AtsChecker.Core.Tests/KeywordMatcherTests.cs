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
    public void Unigram_does_not_match_inside_a_longer_token()
    {
        var match = KeywordMatcher.Match("javascript engineer", "java");
        Assert.DoesNotContain("java", match.Found);
        Assert.Contains("java", match.Missing);
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
