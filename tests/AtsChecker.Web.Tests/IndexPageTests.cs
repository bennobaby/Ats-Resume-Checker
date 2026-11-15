using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AtsChecker.Web.Tests;

public class IndexPageTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public IndexPageTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Posted_paste_shows_both_percents()
    {
        var html = await Post(new Dictionary<string, string>
        {
            ["ResumeText"] = "Ada@Example.com\nExperience\nEducation\nSkills\npython developer",
            ["JobDescription"] = "python developer"
        });

        Assert.Matches(@"id=""parseability"">Parseability \d+%", html);
        Assert.Matches(@"id=""keywords"">Keywords \d+%", html);
    }

    [Fact]
    public async Task Empty_post_asks_for_a_resume()
    {
        var html = await Post(new Dictionary<string, string>());
        Assert.Contains("Add a resume file or paste the resume text.", html);
        Assert.DoesNotContain("id=\"parseability\"", html);
    }

    [Fact]
    public async Task Wrong_extension_is_rejected()
    {
        var html = await PostFile("notes.txt", new byte[] { 1, 2, 3 }, resumeText: "pasted");
        Assert.Contains("Upload a PDF or Word (.docx) file.", html);
        Assert.DoesNotContain("id=\"parseability\"", html);
    }

    [Fact]
    public async Task Oversized_file_is_rejected()
    {
        var html = await PostFile("resume.pdf", new byte[5 * 1024 * 1024 + 1], resumeText: null);
        Assert.Contains("Resume files must be 5 MB or smaller.", html);
        Assert.DoesNotContain("id=\"parseability\"", html);
    }

    [Fact]
    public async Task Unreadable_pdf_shows_only_the_read_error()
    {
        var html = await PostFile("resume.pdf", new byte[] { 1, 2, 3, 4 }, resumeText: "pasted text");
        Assert.Contains("This file could not be read.", html);
        Assert.DoesNotContain("id=\"parseability\"", html);
    }

    [Fact]
    public async Task Uploaded_file_wins_over_pasted_text()
    {
        var pdf = MinimalPdf.Create("Hello ATS");
        var html = await PostFile("resume.PDF", pdf, resumeText: "this paste should be ignored");
        Assert.Contains(">PDF<", html);
        Assert.DoesNotContain(">Pasted text<", html);
    }

    private async Task<string> PostFile(string fileName, byte[] bytes, string? resumeText)
    {
        var client = _factory.CreateClient();
        var page = await client.GetAsync("/");
        var html = await page.Content.ReadAsStringAsync();
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(AntiforgeryToken(html)), "__RequestVerificationToken");
        if (resumeText is not null)
            form.Add(new StringContent(resumeText), "ResumeText");
        var file = new ByteArrayContent(bytes);
        file.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        form.Add(file, "ResumeFile", fileName);
        var response = await client.PostAsync("/", form);
        return await response.Content.ReadAsStringAsync();
    }

    private async Task<string> Post(Dictionary<string, string> fields)
    {
        var client = _factory.CreateClient();
        var page = await client.GetAsync("/");
        var html = await page.Content.ReadAsStringAsync();
        fields["__RequestVerificationToken"] = AntiforgeryToken(html);
        var response = await client.PostAsync("/", new FormUrlEncodedContent(fields));
        return await response.Content.ReadAsStringAsync();
    }

    internal static string AntiforgeryToken(string html)
    {
        var tokenThenValue = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        if (tokenThenValue.Success)
            return tokenThenValue.Groups[1].Value;
        return Regex.Match(html, "value=\"([^\"]+)\"[^>]*name=\"__RequestVerificationToken\"").Groups[1].Value;
    }
}