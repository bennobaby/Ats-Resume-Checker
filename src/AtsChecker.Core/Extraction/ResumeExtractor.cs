using UglyToad.PdfPig;

namespace AtsChecker.Core;

public sealed class ResumeExtractor : IResumeExtractor
{
    public ExtractedResume Extract(ResumeInput input)
    {
        return input.Source switch
        {
            ResumeSource.PlainText => Plain(input),
            ResumeSource.Pdf => Pdf(input),
            ResumeSource.Docx => throw new NotImplementedException(),
            _ => throw new ResumeUnreadableException()
        };
    }

    private static ExtractedResume Plain(ResumeInput input)
    {
        var text = input.PastedText ?? "";
        return new ExtractedResume(
            ResumeSource.PlainText,
            text,
            WordCounter.Count(text),
            new StructureFacts(0, 0, false, false, false));
    }

    private static ExtractedResume Pdf(ResumeInput input)
    {
        if (input.Content is null)
            throw new ResumeUnreadableException();

        try
        {
            using var buffer = Copy(input.Content);
            using var document = PdfDocument.Open(buffer);
            var pages = new List<string>();
            foreach (var page in document.GetPages())
            {
                var words = page.GetWords()
                    .Select(word => new PositionedWord(
                        word.Text,
                        word.BoundingBox.Left,
                        word.BoundingBox.Bottom,
                        word.BoundingBox.Right,
                        word.BoundingBox.Top))
                    .ToList();
                pages.Add(LineGrouper.Join(words));
            }

            var text = string.Join("\n\n", pages);
            return new ExtractedResume(
                ResumeSource.Pdf,
                text,
                WordCounter.Count(text),
                new StructureFacts(0, 0, false, false, false));
        }
        catch (ResumeUnreadableException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ResumeUnreadableException("This file could not be read.", ex);
        }
    }

    private static MemoryStream Copy(Stream content)
    {
        var buffer = new MemoryStream();
        content.CopyTo(buffer);
        buffer.Position = 0;
        return buffer;
    }
}
