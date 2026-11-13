namespace AtsChecker.Core.Tests;

public class TextNormalizerTests
{
    [Fact]
    public void Tokens_drop_symbols_short_words_and_stopwords()
    {
        Assert.Equal(new[] { "net" }, TextNormalizer.Tokens("C# and .NET"));
        Assert.Equal(new[] { "python" }, TextNormalizer.Tokens("with python"));
        Assert.Equal(new[] { "machine", "learning" }, TextNormalizer.Tokens("machine and learning"));
        Assert.Equal(new[] { "net" }, TextNormalizer.Tokens("api and net"));
    }
}
