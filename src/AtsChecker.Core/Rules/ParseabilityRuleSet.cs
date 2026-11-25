using System.Text;

namespace AtsChecker.Core;

public static class ParseabilityRuleSet
{
    public static IReadOnlyList<Finding> Evaluate(ExtractedResume resume)
    {
        var findings = new List<Finding> { TextExtracted(resume) };
        if (string.IsNullOrWhiteSpace(resume.Text))
            return findings;

        var profile = ProfileReader.Read(resume.Text);
        findings.Add(Email(resume));
        findings.Add(Phone(resume));
        findings.Add(Location(profile));
        findings.Add(Heading(resume, "heading-experience", "Experience heading", "experience"));
        findings.Add(Heading(resume, "heading-education", "Education heading", "education"));
        findings.Add(Heading(resume, "heading-skills", "Skills heading", "skills"));
        findings.Add(Length(resume));
        findings.Add(Dates(profile));
        findings.Add(JobTitles(profile));
        findings.Add(Placeholders(resume));
        findings.Add(SpacedLetters(resume));
        findings.Add(Symbols(resume));

        if (resume.Facts.ImagesInspected)
            findings.Add(Images(resume));
        if (resume.Facts.FontsInspected)
            findings.Add(Fonts(resume));

        if (resume.Facts.LayoutInspected || resume.Facts.FlowInspected)
        {
            findings.Add(Tables(resume));
            findings.Add(Columns(resume));
        }

        if (resume.Facts.LayoutInspected)
        {
            findings.Add(TextBoxes(resume));
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

    private static Finding Location(ResumeProfile profile)
    {
        return profile.Location is null
            ? new Finding("location", Severity.Warn, "Location", "No city and region were found.", "none found")
            : new Finding("location", Severity.Pass, "Location", "A city and region are present.", profile.Location);
    }

    private static Finding Dates(ResumeProfile profile)
    {
        var job = profile.Jobs.FirstOrDefault();
        return job is null
            ? new Finding("dates", Severity.Warn, "Dates", "No role has both a start and an end date.", "none found")
            : new Finding("dates", Severity.Pass, "Dates", "A role has a start and an end date.", job.Start + " – " + job.End);
    }

    private static readonly HashSet<string> ShortTitles = new(StringComparer.OrdinalIgnoreCase)
    {
        "sr", "jr", "exec", "mgr", "engr", "asst", "acct"
    };

    private static Finding JobTitles(ResumeProfile profile)
    {
        foreach (var job in profile.Jobs)
        {
            var words = job.Title.Split(new[] { ' ', '.', ',', '/', '-' }, StringSplitOptions.RemoveEmptyEntries);
            if (!words.Any(ShortTitles.Contains))
                continue;

            return new Finding(
                "job-titles",
                Severity.Warn,
                "Job titles",
                "A shortened title such as Sr. or Exec is often stored incomplete.",
                job.Title);
        }

        return new Finding("job-titles", Severity.Pass, "Job titles", "Stored titles are written out.", "none abbreviated");
    }

    private static readonly System.Text.RegularExpressions.Regex Placeholder = new(
        @"\b(?:first last|your name|any company|lorem ipsum|company\s+\d+|client\s+\d+|employee\s+\d+)\b",
        System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.CultureInvariant | System.Text.RegularExpressions.RegexOptions.Compiled);

    private static Finding Placeholders(ExtractedResume resume)
    {
        var match = Placeholder.Match(resume.Text);
        return match.Success
            ? new Finding("placeholders", Severity.Warn, "Placeholder text", "Parsers skip template names such as Company 1 or Your Name.", match.Value)
            : new Finding("placeholders", Severity.Pass, "Placeholder text", "No template names were found.", "none found");
    }

    private static Finding SpacedLetters(ExtractedResume resume)
    {
        var match = Patterns.SpacedLetters.Match(resume.Text);
        return match.Success
            ? new Finding("spaced-letters", Severity.Warn, "Spaced letters", "Spaces between letters can stop a parser from reading a word.", match.Value)
            : new Finding("spaced-letters", Severity.Pass, "Spaced letters", "Letters are not spaced out.", "none found");
    }

    private static Finding Symbols(ExtractedResume resume)
    {
        foreach (var rune in resume.Text.EnumerateRunes())
        {
            if (!IsIcon(rune))
                continue;

            return new Finding(
                "symbols",
                Severity.Warn,
                "Icons",
                "Emoji and icon characters are often dropped or garbled.",
                rune.ToString());
        }

        return new Finding("symbols", Severity.Pass, "Icons", "No emoji or icon characters were found.", "none found");
    }

    private static bool IsIcon(Rune rune)
    {
        var value = rune.Value;
        return value is >= 0x1F000 and <= 0x1FAFF
            or >= 0x2600 and <= 0x26FF
            or >= 0x2700 and <= 0x27BF;
    }

    private static Finding Images(ExtractedResume resume)
    {
        return resume.Facts.ImageCount == 0
            ? new Finding("images", Severity.Pass, "Images", "The file has no images.", "0 images")
            : new Finding("images", Severity.Warn, "Images", "Images and graphics are not read as text.", resume.Facts.ImageCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    private static Finding Fonts(ExtractedResume resume)
    {
        return resume.Facts.UnusualFont is null
            ? new Finding("fonts", Severity.Pass, "Fonts", "The file uses common fonts.", "common fonts")
            : new Finding("fonts", Severity.Warn, "Fonts", "An uncommon font can be read as the wrong characters.", resume.Facts.UnusualFont);
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
