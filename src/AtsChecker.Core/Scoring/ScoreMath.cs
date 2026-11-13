namespace AtsChecker.Core;

public static class ScoreMath
{
    public static int PercentFromSeverities(IReadOnlyList<Severity> severities)
    {
        decimal sum = 0;
        foreach (var severity in severities)
        {
            sum += severity switch
            {
                Severity.Pass => 1m,
                Severity.Warn => 0.5m,
                _ => 0m
            };
        }

        return Round(sum / severities.Count * 100m);
    }

    public static int PercentFromRatio(int numerator, int denominator)
    {
        return Round((decimal)numerator / denominator * 100m);
    }

    private static int Round(decimal value)
    {
        return (int)Math.Round(value, 0, MidpointRounding.AwayFromZero);
    }
}
