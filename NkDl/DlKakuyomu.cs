using HtmlAgilityPack;

namespace NkDl;

public record DlKakuyomuProps(
    string Url,
    string WorkId);

public class DlKakuyomu : IDl
{
    private readonly InputStream _inputStream;
    private readonly DlKakuyomuProps _props;

    public DlKakuyomu(InputStream inputStream, DlKakuyomuProps props)
    {
        _inputStream = inputStream;
        _props = props;
    }

    public string PlatformName => "Kakuyomu";

    private const string UnnecessaryText = "- カクヨム";

    public void Execute()
    {
        processAsync().Wait();
    }

    private async Task processAsync()
    {
        // タイトル取得
        string linkPattern = _props.WorkId + @"/episodes/(\d+)";
        var fetched = await DlCommon.FetchTitleAndIndexes(_props.Url, linkPattern);
        var indexCount = fetched.Indexes.Length;

        var downloadRange = _inputStream.ReadDownloadRange(fetched.Indexes.Length);

        await DlCommon.ProcessDownload(new DownloadingArgs(
            Title: fetched.Title.Replace(UnnecessaryText, "").TrimStart().TrimEnd(),
            Indexes: fetched.Indexes,
            StoryDownloader: downloadStory,
            StoryHeaderMaker: storyLink => $"[episodes/{storyLink.Index} ({storyLink.Number + 1} / {indexCount})]",
            DownloadRange: downloadRange));
    }

    private async Task<string> downloadStory(StoryIndex index)
    {
        var url = $"https://kakuyomu.jp/works/{_props.WorkId}/episodes/{index.Index}";
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Chrome");
        var htmlContent = await httpClient.GetStringAsync(url);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);

        var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'widget-episodeBody')]");
        if (bodyNode == null) throw new ApplicationException("Body was not found: " + url);

        string storyText = "";

        // サブタイトル取得
        var subtitleNode =
            htmlDocument.DocumentNode.SelectSingleNode(
                "//p[@class='widget-episodeTitle js-vertical-composition-item']");
        if (subtitleNode != null) storyText += subtitleNode.InnerHtml.Replace("- カクヨム", "") + "\n\n";

        // pタグを全て取得
        var pNodes = bodyNode.SelectNodes("./p");
        if (pNodes == null) throw new ApplicationException("p was not found in body" + url);

        foreach (var pNode in pNodes)
        {
            string next = pNode.InnerText;
            storyText += next.TrimStart() + "\n";
        }

        return storyText;
    }
}