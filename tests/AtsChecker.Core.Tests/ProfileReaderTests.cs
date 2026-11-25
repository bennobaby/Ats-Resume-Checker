namespace AtsChecker.Core.Tests;

public class ProfileReaderTests
{
    [Fact]
    public void Reads_name_contact_jobs_education_and_years()
    {
        var text = """
            Ada Example
            ada@example.com
            415-555-1212
            https://linkedin.com/in/ada
            Experience
            Software Engineer
            Acme
            Jan 2020 - Jan 2022
            Built tools
            Education
            State University
            """;

        var profile = ProfileReader.Read(text);

        Assert.Equal("Ada Example", profile.Name);
        Assert.Equal("ada@example.com", profile.Email);
        Assert.Equal("415-555-1212", profile.Phone);
        Assert.Contains("https://linkedin.com/in/ada", profile.Links);
        var job = Assert.Single(profile.Jobs);
        Assert.Equal("Software Engineer", job.Title);
        Assert.Equal("Acme", job.Employer);
        Assert.Equal("Jan 2020", job.Start);
        Assert.Equal("Jan 2022", job.End);
        Assert.Equal(24, profile.ExperienceMonths);
        var school = Assert.Single(profile.Education);
        Assert.Equal("State University", school.School);
        Assert.Null(school.Degree);
        Assert.Empty(profile.Certifications);
        Assert.Empty(profile.Skills);
        Assert.Null(profile.Location);
    }

    [Fact]
    public void Reads_city_and_skills_and_keeps_summary_out_of_the_name()
    {
        var text = """
            Ada Example
            Austin, TX
            Summary
            Built reliable systems
            Skills
            Python, SQL
            Experience
            Software Engineer
            Acme
            Jan 2020 - Jan 2022
            """;

        var profile = ProfileReader.Read(text);

        Assert.Equal("Ada Example", profile.Name);
        Assert.Equal("Austin, TX", profile.Location);
        Assert.Equal(new[] { "Python", "SQL" }, profile.Skills);
    }

    [Fact]
    public void Reads_degree_dates_certification_and_a_skill_from_a_bullet()
    {
        var text = """
            Experience
            Software Engineer
            Acme
            Jan 2020 - Jan 2022
            - Built Python services on Kubernetes
            Education
            State University
            Bachelor of Science in Computer Science
            Sep 2014 - May 2018
            Certifications
            AWS Certified Developer
            Jun 2021
            """;

        var profile = ProfileReader.Read(text);

        Assert.Contains("Python", profile.Skills);
        Assert.Contains("Kubernetes", profile.Skills);
        var school = Assert.Single(profile.Education);
        Assert.Equal("State University", school.School);
        Assert.Equal("Bachelor of Science in Computer Science", school.Degree);
        Assert.Equal("Sep 2014", school.Start);
        Assert.Equal("May 2018", school.End);
        var certificate = Assert.Single(profile.Certifications);
        Assert.Equal("AWS Certified Developer", certificate.Name);
        Assert.Equal("Jun 2021", certificate.Date);
    }

    [Fact]
    public void A_date_with_no_title_is_not_a_job()
    {
        var profile = ProfileReader.Read("Jan 2020 - Mar 2023");
        Assert.Empty(profile.Jobs);
        Assert.Null(profile.ExperienceMonths);
    }
}
