namespace AtsChecker.Core.Tests;

public class FileSizeRuleTests
{
    [Fact]
    public void Files_over_two_and_a_half_megabytes_warn()
    {
        Assert.Null(FileSizeRule.ForLength(FileSizeRule.ParserLimit));
        var finding = FileSizeRule.ForLength(FileSizeRule.ParserLimit + 1);
        Assert.NotNull(finding);
        Assert.Equal("file-size", finding.RuleId);
        Assert.Equal(Severity.Warn, finding.Severity);
        Assert.Equal("Some ATS parsers skip files larger than 2.5 MB.", finding.Explanation);
    }
}
