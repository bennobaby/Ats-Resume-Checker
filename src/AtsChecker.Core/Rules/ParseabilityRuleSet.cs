namespace AtsChecker.Core;

public static class ParseabilityRuleSet
{
    public static IReadOnlyList<Finding> Evaluate(ExtractedResume resume)
    {
        var findings = new List<Finding> { TextExtracted(resume) };
        if (string.IsNullOrWhiteSpace(resume.Text))
            return findings;

        findings.Add(Email(resume));
        findings.Add(Phone(resume));
        findings.Add(Heading(resume, "heading-experience", "Experience heading", "experience"));
        findings.Add(Heading(resume, "heading-education", "Education heading", "education"));
        findings.Add(Heading(resume, "heading-skills", "Skills heading", "skills"));
        findings.Add(Length(resume));

        if (resume.Facts.LayoutInspected)
        {
            findings.Add(Tables(resume));
            findings.Add(TextBoxes(resume));
            findings.Add(Columns(resume));
            findings.Add(HeaderFooter(resume));
        }

        return findings;
    }

    private static Finding TextExtracted(ExtractedResume resume)
    {
        if (string.IsNullOrWhiteSpace(resume.Text))
        {
            return new Finding(
                "text-extracted",
                Severity.Fail,
                "Text extracted",
                "No selectable text was found, so an ATS cannot read this resume.",
                "0 characters extracted.");
        }

        return new Finding(
            "text-extracted",
            Severity.Pass,
            "Text extracted",
            "The file produced selectable text.",
            resume.WordCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    private static Finding Email(ExtractedResume resume)
    {
        var match = Patterns.Email.Match(resume.Text);
        return match.Success
            ? new Finding("email", Severity.Pass, "Email", "An email address is present.", match.Value)
            : new Finding("email", Severity.Warn, "Email", "No email address was found.", "none found");
    }

    private static Finding Phone(ExtractedResume resume)
    {
        var match = Patterns.Phone.Match(resume.Text);
        return match.Success
            ? new Finding("phone", Severity.Pass, "Phone", "A phone number is present.", match.Value)
            : new Finding("phone", Severity.Warn, "Phone", "No phone number was found.", "none found");
    }

    private static Finding Heading(ExtractedResume resume, string ruleId, string title, string kind)
    {
        var line = HeadingLines.FirstMatch(resume.Text, kind);
        return line is null
            ? new Finding(ruleId, Severity.Warn, title, "A standard heading is missing.", "none found")
            : new Finding(ruleId, Severity.Pass, title, "A standard heading is present.", line);
    }

    private static Finding Length(ExtractedResume resume)
    {
        var evidence = resume.WordCount.ToString(System.Globalization.CultureInfo.InvariantCulture);
        if (resume.WordCount < 150)
            return new Finding("length", Severity.Warn, "Length", "The resume is under 150 words.", evidence);
        if (resume.WordCount > 1200)
            return new Finding("length", Severity.Warn, "Length", "The resume is over 1200 words.", evidence);
        return new Finding("length", Severity.Pass, "Length", "The resume length is within 150 to 1200 words.", evidence);
    }

    private static Finding Tables(ExtractedResume resume)
    {
        return resume.Facts.TableCount == 0
            ? new Finding("tables", Severity.Pass, "Tables", "The document has no tables.", "0 tables")
            : new Finding("tables", Severity.Warn, "Tables", "Tables often break ATS parsers.", resume.Facts.TableCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    private static Finding TextBoxes(ExtractedResume resume)
    {
        return resume.Facts.TextBoxCount == 0
            ? new Finding("text-boxes", Severity.Pass, "Text boxes", "The document has no text boxes.", "0 text boxes")
            : new Finding("text-boxes", Severity.Warn, "Text boxes", "Text boxes are often skipped by ATS parsers.", resume.Facts.TextBoxCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    private static Finding Columns(ExtractedResume resume)
    {
        return resume.Facts.HasMultipleColumns
            ? new Finding("columns", Severity.Warn, "Columns", "Multiple columns often scramble ATS reading order.", "multiple columns")
            : new Finding("columns", Severity.Pass, "Columns", "The document uses a single column.", "single column");
    }

    private static Finding HeaderFooter(ExtractedResume resume)
    {
        return resume.Facts.HeaderOrFooterHasText
            ? new Finding("header-footer", Severity.Warn, "Header or footer", "Header and footer text is often ignored by an ATS.", "header or footer contains text")
            : new Finding("header-footer", Severity.Pass, "Header or footer", "The header and footer have no text.", "no header or footer text");
    }
}
