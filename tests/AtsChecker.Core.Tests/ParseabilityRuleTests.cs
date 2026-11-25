namespace AtsChecker.Core.Tests;

public class ParseabilityRuleTests
{
    [Fact]
    public void Empty_text_runs_only_the_text_rule_and_fails()
    {
        var findings = ParseabilityRuleSet.Evaluate(Sample("   ", wordCount: 0));
        var finding = Assert.Single(findings);
        Assert.Equal("text-extracted", finding.RuleId);
        Assert.Equal(Severity.Fail, finding.Severity);
        Assert.Equal("Text extracted", finding.Title);
        Assert.Equal("No selectable text was found, so an ATS cannot read this resume.", finding.Explanation);
        Assert.Equal("0 characters extracted.", finding.Evidence);
    }

    [Fact]
    public void Email_phone_and_headings_pass_and_warn()
    {
        var pass = ParseabilityRuleSet.Evaluate(Sample(
            "Ada@Example.com\n415-555-1212\nExperience\nEducation\nSkills\nbuilder",
            wordCount: 200));
        AssertFinding(pass, "email", Severity.Pass, "An email address is present.", "Ada@Example.com");
        AssertFinding(pass, "phone", Severity.Pass, "A phone number is present.", "415-555-1212");
        AssertFinding(pass, "heading-experience", Severity.Pass, "A standard heading is present.", "Experience");
        AssertFinding(pass, "heading-education", Severity.Pass, "A standard heading is present.", "Education");
        AssertFinding(pass, "heading-skills", Severity.Pass, "A standard heading is present.", "Skills");
        AssertFinding(pass, "text-extracted", Severity.Pass, "The file produced selectable text.", "200");

        var warn = ParseabilityRuleSet.Evaluate(Sample("Experience with java is useful", wordCount: 200));
        AssertFinding(warn, "email", Severity.Warn, "No email address was found.", "none found");
        AssertFinding(warn, "phone", Severity.Warn, "No phone number was found.", "none found");
        AssertFinding(warn, "heading-experience", Severity.Warn, "A standard heading is missing.", "none found");
    }

    [Fact]
    public void Phone_does_not_cross_a_line_break()
    {
        var findings = ParseabilityRuleSet.Evaluate(Sample("415 555\n1212", wordCount: 200));
        AssertFinding(findings, "phone", Severity.Warn, "No phone number was found.", "none found");
    }

    [Fact]
    public void Heading_synonym_with_colon_matches_and_a_sentence_does_not()
    {
        var pass = ParseabilityRuleSet.Evaluate(Sample("Work History:\nAcademic Background\nTechnical Skills", wordCount: 200));
        AssertFinding(pass, "heading-experience", Severity.Pass, "A standard heading is present.", "Work History:");
        AssertFinding(pass, "heading-education", Severity.Pass, "A standard heading is present.", "Academic Background");
        AssertFinding(pass, "heading-skills", Severity.Pass, "A standard heading is present.", "Technical Skills");
    }

    [Fact]
    public void Length_warns_outside_150_to_1200()
    {
        var shortResume = ParseabilityRuleSet.Evaluate(Sample("short resume text", wordCount: 149));
        AssertFinding(shortResume, "length", Severity.Warn, "The resume is under 150 words.", "149");

        var longResume = ParseabilityRuleSet.Evaluate(Sample("long resume text", wordCount: 1201));
        AssertFinding(longResume, "length", Severity.Warn, "The resume is over 1200 words.", "1201");

        var ok = ParseabilityRuleSet.Evaluate(Sample("ok resume text", wordCount: 150));
        AssertFinding(ok, "length", Severity.Pass, "The resume length is within 150 to 1200 words.", "150");
    }

    [Fact]
    public void Layout_rules_warn_from_structure_facts()
    {
        var resume = new ExtractedResume(
            ResumeSource.Docx,
            "body text",
            200,
            new StructureFacts(2, 1, true, true, true));
        var findings = ParseabilityRuleSet.Evaluate(resume);
        AssertFinding(findings, "tables", Severity.Warn, "Tables often break ATS parsers.", "2");
        AssertFinding(findings, "text-boxes", Severity.Warn, "Text boxes are often skipped by ATS parsers.", "1");
        AssertFinding(findings, "columns", Severity.Warn, "Multiple columns often scramble ATS reading order.", "multiple columns");
        AssertFinding(findings, "header-footer", Severity.Warn, "Header and footer text is often ignored by an ATS.", "header or footer contains text");
    }

    [Fact]
    public void Layout_rules_do_not_run_when_layout_was_not_inspected()
    {
        var findings = ParseabilityRuleSet.Evaluate(Sample("body text", 200));
        Assert.DoesNotContain(findings, finding => finding.RuleId is "tables" or "text-boxes" or "columns" or "header-footer" or "images" or "fonts");
    }

