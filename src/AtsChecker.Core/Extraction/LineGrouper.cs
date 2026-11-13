namespace AtsChecker.Core;

public readonly record struct PositionedWord(
    string Text,
    double Left,
    double Bottom,
    double Right,
    double Top);

public static class LineGrouper
{
    public static string Join(IReadOnlyList<PositionedWord> words)
    {
        if (words.Count == 0)
            return "";

        var ordered = words.OrderByDescending(word => word.Top).ThenBy(word => word.Left).ToList();
        var lines = new List<List<PositionedWord>>();
        List<PositionedWord>? current = null;
        var lineTop = 0d;
        var lineBottom = 0d;

        foreach (var word in ordered)
        {
            var overlaps = current is not null && word.Bottom < lineTop && word.Top > lineBottom;
            if (!overlaps)
            {
                current = new List<PositionedWord>();
                lines.Add(current);
                lineTop = word.Top;
                lineBottom = word.Bottom;
            }
            else
            {
                lineTop = Math.Max(lineTop, word.Top);
                lineBottom = Math.Min(lineBottom, word.Bottom);
            }

            current!.Add(word);
        }

        return string.Join(
            "\n",
            lines.Select(line => string.Join(" ", line.OrderBy(word => word.Left).Select(word => word.Text))));
    }
}
