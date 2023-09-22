using System.Text.RegularExpressions;

namespace NkDl;

public static class DownloaderFactory
{
    public static IDl Create(ProgramArgs programArgs)
    {
        var url = programArgs.Url;
        if (url.StartsWith("https://ncode.syosetu.com/"))
        {
            var ncode = Regex.Match(url, @"\/(n\w+)\/?$").Groups[1].Value;
            return new DlNarou(programArgs, new DlNarouProps(ncode));
        }

        if (url.StartsWith("https://kakuyomu.jp/works/"))
        {
            var workId = Regex.Match(url, @"works/(\d+)").Groups[1].Value;
            return new DlKakuyomu(programArgs, new DlKakuyomuProps(workId));
        }

        throw new ArgumentException();
    }
}