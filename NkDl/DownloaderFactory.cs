using System.Text.RegularExpressions;

namespace NkDl;

public static class DownloaderFactory
{
    public static IDl Create(InputStream inputStream)
    {
        var url = inputStream.ReadUrl();
        if (url.EndsWith("/")) url = url.TrimEnd('/');

        if (url.StartsWith("https://ncode.syosetu.com/"))
        {
            var ncode = Regex.Match(url, @"\/(n\w+)\/?$").Groups[1].Value;
            return new DlNarou(inputStream, new DlNarouProps(url, ncode));
        }

        if (url.StartsWith("https://novel18.syosetu.com/"))
        {
            var ncode = Regex.Match(url, @"\/(n\w+)\/?$").Groups[1].Value;
            return new DlNarou(inputStream, new DlNarouProps(url, ncode));
        }

        if (url.StartsWith("https://kakuyomu.jp/works/"))
        {
            var workId = Regex.Match(url, @"works/(\d+)").Groups[1].Value;
            return new DlKakuyomu(inputStream, new DlKakuyomuProps(url, workId));
        }

        if (url.StartsWith("https://www.royalroad.com/"))
        {
            var works = Regex.Match(url, @"https://www.royalroad.com/(\w+)/(\d+)/([\w-]+)");
            var workId = works.Groups[2].Value + "/" + works.Groups[3].Value;
            // TODO
        }

        if (url.StartsWith("https://www.scribblehub.com/series/"))
        {
            var works = Regex.Match(url, @"https://www.scribblehub.com/series/(\d+)/([\w-]+)");
            var workId = works.Groups[1].Value;
            var title = works.Groups[2].Value;
            return new DlScribbleHub(inputStream, new DlDlScribbleHubProps(url, workId, title));
        }

        throw new ArgumentException();
    }
}