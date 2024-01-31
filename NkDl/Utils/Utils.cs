#nullable enable

namespace NkDl.Utils;

public static class Utils
{
    public static bool IsNullOrWhiteSpace(this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }
}