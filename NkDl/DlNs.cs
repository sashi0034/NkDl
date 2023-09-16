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

        var allText = "";
        for (int i = 0; i < links.Length; ++i)
        {
            allText += downloadStory("https://ncode.syosetu.com/n4006r/" + links[i]);
        }
    }

    async Task<string> downloadStory(string url)
    {
        // TODO
        return "";
    }
}