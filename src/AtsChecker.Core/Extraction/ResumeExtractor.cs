using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
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
            ResumeSource.Docx => Docx(input),
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

    private static ExtractedResume Docx(ResumeInput input)
    {
        if (input.Content is null)
            throw new ResumeUnreadableException();

        try
        {
            using var buffer = Copy(input.Content);
            using var doc = WordprocessingDocument.Open(buffer, false);
            var body = doc.MainDocumentPart?.Document?.Body
                ?? throw new ResumeUnreadableException();
            var text = string.Join("\n", BodyParagraphs(body).Select(ParagraphText));
            var facts = new StructureFacts(
                body.Descendants<Table>().Count(),
                body.Descendants().Count(element => element.LocalName == "txbxContent"),
                body.Descendants<Columns>().Any(columns => columns.ColumnCount is not null && columns.ColumnCount.Value > 1),
                HeaderOrFooterHasText(doc),
                true);
            return new ExtractedResume(ResumeSource.Docx, text, WordCounter.Count(text), facts);
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

    private static IEnumerable<OpenXmlElement> BodyParagraphs(Body body) =>
        body.Descendants().Where(element => element.LocalName == "p");

    private static string ParagraphText(OpenXmlElement paragraph) =>
        string.Concat(paragraph
            .Descendants()
            .Where(element => element.LocalName == "t")
            .Where(text => NearestParagraph(text) == paragraph)
            .Select(text => text.InnerText));

    private static OpenXmlElement? NearestParagraph(OpenXmlElement element)
    {
        for (var current = element.Parent; current is not null; current = current.Parent)
        {
            if (current.LocalName == "p")
                return current;
        }

        return null;
    }

    private static bool HeaderOrFooterHasText(WordprocessingDocument doc)
    {
        var main = doc.MainDocumentPart;
        if (main is null)
            return false;

        foreach (var header in main.HeaderParts)
        {
            if (!string.IsNullOrWhiteSpace(header.Header?.InnerText))
                return true;
        }

        foreach (var footer in main.FooterParts)
        {
            if (!string.IsNullOrWhiteSpace(footer.Footer?.InnerText))
                return true;
        }

        return false;
    }

    private static MemoryStream Copy(Stream content)
    {
        var buffer = new MemoryStream();
        content.CopyTo(buffer);
        buffer.Position = 0;
        return buffer;
    }
}
