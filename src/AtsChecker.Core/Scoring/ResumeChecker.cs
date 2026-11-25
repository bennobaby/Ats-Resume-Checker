namespace AtsChecker.Core;

public sealed class ResumeChecker : IResumeChecker
{
    public CheckReport Check(ExtractedResume resume, string jobDescription)
    {
        var findings = ParseabilityRuleSet.Evaluate(resume);
        var percent = ScoreMath.PercentFromSeverities(findings.Select(finding => finding.Severity).ToArray());
        var profile = ProfileReader.Read(resume.Text);
        var keywords = KeywordMatcher.Match(resume.Text, jobDescription, profile.Skills);
        var layoutNote = resume.Facts.LayoutInspected
            ? null
            : resume.Facts.FlowInspected
                ? "Text boxes and headers were not inspected for this source."
                : "Layout was not inspected for this source.";

        return new CheckReport(
            percent,
            keywords,
            findings,
            resume.Source,
            resume.WordCount,
            layoutNote,
            profile);
    }
}
