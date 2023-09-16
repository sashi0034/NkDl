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
        var textFiler = new TextFiler();
        await startDownloadAll(title, links, textFiler);

        // AZW3へ変換
        textFiler.ConvertAll();
    }

    private async Task startDownloadAll(string title, string[] links, TextFiler textFiler)
    {
        var allText = "";
        string fileLower = links[0];
        string fileUpper = "?";
        try
        {
            for (int i = 0; i < links.Length; ++i)
            {
                Console.SetCursorPosition(0, Console.CursorTop);
                Console.Write($"Downloading... {i + 1} / {links.Length}");

                fileUpper = links[i];
                string next = await downloadStory($"https://ncode.syosetu.com/{_props.NCode}/{links[i]}");

                allText += $"[{_props.NCode}/{links[i]}]\n" + next + "\n";

                if (allText.Length > DlCommon.HugeCharacterLimit)
                {
                    // 文字数が多すぎるので、ファイルを分割
                    Console.WriteLine("\n");
                    textFiler.Save(DlCommon.GetFilePath(title, fileLower, fileUpper), allText);
                    if (i != links.Length - 1) fileLower = links[i + 1];
                    allText = "";
                }

                await Task.Delay(DlCommon.DownloadInterval);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine("\n");
            Console.WriteLine("errored while downloading: " + e.Message);
            return;
        }

        // ファイル保存
        Console.WriteLine("\n");
        textFiler.Save(DlCommon.GetFilePath(title, fileLower, fileUpper), allText);
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