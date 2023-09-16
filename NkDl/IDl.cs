using System.Diagnostics;

namespace NkDl;

public interface IDl
{
    public string PlatformName { get; }
    public void Execute();
}

public static class DlCommon
{
    public const int HugeCharacterLimit = 150_0000;
    public const int DownloadInterval = 300;

    public static string GetFilePath(string title, string lower, string upper)
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads",
            $"{title}_{lower}_{upper}.txt");
    }
}

public class TextFiler
{
    private readonly List<string> _pathList = new();

    public void Save(string filePath, string allText)
    {
        saveTextFile(filePath, allText);
        _pathList.Add(filePath);
    }

    private static void saveTextFile(string filePath, string allText)
    {
        try
        {
            File.WriteAllText(filePath, allText);
            Console.WriteLine("Saved: " + filePath);
        }
        catch (IOException e)
        {
            Console.WriteLine("Save error: " + e.Message);
        }
    }

    public void ConvertAll()
    {
        foreach (var path in _pathList)
        {
            convertTxtToAzw3(path);
        }
    }

    private static void convertTxtToAzw3(string txtPath)
    {
        var azw3Path = Path.ChangeExtension(txtPath, ".azw3");

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "ebook-convert.exe",
            Arguments = "\"" + txtPath + "\" \"" + azw3Path + "\"",
            CreateNoWindow = false
        };

        Process process = new Process
        {
            StartInfo = psi,
        };

        Console.WriteLine($"Convert {txtPath} -> {azw3Path}");
        try
        {
            process.Start();
            Console.WriteLine($"Finished conversion: {azw3Path}");
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to convert: " + e.Message);
        }
    }
}