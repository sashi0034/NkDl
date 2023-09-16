using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace NkDl;

public record DlNsProps(
    string NCode);

public class DlNs : IDl
{
    private readonly ProgramArgs _programArgs;
    private readonly DlNsProps _props;

    public DlNs(ProgramArgs programArgs, DlNsProps props)
    {
        _programArgs = programArgs;
        _props = props;
    }

    public string PlatformName => "Narou";

    public void Execute()
    {
        processAsync().Wait();
    }

    private async Task processAsync()
    {
        var url = _programArgs.Url;
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Chrome");
        var htmlContent = await httpClient.GetStringAsync(url);

        // タイトル取得
        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);
        var title = htmlDocument.DocumentNode.SelectSingleNode("//title").InnerText ?? "Unknown";

        Console.WriteLine($"fetched {url} [{title}]");

        // 話数の数字配列を作成
        string linkPattern = _props.NCode + @"/(\d+)/";
        var links = Regex.Matches(htmlContent, linkPattern)
            .Select(match => match.Groups[1].Value)
            .ToArray();

        // ダウンロード
        await startDownloadAll(title, links);
    }

    private async Task<string> startDownloadAll(string title, string[] links)
    {
        var allText = "";
        string fileUpper = links[0];
        try
        {
            for (int i = 0; i < links.Length; ++i)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Downloading... {i + 1} / {links.Length}");

                string next = await downloadStory($"https://ncode.syosetu.com/{_props.NCode}/{links[i]}");

                allText += $"[{_props.NCode}/{links[i]}]\n" + next + "\n";
                await Task.Delay(DlCommon.DownloadInterval);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("\n");
            Console.WriteLine("errored while downloading: " + e.Message);
            return allText;
        }

        // ファイル保存
        Console.WriteLine("\n");
        DlCommon.SaveTextFile(DlCommon.GetFilePath(title, fileUpper, links[^1]), allText);
        return allText;
    }

    async Task<string> downloadStory(string url)
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Chrome");
        var htmlContent = await httpClient.GetStringAsync(url);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);

        var divNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='novel_honbun']");
        if (divNode == null) throw new ApplicationException("novel_honbun was not found: " + url);

        // pタグを全て取得
        string storyText = "";
        var pNodes = divNode.SelectNodes("./p");
        if (pNodes == null) throw new ApplicationException("p was not found in novel_honbun: " + url);

        foreach (var pNode in pNodes)
        {
            string next = pNode.InnerText;
            storyText += next.TrimStart() + "\n";
        }

        return storyText;
    }
}