namespace AtsChecker.Core.Tests;

public class ScoreMathTests
{
    [Fact]
    public void Percents_round_half_away_from_zero()
    {
        Assert.Equal(75, ScoreMath.PercentFromSeverities(new[] { Severity.Pass, Severity.Warn }));
        Assert.Equal(83, ScoreMath.PercentFromSeverities(new[] { Severity.Pass, Severity.Pass, Severity.Warn }));
        Assert.Equal(0, ScoreMath.PercentFromSeverities(new[] { Severity.Fail }));
        Assert.Equal(13, ScoreMath.PercentFromRatio(1, 8));
        Assert.Equal(67, ScoreMath.PercentFromRatio(2, 3));
    }
}
