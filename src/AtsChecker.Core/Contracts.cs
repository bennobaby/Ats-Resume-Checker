namespace AtsChecker.Core;

public enum ResumeSource { Pdf, Docx, PlainText }

public enum Severity { Pass, Warn, Fail }

public sealed record ResumeInput(
    ResumeSource Source,
    Stream? Content,
    string? PastedText,
    string? FileName,
    string JobDescription);

public sealed record StructureFacts(
    int TableCount,
    int TextBoxCount,
    bool HasMultipleColumns,
    bool HeaderOrFooterHasText,
    bool LayoutInspected);

public sealed record ExtractedResume(
    ResumeSource Source,
    string Text,
    int WordCount,
    StructureFacts Facts);

public sealed record Finding(
    string RuleId,
    Severity Severity,
    string Title,
    string Explanation,
    string Evidence);

public sealed record KeywordMatch(
    bool WasRun,
    int? ScorePercent,
    IReadOnlyList<string> Found,
    IReadOnlyList<string> Missing,
    string? SkipReason);

public sealed record CheckReport(
    int ParseabilityPercent,
    KeywordMatch Keywords,
    IReadOnlyList<Finding> Findings,
    ResumeSource Source,
    int WordCount,
    string? LayoutNote);

public interface IResumeExtractor
{
    ExtractedResume Extract(ResumeInput input);
}

public interface IResumeChecker
{
    CheckReport Check(ExtractedResume resume, string jobDescription);
}

public sealed class ResumeUnreadableException : Exception
{
    public ResumeUnreadableException()
        : base("This file could not be read.")
    {
    }

    public ResumeUnreadableException(string message)
        : base(message)
    {
    }

    public ResumeUnreadableException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
