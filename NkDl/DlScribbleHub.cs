#nullable enable

using System.Text.RegularExpressions;
using System.Web;
using HtmlAgilityPack;

namespace NkDl;

public record DlDlScribbleHubProps(
    string Url,
    string WorkId,
    string Title);

public class DlScribbleHub : IDl
{
    private readonly InputStream _inputStream;
    private readonly DlDlScribbleHubProps _props;

    public DlScribbleHub(InputStream inputStream, DlDlScribbleHubProps props)
    {
        _inputStream = inputStream;
        _props = props;
    }

    public string PlatformName => "Scribble Hub";

    public void Execute()
    {
        processAsync().Wait();
    }

    private async Task processAsync()
    {
        // タイトル取得
        var fetched = await fetchTitleAndIndexes(_props.Url);
        var indexCount = fetched.Indexes.Length;

        var downloadRange = _inputStream.ReadDownloadRange(fetched.Indexes.Length);
        var filename = _inputStream.ReadFileName(_props.WorkId + "-" + _props.Title.Trim());

        await DlCommon.ProcessDownload(new DownloadingArgs(
            Filename: filename,
            Indexes: fetched.Indexes,
            StoryDownloader: downloadStory,
            StoryHeaderMaker: storyLink =>
                $"[{getChapterLinkTitle(storyLink.Index)} ({storyLink.Number + 1} / {indexCount})]",
            DownloadRange: downloadRange,
            true,
            4000));
    }

    private static string getChapterLinkTitle(string uri)
    {
        var links = Regex.Match(uri, @"chapter/(\d+)");
        return "chapter/" + links.Groups[1].Value ?? "Unknown";
    }

    public static async Task<ContentTable> fetchTitleAndIndexes(string url)
    {
        string pattern = @"<a\s+class=""toc_a""\s+href=""(https://www\.scribblehub\.com/read/[^""]+)""";
        string title = url;
        var allLinks = new List<string>();

        // 1ページ目から順にメタ情報を取得していく
        for (int i = 1;; ++i)
        {
            var pageUrl = $"{url}/?toc={i}";
            var htmlContent = await DlCommon.TryFetchHtmlContent(pageUrl);
            if (htmlContent == null) break;

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            if (i == 1)
            {
                // タイトル取得
                title = htmlDocument.DocumentNode.SelectSingleNode("//title").InnerText ?? "Unknown";
                title = HttpUtility.HtmlDecode(title);

                const string siteName = "| Scribble Hub";
                if (title.EndsWith(siteName)) title = title.Substring(0, title.Length - siteName.Length);
                title = title.Trim();

                Console.WriteLine($"Fetched title: {title}");
            }

            // リンク取得
            var links = Regex.Matches(htmlContent, pattern)
                .Select(match => match.Groups[1].Value)
                .Where(link => allLinks.Contains(link) == false)
                .Distinct()
                .ToList();

            Console.WriteLine($"Fetched {links.Count} from {pageUrl}");

            if (links.Count == 0) break;

            allLinks.AddRange(links);
        }

        // allLinks.Sort();

        return new ContentTable(title, allLinks.ToArray());
    }

    private async Task<string> downloadStory(StoryIndex index)
    {
        var url = index.Index;
        var htmlContent = await DlCommon.FetchHtmlContent(url);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);

        var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='chp_raw']");
        if (bodyNode == null) throw new ApplicationException("chp_raw was not found: " + url);

        string storyText = "";

        // サブタイトル取得
        var subtitleNode = htmlDocument.DocumentNode.SelectSingleNode("//div[contains(@class, 'chapter-title')]");
        if (subtitleNode != null) storyText += subtitleNode.InnerHtml + "\n\n";

        // pタグを全て取得
        var pNodes = bodyNode.SelectNodes(".//p");
        if (pNodes == null) throw new ApplicationException("p was not found in chp_raw: " + url);

        foreach (var pNode in pNodes)
        {
            string next = pNode.InnerText;
            storyText += next.TrimStart() + "\n\n";
        }

        return storyText;
    }
}