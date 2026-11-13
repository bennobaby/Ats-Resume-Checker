namespace AtsChecker.Core.Tests;

public class LineGrouperTests
{
    [Fact]
    public void Overlapping_words_share_a_line_and_a_lower_word_starts_the_next()
    {
        var text = LineGrouper.Join(new[]
        {
            new PositionedWord("heading", 80, 90, 140, 100),
            new PositionedWord("Experience", 10, 90, 70, 100),
            new PositionedWord("Skills", 10, 60, 50, 70)
        });

        Assert.Equal("Experience heading\nSkills", text);
    }
}
