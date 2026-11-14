using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AtsChecker.Core.Tests;

internal static class DocxFixtures
{
    public static byte[] WithLayoutTraps()
    {
        using var stream = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document, true))
        {
            var main = doc.AddMainDocumentPart();
            main.Document = new Document(new Body());
            var body = main.Document.Body!;
            body.AppendChild(new Paragraph(new Run(new Text("Visible body"))));
            body.AppendChild(new Table(new TableRow(new TableCell(
                new Paragraph(new Run(new Text("Inside table")))))));

            var run = new Run();
            run.AppendChild(new TextBoxContent(new Paragraph(new Run(new Text("Inside box")))));
            body.AppendChild(new Paragraph(run));

            var section = new SectionProperties(new Columns { ColumnCount = 2 });
            var headerPart = main.AddNewPart<HeaderPart>();
            headerPart.Header = new Header(new Paragraph(new Run(new Text("Secret Header"))));
            headerPart.Header.Save();
            section.PrependChild(new HeaderReference
            {
                Type = HeaderFooterValues.Default,
                Id = main.GetIdOfPart(headerPart)
            });
            body.AppendChild(section);
            main.Document.Save();
        }

        return stream.ToArray();
    }
}