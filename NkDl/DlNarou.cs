using HtmlAgilityPack;

namespace NkDl;

public record DlNarouProps(
    string NCode);

public class DlNarou : IDl
{
    private readonly ProgramArgs _programArgs;
    private readonly DlNarouProps _props;

    public DlNarou(ProgramArgs programArgs, DlNarouProps props)
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
        // タイトル取得
        string linkPattern = _props.NCode + @"/(\d+)/";
        var fetched = await DnLdCommon.FetchTitleAndIndexes(_programArgs.Url, linkPattern);

        await DnLdCommon.ProcessDownload(new DownloadingArgs(
            Title: fetched.Title,
            Indexes: fetched.Indexes,
            StoryDownloader: downloadStory,
            StoryHeaderMaker: storyLink => $"[{_props.NCode}/{storyLink.Index}]"));
    }

    private async Task<string> downloadStory(StoryIndex index)
    {
        var url = $"https://ncode.syosetu.com/{_props.NCode}/{index.Index}";
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Chrome");
        var htmlContent = await httpClient.GetStringAsync(url);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);

        var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@id='novel_honbun']");
        if (bodyNode == null) throw new ApplicationException("novel_honbun was not found: " + url);

        string storyText = "";

        // サブタイトル取得
        var subtitleNode = htmlDocument.DocumentNode.SelectSingleNode("//p[@class='novel_subtitle']");
        if (subtitleNode != null) storyText += subtitleNode.InnerHtml + "\n\n";

        // pタグを全て取得
        var pNodes = bodyNode.SelectNodes("./p");
        if (pNodes == null) throw new ApplicationException("p was not found in novel_honbun: " + url);

        foreach (var pNode in pNodes)
        {
            string next = pNode.InnerText;
            storyText += next.TrimStart() + "\n";
        }

        return storyText;
    }
}