using AtsChecker.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AtsChecker.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IResumeExtractor _extractor;
    private readonly IResumeChecker _checker;

    public IndexModel(IResumeExtractor extractor, IResumeChecker checker)
    {
        _extractor = extractor;
        _checker = checker;
    }

    [BindProperty]
    public IFormFile? ResumeFile { get; set; }

    [BindProperty]
    public string? ResumeText { get; set; }

    [BindProperty]
    public string? JobDescription { get; set; }

    public string? ErrorMessage { get; set; }
    public CheckReport? Report { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ResumeFile is null && string.IsNullOrWhiteSpace(ResumeText))
        {
            ErrorMessage = "Add a resume file or paste the resume text.";
            return Page();
        }

        try
        {
            if (ResumeFile is not null)
            {
                var extension = Path.GetExtension(ResumeFile.FileName);
                if (!extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)
                    && !extension.Equals(".docx", StringComparison.OrdinalIgnoreCase))
                {
                    ErrorMessage = "Upload a PDF or Word (.docx) file.";
                    return Page();
                }

                if (ResumeFile.Length > 5 * 1024 * 1024)
                {
                    ErrorMessage = "Resume files must be 5 MB or smaller.";
                    return Page();
                }

                await using var upload = ResumeFile.OpenReadStream();
                using var buffer = new MemoryStream();
                await upload.CopyToAsync(buffer);
                buffer.Position = 0;
                var source = extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase)
                    ? ResumeSource.Pdf
                    : ResumeSource.Docx;
                var extracted = _extractor.Extract(new ResumeInput(
                    source, buffer, null, ResumeFile.FileName, JobDescription ?? ""));
                Report = _checker.Check(extracted, JobDescription ?? "");
                return Page();
            }

            var pasted = _extractor.Extract(new ResumeInput(
                ResumeSource.PlainText, null, ResumeText, null, JobDescription ?? ""));
            Report = _checker.Check(pasted, JobDescription ?? "");
            return Page();
        }
        catch (ResumeUnreadableException)
        {
            ErrorMessage = "This file could not be read.";
            Report = null;
            return Page();
        }
    }
}