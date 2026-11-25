namespace AtsChecker.Core;

public static class ReadingOrder
{
    public static bool HasColumns(IReadOnlyList<PositionedWord> words)
    {
        if (words.Count < 12)
            return false;

        var centers = words.Select(word => (word.Left + word.Right) / 2).OrderBy(center => center).ToList();
        var bestGap = 0d;
        var split = 0;
        for (var i = 1; i < centers.Count; i++)
        {
            var gap = centers[i] - centers[i - 1];
            if (gap <= bestGap)
                continue;

            bestGap = gap;
            split = i;
        }

        var right = centers.Count - split;
        return bestGap >= 40 && split >= words.Count * 0.3 && right >= words.Count * 0.3;
    }

    public static bool HasTable(IReadOnlyList<PositionedWord> words)
    {
        var rows = words.GroupBy(word => Math.Round(word.Top / 4d)).Count(group => group.Count() >= 3);
        var columns = words.GroupBy(word => Math.Round(word.Left / 8d)).Count(group => group.Count() >= 3);
        return rows >= 3 && columns >= 3;
    }
}
