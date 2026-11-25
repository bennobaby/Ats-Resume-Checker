using System.Globalization;

namespace AtsChecker.Core;

public static class FileSizeRule
{
    public const long ParserLimit = 2_621_440;

    public static Finding? ForLength(long bytes)
    {
        if (bytes <= ParserLimit)
            return null;

        var megabytes = bytes / (1024m * 1024m);
        return new Finding(
            "file-size",
            Severity.Warn,
            "File size",
            "Some ATS parsers skip files larger than 2.5 MB.",
            megabytes.ToString("0.0", CultureInfo.InvariantCulture) + " MB");
    }
}
