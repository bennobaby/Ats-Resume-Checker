namespace AtsChecker.Core.Tests;

public class PdfExtractorTests
{
    [Fact]
    public void Text_pdf_returns_words_without_layout_inspection()
    {
        using var stream = new MemoryStream(MinimalPdf.Create("Hello ATS"));
        var resume = new ResumeExtractor().Extract(new ResumeInput(
            ResumeSource.Pdf, stream, null, "resume.pdf", ""));

        Assert.Contains("Hello", resume.Text);
        Assert.Contains("ATS", resume.Text);
        Assert.False(resume.Facts.LayoutInspected);
        Assert.True(resume.WordCount >= 2);
    }

    [Fact]
    public void Corrupt_pdf_throws()
    {
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        var ex = Assert.Throws<ResumeUnreadableException>(() =>
            new ResumeExtractor().Extract(new ResumeInput(
                ResumeSource.Pdf, stream, null, "bad.pdf", "")));
        Assert.Equal("This file could not be read.", ex.Message);
    }
}
