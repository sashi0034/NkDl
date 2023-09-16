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

        // 話数の数字配列を作成
        string linkPattern = _props.NCode + @"/(\d+)/";
        var links = Regex.Matches(htmlContent, linkPattern)
            .Select(match => match.Groups[1].Value)
            .ToArray();

        // ファイルパス決定
        string filePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            $"{title}_{links[0]}_{links[^1]}.txt");

        // ダウンロード
        var allText = "";
        for (int i = 0; i < links.Length; ++i)
        {
            string next = await downloadStory($"https://ncode.syosetu.com/{_props.NCode}/{links[i]}");

            allText += $"[{_props.NCode}/{links[i]}]\n" + next;
            await Task.Delay(DlCommon.DownloadInterval);
        }

        // ファイル保存
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