namespace NkDl;

public interface IDl
{
    public string PlatformName { get; }
    public void Execute();
}

public static class DlCommon
{
    public const int DownloadInterval = 300;

    public static string GetFilePath(string title, string lower, string upper)
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads",
            $"{title}_{lower}_{upper}.txt");
    }

    public static void SaveTextFile(string filePath, string allText)
    {
        try
        {
            File.WriteAllText(filePath, allText);
            Console.WriteLine("Save: " + filePath);
        }
        catch (IOException e)
        {
            Console.WriteLine("Save error: " + e.Message);
        }
    }
}