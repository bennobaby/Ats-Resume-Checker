namespace AtsChecker.Core.Tests;

public class ResumeCheckerTests
{
    private readonly ResumeChecker _checker = new();

    [Fact]
    public void Empty_resume_scores_zero_and_skips_keywords()
    {
        var report = _checker.Check(Sample("  ", 0, inspected: false), "python developer");
        Assert.Equal(0, report.ParseabilityPercent);
        Assert.Equal("text-extracted", Assert.Single(report.Findings).RuleId);
        Assert.Equal("Keyword match was skipped because the resume had no text.", report.Keywords.SkipReason);
        Assert.Equal("Layout was not inspected for this source.", report.LayoutNote);
    }

    [Fact]
    public void Empty_job_description_keeps_parseability()
    {
        var report = _checker.Check(Sample("Ada@Example.com python", 10, inspected: true), " ");
        Assert.True(report.ParseabilityPercent > 0);
        Assert.Equal("Keyword match was skipped because the job description was empty.", report.Keywords.SkipReason);
        Assert.Null(report.LayoutNote);
        Assert.Contains(report.Findings, finding => finding.RuleId == "tables");
    }

    private static ExtractedResume Sample(string text, int wordCount, bool inspected)
    {
        return new ExtractedResume(
            inspected ? ResumeSource.Docx : ResumeSource.PlainText,
            text,
            wordCount,
            new StructureFacts(0, 0, false, false, inspected));
    }
}
