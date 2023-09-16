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
            return new DlNs(programArgs, new DlNsProps(ncode));
        }

        if (url.StartsWith("https://kakuyomu.jp/works/"))
        {
            // TODO
        }

        throw new ArgumentException();
    }
}