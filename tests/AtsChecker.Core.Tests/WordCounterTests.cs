namespace AtsChecker.Core.Tests;

public class WordCounterTests
{
    [Fact]
    public void Count_splits_on_whitespace()
    {
        Assert.Equal(2, WordCounter.Count("hello world"));
        Assert.Equal(2, WordCounter.Count("  a   b  "));
        Assert.Equal(0, WordCounter.Count("   "));
        Assert.Equal(0, WordCounter.Count(null));
    }
}