    [Fact]
    public void Dates_need_a_role_with_a_start_and_an_end()
    {
        var pass = ParseabilityRuleSet.Evaluate(Sample(
            "Experience\nEngineer\nAcme\nJan 2020 - Mar 2023",
            wordCount: 200));
        AssertFinding(pass, "dates", Severity.Pass, "A role has a start and an end date.", "Jan 2020 – Mar 2023");

        var lone = ParseabilityRuleSet.Evaluate(Sample("Jan 2020 to March 2023", wordCount: 200));
        AssertFinding(lone, "dates", Severity.Warn, "No role has both a start and an end date.", "none found");
    }

    [Fact]
    public void Location_placeholder_and_short_title_are_reported()
    {
        var pass = ParseabilityRuleSet.Evaluate(Sample("Austin, TX\nSenior Account Executive", wordCount: 200));
        AssertFinding(pass, "location", Severity.Pass, "A city and region are present.", "Austin, TX");
        AssertFinding(pass, "placeholders", Severity.Pass, "No template names were found.", "none found");
        AssertFinding(pass, "job-titles", Severity.Pass, "Stored titles are written out.", "none abbreviated");

        var warn = ParseabilityRuleSet.Evaluate(Sample(
            "Experience\nSr. Account Exec\nCompany 1\nJan 2020 - Mar 2023",
            wordCount: 200));
        AssertFinding(warn, "location", Severity.Warn, "No city and region were found.", "none found");
        AssertFinding(warn, "placeholders", Severity.Warn, "Parsers skip template names such as Company 1 or Your Name.", "Company 1");
        AssertFinding(warn, "job-titles", Severity.Warn, "A shortened title such as Sr. or Exec is often stored incomplete.", "Sr. Account Exec");
    }

    [Fact]
    public void Heading_with_a_date_suffix_matches_and_a_sentence_does_not()
    {
        var pass = ParseabilityRuleSet.Evaluate(Sample("Experience (2018-2024)", wordCount: 200));
        AssertFinding(pass, "heading-experience", Severity.Pass, "A standard heading is present.", "Experience (2018-2024)");
    }

    [Fact]
    public void Spaced_letters_and_icons_warn()
    {
        var spaced = ParseabilityRuleSet.Evaluate(Sample("E X P E R I E N C E", wordCount: 200));
        AssertFinding(spaced, "spaced-letters", Severity.Warn, "Spaces between letters can stop a parser from reading a word.", "E X P E R I E N C E");

        var icons = ParseabilityRuleSet.Evaluate(Sample("Skills \u260E", wordCount: 200));
        AssertFinding(icons, "symbols", Severity.Warn, "Emoji and icon characters are often dropped or garbled.", "\u260E");

        var plain = ParseabilityRuleSet.Evaluate(Sample("Skills and experience", wordCount: 200));
        AssertFinding(plain, "spaced-letters", Severity.Pass, "Letters are not spaced out.", "none found");
        AssertFinding(plain, "symbols", Severity.Pass, "No emoji or icon characters were found.", "none found");
    }

    [Fact]
    public void Images_and_fonts_follow_inspection_flags()
    {
        var warned = new ExtractedResume(
            ResumeSource.Pdf,
            "body",
            200,
            new StructureFacts(0, 0, false, false, false, 2, true, "Comic Sans MS", true));
        var findings = ParseabilityRuleSet.Evaluate(warned);
        AssertFinding(findings, "images", Severity.Warn, "Images and graphics are not read as text.", "2");
        AssertFinding(findings, "fonts", Severity.Warn, "An uncommon font can be read as the wrong characters.", "Comic Sans MS");

        var clean = new ExtractedResume(
            ResumeSource.Docx,
            "body",
            200,
            new StructureFacts(0, 0, false, false, true, 0, true, null, true));
        var passed = ParseabilityRuleSet.Evaluate(clean);
        AssertFinding(passed, "images", Severity.Pass, "The file has no images.", "0 images");
        AssertFinding(passed, "fonts", Severity.Pass, "The file uses common fonts.", "common fonts");
    }

    private static ExtractedResume Sample(string text, int wordCount)
    {
        return new ExtractedResume(
            ResumeSource.PlainText,
            text,
            wordCount,
            new StructureFacts(0, 0, false, false, false));
    }

    private static void AssertFinding(
        IReadOnlyList<Finding> findings,
        string ruleId,
        Severity severity,
        string explanation,
        string evidence)
    {
        var finding = Assert.Single(findings, item => item.RuleId == ruleId);
        Assert.Equal(severity, finding.Severity);
        Assert.Equal(explanation, finding.Explanation);
        Assert.Equal(evidence, finding.Evidence);
    }
}
