#nullable enable

namespace NkDl.Utils;

public static class Util
{
    public static bool IsNullOrWhiteSpace(this string? str)
    {
        return string.IsNullOrWhiteSpace(str);
    }

    public static bool IsValidFileName(string fileName)
    {
        // ファイル名に使用できない文字
        char[] invalidChars = Path.GetInvalidFileNameChars();

        if (fileName.Any(ch => invalidChars.Contains(ch)))
        {
            return false;
        }

        string[] reservedNames =
        {
            "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1",
            "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9"
        };

        string fileRoot = Path.GetFileNameWithoutExtension(fileName).ToUpperInvariant();
        if (reservedNames.Contains(fileRoot))
        {
            return false;
        }

        return true;
    }
}