namespace AtsChecker.Core;

public static class WordCounter
{
    public static int Count(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        var count = 0;
        var inToken = false;
        foreach (var ch in text)
        {
            if (char.IsWhiteSpace(ch))
            {
                inToken = false;
            }
            else if (!inToken)
            {
                inToken = true;
                count++;
            }
        }

        return count;
    }
}
