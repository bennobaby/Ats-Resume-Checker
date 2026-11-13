namespace AtsChecker.Core.Tests;

public class PlainTextExtractorTests
{
    [Fact]
    public void Paste_is_returned_unchanged_without_layout()
    {
        var extractor = new ResumeExtractor();
        var resume = extractor.Extract(new ResumeInput(
            ResumeSource.PlainText,
            null,
            "  Ada example  ",
            null,
            "ignored by the extractor"));

        Assert.Equal(ResumeSource.PlainText, resume.Source);
        Assert.Equal("  Ada example  ", resume.Text);
        Assert.Equal(2, resume.WordCount);
        Assert.False(resume.Facts.LayoutInspected);
        Assert.Equal(0, resume.Facts.TableCount);
        Assert.Equal(0, resume.Facts.TextBoxCount);
        Assert.False(resume.Facts.HasMultipleColumns);
        Assert.False(resume.Facts.HeaderOrFooterHasText);
    }
}
