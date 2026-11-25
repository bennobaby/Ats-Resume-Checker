namespace AtsChecker.Core.Tests;

public class ReadingOrderTests
{
    [Fact]
    public void Two_word_bands_are_columns()
    {
        var words = new List<PositionedWord>();
        for (var i = 0; i < 8; i++)
        {
            words.Add(new PositionedWord("left", 10, i, 40, i + 1));
            words.Add(new PositionedWord("right", 300, i, 340, i + 1));
        }

        Assert.True(ReadingOrder.HasColumns(words));
    }

    [Fact]
    public void A_single_column_is_not_columns()
    {
        var words = Enumerable.Range(0, 12)
            .Select(i => new PositionedWord("word", 40, i * 12, 80, i * 12 + 8))
            .ToList();

        Assert.False(ReadingOrder.HasColumns(words));
    }

    [Fact]
    public void Aligned_rows_and_columns_are_a_table()
    {
        var words = new List<PositionedWord>();
        for (var row = 0; row < 3; row++)
        {
            for (var column = 0; column < 3; column++)
                words.Add(new PositionedWord("cell", column * 80, row * 20, column * 80 + 30, row * 20 + 8));
        }

        Assert.True(ReadingOrder.HasTable(words));
    }
}
