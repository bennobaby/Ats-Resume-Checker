namespace AtsChecker.Core.Tests;

public class DocxExtractorTests
{
    [Fact]
    public void Docx_includes_table_and_textbox_text_and_warns_on_layout()
    {
        using var stream = new MemoryStream(DocxFixtures.WithLayoutTraps());
        var resume = new ResumeExtractor().Extract(new ResumeInput(
            ResumeSource.Docx, stream, null, "resume.docx", ""));

        Assert.Equal(
            "Visible body\nInside table\n\nInside box",
            resume.Text);
        Assert.Equal(6, resume.WordCount);
        Assert.DoesNotContain("Secret Header", resume.Text);
        Assert.True(resume.Facts.LayoutInspected);
        Assert.True(resume.Facts.TableCount >= 1);
        Assert.True(resume.Facts.TextBoxCount >= 1);
        Assert.True(resume.Facts.HasMultipleColumns);
        Assert.True(resume.Facts.HeaderOrFooterHasText);

        var findings = ParseabilityRuleSet.Evaluate(resume);
        Assert.Equal(Severity.Warn, Assert.Single(findings, finding => finding.RuleId == "tables").Severity);
        Assert.Equal(Severity.Warn, Assert.Single(findings, finding => finding.RuleId == "text-boxes").Severity);
        Assert.Equal(Severity.Warn, Assert.Single(findings, finding => finding.RuleId == "columns").Severity);
        Assert.Equal(Severity.Warn, Assert.Single(findings, finding => finding.RuleId == "header-footer").Severity);
    }

    [Fact]
    public void Uncommon_font_is_reported()
    {
        using var stream = new MemoryStream(DocxFixtures.WithFont("Comic Sans MS"));
        var resume = new ResumeExtractor().Extract(new ResumeInput(
            ResumeSource.Docx, stream, null, "resume.docx", ""));

        Assert.True(resume.Facts.FontsInspected);
        Assert.Equal("Comic Sans MS", resume.Facts.UnusualFont);
        Assert.Equal(0, resume.Facts.ImageCount);
    }

    [Fact]
    public void Corrupt_docx_throws()
    {
        using var stream = new MemoryStream(new byte[] { 1, 2, 3, 4 });
        Assert.Throws<ResumeUnreadableException>(() =>
            new ResumeExtractor().Extract(new ResumeInput(
                ResumeSource.Docx, stream, null, "bad.docx", "")));
    }
}