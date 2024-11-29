using System.Text.RegularExpressions;
using HtmlAgilityPack;

namespace NkDl;

public record DlNarouProps(
    string Url,
    string NCode);

public class DlNarou : IDl
{
    private readonly InputStream _inputStream;
    private readonly DlNarouProps _props;

    public DlNarou(InputStream inputStream, DlNarouProps props)
    {
        _inputStream = inputStream;
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
        var fetched = await fetchTitleAndIndexes(_props.Url, _props.NCode);

        var downloadRange = _inputStream.ReadDownloadRange(fetched.Indexes.Length);
        var filename = _inputStream.ReadFileName(fetched.Title);

        await DlCommon.ProcessDownload(new DownloadingArgs(
            Filename: filename,
            Indexes: fetched.Indexes,
            StoryDownloader: downloadStory,
            StoryHeaderMaker: storyLink => $"[{_props.NCode}/{storyLink.Index}]",
            DownloadRange: downloadRange));
    }

    private static async Task<ContentTable> fetchTitleAndIndexes(string topUrl, string ncode)
    {
        string linkPattern = ncode + @"/(\d+)/";
        string title = "Unknown Title";
        var allLinks = new List<string>();

        // 1ページ目から順にメタ情報を取得していく
        for (int i = 1;; ++i)
        {
            var pageUrl = $"{topUrl}/?p={i}";
            var htmlContent = await DlCommon.TryFetchHtmlContent(pageUrl);
            if (htmlContent == null) break;

            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            if (i == 1)
            {
                // タイトル取得
                title = htmlDocument.DocumentNode.SelectSingleNode("//title").InnerText ?? "Unknown";
                Console.WriteLine($"Fetched title: {title}");
            }

            // リンク取得
            var links = Regex.Matches(htmlContent, linkPattern)
                .Select(match => match.Groups[1].Value)
                .Distinct()
                .ToList();
            allLinks.AddRange(links);

            Console.WriteLine($"Fetched {links.Count} from {pageUrl}");

            if (htmlContent.Contains($"{ncode}/?p={i + 1}") == false) break;
            if (links.Count == 0) break;
        }

        return new ContentTable(title, allLinks.ToArray());
    }

    private async Task<string> downloadStory(StoryIndex index)
    {
        var url = $"https://ncode.syosetu.com/{_props.NCode}/{index.Index}";
        var htmlContent = await DlCommon.FetchHtmlContent(url);

        var htmlDocument = new HtmlDocument();
        htmlDocument.LoadHtml(htmlContent);

        var bodyNode = htmlDocument.DocumentNode.SelectSingleNode("//div[@class='p-novel__body']");
        if (bodyNode == null) throw new ApplicationException("novel_honbun was not found: " + url);

        string storyText = "";

        // サブタイトル取得
        var subtitleNode = htmlDocument.DocumentNode.SelectSingleNode("//h1[contains(@class, 'p-novel__title')]");
        if (subtitleNode != null) storyText += subtitleNode.InnerHtml + "\n\n";

        // pタグを全て取得
        var pNodes = bodyNode.SelectNodes(".//p");
        if (pNodes == null) throw new ApplicationException("p was not found in novel_honbun: " + url);

        foreach (var pNode in pNodes)
        {
            string next = pNode.InnerText;
            storyText += next.TrimStart() + "\n\n";
        }

        return storyText;
    }
}