using System.Text.RegularExpressions;

namespace NkDl;

public static class DownloaderFactory
{
    public static IDl Create(InputStream inputStream)
    {
        var url = inputStream.ReadUrl();
        if (url.StartsWith("https://ncode.syosetu.com/"))
        {
            var ncode = Regex.Match(url, @"\/(n\w+)\/?$").Groups[1].Value;
            return new DlNarou(inputStream, new DlNarouProps(url, ncode));
        }

        if (url.StartsWith("https://kakuyomu.jp/works/"))
        {
            var workId = Regex.Match(url, @"works/(\d+)").Groups[1].Value;
            return new DlKakuyomu(inputStream, new DlKakuyomuProps(url, workId));
        }

        throw new ArgumentException();
    }
}