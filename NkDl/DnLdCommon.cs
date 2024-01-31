using System.Diagnostics;
using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace NkDl;

public record ContentTable(string Title, string[] Indexes)
{
}

public record struct StoryIndex(string Index, int Number);

public record DownloadingArgs(
    string Title,
    string[] Indexes,
    Func<StoryIndex, Task<string>> StoryDownloader,
    Func<StoryIndex, string> StoryHeaderMaker);

public static class DnLdCommon
{
    public const int HugeCharacterLimit = 200_0000;
    public const int DownloadInterval = 300;

    public static string GetFilePath(string title, string lower, string upper)
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads",
            $"{title}_{lower}_{upper}.txt");
    }

    public static async Task<string> FetchHtmlContent(string url)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Chrome");
        var htmlContent = await httpClient.GetStringAsync(url);
        return htmlContent;
    }

    public static async Task<string?> TryFetchHtmlContent(string url)
    {
        try
        {
            return await FetchHtmlContent(url);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static async Task<ContentTable> FetchTitleAndIndexes(string url, string linkPattern)
    {
        var htmlContent = await FetchHtmlContent(url);

        // タイトル取得
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);
        var title = htmlDocument.DocumentNode.SelectSingleNode("//title").InnerText ?? "Unknown";

        Console.WriteLine($"Fetched {url} [{title}]");

        // 話数の数字配列を作成
        var links = Regex.Matches(htmlContent, linkPattern)
            .Select(match => match.Groups[1].Value)
            .Distinct()
            .ToArray();

        return new ContentTable(title, links);
    }

    public static async Task ProcessDownload(DownloadingArgs args)
    {
        var textFilter = new TextFiler();
        await downloadAsync(args, textFilter);

        // AZW3へ変換
        textFilter.ConvertAll();
    }

    public static async Task downloadAsync(DownloadingArgs args, TextFiler textFiler)
    {
        var title = args.Title;
        var indexes = args.Indexes;
        var allText = "";
        string fileLower = "1";
        string fileUpper = "?";
        try
        {
            for (int i = 0; i < indexes.Length; ++i)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Downloading... {i + 1} / {indexes.Length}");

                fileUpper = (i + 1).ToString();
                var storyIndex = new StoryIndex(indexes[i], i);
                string next = await args.StoryDownloader(storyIndex);

                // 強調表現(・・)を取り除く
                next = Regex.Replace(next, @"（・*）|\(・*\)", "");

                allText += args.StoryHeaderMaker(storyIndex) + "\n" + next + "\n";

                if (allText.Length > HugeCharacterLimit)
                {
                    // 文字数が多すぎるので、ファイルを分割
                    Console.WriteLine("\n");
                    textFiler.Save(GetFilePath(title, fileLower, fileUpper), allText);
                    if (i != indexes.Length - 1) fileLower = (i + 2).ToString();
                    allText = "";
                }

                await Task.Delay(DownloadInterval);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("\n");
            Console.WriteLine("Errored while downloading: " + e.Message);
            return;
        }

        // ファイル保存
        Console.WriteLine("\n");
        textFiler.Save(GetFilePath(title, fileLower, fileUpper), allText);
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
            process.WaitForExit();
            Console.WriteLine($"Finished conversion: {azw3Path}");
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to convert: " + e.Message);
        }
    }
}