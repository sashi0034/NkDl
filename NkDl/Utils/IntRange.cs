#nullable enable

namespace NkDl.Utils;

public record IntRange(
    int Start,
    int End)
{
    public int Clamp(int value)
    {
        return Math.Max(Start, Math.Min(value, End));
    }

    public IntRange Clamp(IntRange target)
    {
        return new IntRange(Math.Max(Start, target.Start), Math.Min(End, target.End));
    }
}