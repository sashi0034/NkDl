using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using NkDl.Utils;

namespace NkDl;

public record ContentTable(string Title, string[] Indexes)
{
}

public record struct StoryIndex(string Index, int Number);

public record DownloadingArgs(
    string Filename,
    string[] Indexes,
    Func<StoryIndex, Task<string>> StoryDownloader,
    Func<StoryIndex, string> StoryHeaderMaker,
    IntRange DownloadRange);

public static class DlCommon
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
        // CookieContainer と HttpClientHandler の設定
        var cookieContainer = new CookieContainer();
        using var handler = new HttpClientHandler() { CookieContainer = cookieContainer };
        using var httpClient = new HttpClient(handler);

        // ユーザーエージェントの設定
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.0.0 Safari/537.36");

        // クッキーの追加
        cookieContainer.Add(new Uri("https://novel18.syosetu.com"), new Cookie("over18", "yes"));

        // HTTP GETリクエストの送信とHTMLコンテンツの取得
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

    public static async Task ProcessDownload(DownloadingArgs args)
    {
        Console.WriteLine($"Start download [{args.DownloadRange.Start}, {args.DownloadRange.End}]");

        var textFilter = new TextFiler();
        await downloadAsync(args, textFilter);

        // AZW3へ変換
        textFilter.ConvertAll();
    }

    public static async Task downloadAsync(DownloadingArgs args, TextFiler textFiler)
    {
        var filename = args.Filename;
        var indexes = args.Indexes;
        var allText = "";
        string fileLower = (args.DownloadRange.Start).ToString();
        string fileUpper = "?";
        try
        {
            var random = new Random();
            for (int i = args.DownloadRange.Start - 1; i <= args.DownloadRange.End - 1; ++i)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Downloading... {i + 1}");

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
                    textFiler.Save(GetFilePath(filename, fileLower, fileUpper), allText);
                    if (i != indexes.Length - 1) fileLower = (i + 2).ToString();
                    allText = "";
                }

                // 適当な時間を待機
                await Task.Delay(random.NextInt64(0, 10) switch
                {
                    (<= 4) => DownloadInterval,
                    (<= 8) => (DownloadInterval * random.Next(2, 4)),
                    _ => DownloadInterval * random.Next(2, 16)
                });
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
        textFiler.Save(GetFilePath(filename, fileLower, fileUpper), allText);
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
        var title = Path.GetFileName(Path.ChangeExtension(txtPath, ""));
        var azw3Path = Path.ChangeExtension(txtPath, ".azw3");

        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "ebook-convert.exe",
            Arguments = "\"" + txtPath + "\" \"" + azw3Path + "\" --title " + title,
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