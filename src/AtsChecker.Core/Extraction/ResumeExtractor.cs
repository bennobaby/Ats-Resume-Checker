namespace AtsChecker.Core;

public sealed class ResumeExtractor : IResumeExtractor
{
    public ExtractedResume Extract(ResumeInput input)
    {
        return input.Source switch
        {
            ResumeSource.PlainText => Plain(input),
            ResumeSource.Pdf => throw new NotImplementedException(),
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
}
