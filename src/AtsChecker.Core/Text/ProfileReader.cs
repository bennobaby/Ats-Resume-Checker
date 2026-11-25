using System.Globalization;
using System.Text.RegularExpressions;

namespace AtsChecker.Core;

public static class ProfileReader
{
    private static readonly Regex Link = new(
        @"https?://\S+|linkedin\.com/\S+|github\.com/\S+",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex CityRegion = new(
        @"\b(?<city>[A-Za-z]+(?:[ \t]+[A-Za-z]+){0,2}),[ \t]*(?<region>[A-Za-z]+(?:[ \t]+[A-Za-z]+){0,2})\b",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly HashSet<string> Regions = new(StringComparer.OrdinalIgnoreCase)
    {
        "AL", "AK", "AZ", "AR", "CA", "CO", "CT", "DC", "DE", "FL", "GA", "HI", "IA", "ID", "IL", "IN",
        "KS", "KY", "LA", "MA", "MD", "ME", "MI", "MN", "MO", "MS", "MT", "NC", "ND", "NE", "NH", "NJ",
        "NM", "NV", "NY", "OH", "OK", "OR", "PA", "RI", "SC", "SD", "TN", "TX", "UT", "VA", "VT", "WA",
        "WI", "WV", "WY",
        "Alabama", "Alaska", "Arizona", "Arkansas", "California", "Colorado", "Connecticut",
        "Delaware", "Florida", "Georgia", "Hawaii", "Idaho", "Illinois", "Indiana", "Iowa",
        "Kansas", "Kentucky", "Louisiana", "Maine", "Maryland", "Massachusetts", "Michigan",
        "Minnesota", "Mississippi", "Missouri", "Montana", "Nebraska", "Nevada", "New Hampshire",
        "New Jersey", "New Mexico", "New York", "North Carolina", "North Dakota", "Ohio",
        "Oklahoma", "Oregon", "Pennsylvania", "Rhode Island", "South Carolina", "South Dakota",
        "Tennessee", "Texas", "Utah", "Vermont", "Virginia", "Washington", "West Virginia",
        "Wisconsin", "Wyoming", "District of Columbia",
        "US", "USA", "UK", "UAE", "United States", "United Kingdom", "Canada", "India", "Germany",
        "France", "Australia", "Ireland", "Netherlands", "Singapore", "Japan", "Brazil", "Mexico",
        "Spain", "Italy", "Sweden", "Norway", "Denmark", "Finland", "Switzerland", "Poland",
        "Portugal", "New Zealand", "South Africa"
    };

    private static readonly Regex DateRange = new(
        @"(?<start>(?:jan(?:uary)?|feb(?:ruary)?|mar(?:ch)?|apr(?:il)?|may|jun(?:e)?|jul(?:y)?|aug(?:ust)?|sep(?:t(?:ember)?)?|oct(?:ober)?|nov(?:ember)?|dec(?:ember)?)\.?\s+(?:19|20)\d{2}|(?:0?[1-9]|1[0-2])[/-](?:19|20)\d{2})\s*(?:-|–|—|to)+\s*(?<end>present|current|(?:jan(?:uary)?|feb(?:ruary)?|mar(?:ch)?|apr(?:il)?|may|jun(?:e)?|jul(?:y)?|aug(?:ust)?|sep(?:t(?:ember)?)?|oct(?:ober)?|nov(?:ember)?|dec(?:ember)?)\.?\s+(?:19|20)\d{2}|(?:0?[1-9]|1[0-2])[/-](?:19|20)\d{2})",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex YearRange = new(
        @"\b(?<start>(?:19|20)\d{2})\s*(?:-|–|—|to)+\s*(?<end>present|current|(?:19|20)\d{2})\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex Degree = new(
        @"\b(?:bachelor|bachelors|master|masters|doctor|doctorate|associate|diploma|ph\.?d|mba|b\.?\s?s\.?|b\.?\s?a\.?|m\.?\s?s\.?|m\.?\s?a\.?|b\.?\s?sc|m\.?\s?sc|b\.?\s?tech|m\.?\s?tech|bsc|msc|btech|mtech)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static ResumeProfile Read(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Empty();

        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        var section = "preamble";
        var preamble = new List<string>();
        var experience = new List<string>();
        var education = new List<string>();
        var skills = new List<string>();
        var certifications = new List<string>();
        var sawExperience = false;

        foreach (var raw in lines)
        {
            var line = raw.Trim();
            if (line.Length == 0)
            {
                if (section == "experience")
                    experience.Add("");
                else if (section == "education")
                    education.Add("");
                else if (section == "certifications")
                    certifications.Add("");
                continue;
            }

            var kind = HeadingLines.Kind(line);
            if (kind == "experience")
            {
                section = "experience";
                sawExperience = true;
                continue;
            }

            if (kind == "education")
            {
                section = "education";
                continue;
            }

            if (kind == "skills")
            {
                section = "skills";
                continue;
            }

            if (kind == "summary")
            {
                section = kind;
                continue;
            }

            if (kind == "certifications")
            {
                section = "certifications";
                continue;
            }

            switch (section)
            {
                case "preamble":
                    preamble.Add(line);
                    break;
                case "experience":
                    experience.Add(line);
                    break;
                case "education":
                    education.Add(line);
                    break;
                case "skills":
                    skills.Add(line);
                    break;
                case "certifications":
                    certifications.Add(line);
                    break;
            }
        }

        var jobs = JobsFrom(sawExperience ? experience : lines.Select(line => line.Trim()).Where(line => HeadingLines.Kind(line) is null).ToList());
        var email = Patterns.Email.Match(text);
        var phone = Patterns.Phone.Match(text);
        return new ResumeProfile(
            preamble.FirstOrDefault(IsName),
            email.Success ? email.Value : null,
            phone.Success ? phone.Value : null,
            FindLocation(text),
            Links(text),
            SkillItems(skills, experience),
            jobs,
            EducationEntries(education),
            CertificationEntries(certifications),
            Months(jobs));
    }

    private static ResumeProfile Empty()
    {
        return new ResumeProfile(
            null, null, null, null,
            Array.Empty<string>(), Array.Empty<string>(),
            Array.Empty<JobPost>(), Array.Empty<EducationEntry>(), Array.Empty<CertificationEntry>(), null);
    }

    internal static string? FindLocation(string text)
    {
        foreach (Match match in CityRegion.Matches(text))
        {
            var region = match.Groups["region"].Value;
            if (!Regions.Contains(region))
                continue;

            return match.Groups["city"].Value + ", " + region;
        }

        return null;
    }

    private static IReadOnlyList<string> SkillItems(IReadOnlyList<string> lines, IReadOnlyList<string> experience)
    {
        var skills = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines)
            AddListed(line, skills, seen, listedOnly: false);

        foreach (var line in experience)
        {
            var trimmed = line.Trim().TrimStart('-', '*', '•', '·').Trim();
            if (trimmed.Length == 0 || DateRange.IsMatch(trimmed))
                continue;
            if (trimmed.Contains(',') || trimmed.Contains(';') || trimmed.Contains('|'))
                AddListed(trimmed, skills, seen, listedOnly: true);
            else
                AddKnownWords(trimmed, skills, seen);
        }

        return skills;
    }

    private static void AddListed(string line, List<string> skills, HashSet<string> seen, bool listedOnly)
    {
        foreach (var part in line.Split(',', ';', '|', '•', '·'))
        {
            var skill = part.Trim().Trim('-', '*', ' ');
            if (skill.Length is < 2 or > 40)
                continue;
            if (listedOnly && !HasKnownSkill(skill))
                continue;
            if (seen.Add(skill) )
                skills.Add(skill);
            if (skills.Count == 40)
                return;
        }
    }

    private static void AddKnownWords(string line, List<string> skills, HashSet<string> seen)
    {
        foreach (Match match in Regex.Matches(line, @"[A-Za-z0-9#+]+"))
        {
            if (!SkillWords.Words.Contains(match.Value))
                continue;
            if (seen.Add(match.Value))
                skills.Add(match.Value);
            if (skills.Count == 40)
                return;
        }
    }

    private static bool HasKnownSkill(string skill)
    {
        foreach (Match match in Regex.Matches(skill, @"[A-Za-z0-9#+]+"))
        {
            if (SkillWords.Words.Contains(match.Value))
                return true;
        }

        return false;
    }

    private static List<JobPost> JobsFrom(IReadOnlyList<string> lines)
    {
        var jobs = new List<JobPost>();
        foreach (var block in Blocks(lines))
        {
            string? start = null;
            string? end = null;
            string? title = null;
            var others = new List<string>();
            foreach (var line in block)
            {
                var match = DateRange.Match(line);
                if (!match.Success)
                {
                    others.Add(line);
                    continue;
                }

                start ??= match.Groups["start"].Value;
                end ??= match.Groups["end"].Value;
                var stripped = DateRange.Replace(line, " ").Trim().Trim(' ', ',', '|', '-', '–', '—', '·');
                if (stripped.Length > 0)
                    title ??= stripped;
            }

            if (start is null || end is null)
                continue;

            title ??= others.FirstOrDefault();
            if (string.IsNullOrWhiteSpace(title))
                continue;

            var employer = others.FirstOrDefault(line => !string.Equals(line, title, StringComparison.Ordinal));
            jobs.Add(new JobPost(title, employer, start, end));
        }

        return jobs;
    }

    private static List<List<string>> Blocks(IReadOnlyList<string> lines)
    {
        var blocks = new List<List<string>>();
        var current = new List<string>();
        foreach (var line in lines)
        {
            if (line.Length == 0)
            {
                if (current.Count > 0)
                    blocks.Add(current);
                current = new List<string>();
                continue;
            }

            current.Add(line);
        }

        if (current.Count > 0)
            blocks.Add(current);
        return blocks;
    }

    private static IReadOnlyList<EducationEntry> EducationEntries(IReadOnlyList<string> lines)
    {
        var entries = new List<EducationEntry>();
        foreach (var block in Blocks(lines))
        {
            string? start = null;
            string? end = null;
            string? degree = null;
            var rest = new List<string>();
            foreach (var line in block)
            {
                if (TakeRange(line, ref start, ref end))
                    continue;
                if (Degree.IsMatch(line) && !line.Contains(','))
                {
                    degree ??= line;
                    continue;
                }

                var parts = line.Split(',');
                if (parts.Length == 2 && (Degree.IsMatch(parts[0]) || Degree.IsMatch(parts[1])))
                {
                    degree ??= (Degree.IsMatch(parts[0]) ? parts[0] : parts[1]).Trim();
                    rest.Add((Degree.IsMatch(parts[0]) ? parts[1] : parts[0]).Trim());
                    continue;
                }

                rest.Add(line);
            }

            var school = rest.FirstOrDefault() ?? degree;
            if (string.IsNullOrWhiteSpace(school))
                continue;
            if (string.Equals(degree, school, StringComparison.Ordinal))
                degree = null;
            entries.Add(new EducationEntry(school, degree, start, end));
        }

        return entries;
    }

    private static IReadOnlyList<CertificationEntry> CertificationEntries(IReadOnlyList<string> lines)
    {
        var entries = new List<CertificationEntry>();
        foreach (var block in Blocks(lines))
        {
            string? date = null;
            var names = new List<string>();
            foreach (var line in block)
            {
                string? start = null;
                string? end = null;
                if (TakeRange(line, ref start, ref end))
                {
                    date ??= start + (end is null ? "" : " – " + end);
                    continue;
                }

                var single = Patterns.MonthYear.Match(line);
                if (single.Success && Patterns.MonthYear.Replace(line, "").Trim().Trim(',', '-', '–', '—').Length == 0)
                {
                    date ??= single.Value;
                    continue;
                }

                var comma = line.LastIndexOf(',');
                if (comma > 0 && Patterns.MonthYear.IsMatch(line[(comma + 1)..]))
                {
                    names.Add(line[..comma].Trim());
                    date ??= Patterns.MonthYear.Match(line).Value;
                    continue;
                }

                names.Add(line);
            }

            var name = string.Join(" ", names).Trim();
            if (name.Length == 0)
                continue;
            entries.Add(new CertificationEntry(name, date));
        }

        return entries;
    }

    private static bool TakeRange(string line, ref string? start, ref string? end)
    {
        var range = DateRange.Match(line);
        if (range.Success && DateRange.Replace(line, "").Trim().Length == 0)
        {
            start ??= range.Groups["start"].Value;
            end ??= range.Groups["end"].Value;
            return true;
        }

        var years = YearRange.Match(line);
        if (years.Success && YearRange.Replace(line, "").Trim().Length == 0)
        {
            start ??= years.Groups["start"].Value;
            end ??= years.Groups["end"].Value;
            return true;
        }

        return false;
    }

    private static IReadOnlyList<string> Links(string text)
    {
        return Link.Matches(text).Select(match => match.Value.TrimEnd('.', ',')).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
    }

    private static bool IsName(string line)
    {
        if (line.Contains('@', StringComparison.Ordinal) || line.Contains("http", StringComparison.OrdinalIgnoreCase))
            return false;

        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length is < 2 or > 4)
            return false;

        return parts.All(part => part.All(character => char.IsLetter(character) || character is '\'' or '-' or '.'));
    }

    private static int? Months(IReadOnlyList<JobPost> jobs)
    {
        var total = 0;
        var any = false;
        foreach (var job in jobs)
        {
            if (!TryMonth(job.Start, out var start))
                continue;

            DateOnly end;
            if (job.End.Equals("present", StringComparison.OrdinalIgnoreCase) || job.End.Equals("current", StringComparison.OrdinalIgnoreCase))
                end = DateOnly.FromDateTime(DateTime.Today);
            else if (!TryMonth(job.End, out end))
                continue;

            var months = (end.Year - start.Year) * 12 + (end.Month - start.Month);
            if (months < 0)
                continue;

            total += months;
            any = true;
        }

        return any ? total : null;
    }

    private static bool TryMonth(string text, out DateOnly date)
    {
        if (DateTime.TryParseExact(text, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var numeric)
            || DateTime.TryParseExact(text, "M/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out numeric))
        {
            date = DateOnly.FromDateTime(numeric);
            return true;
        }

        if (DateTime.TryParseExact(text, new[] { "MMM yyyy", "MMMM yyyy" }, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var named))
        {
            date = DateOnly.FromDateTime(named);
            return true;
        }

        date = default;
        return false;
    }
}
