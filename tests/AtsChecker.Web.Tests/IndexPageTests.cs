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