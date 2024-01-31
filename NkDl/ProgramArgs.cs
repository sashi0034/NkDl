namespace NkDl;

public record ProgramArgs(
    string Url = "",
    int FirstPage = 1,
    int LastPage = 0,
    string NovelTitle = "")
{
    public const string UsageDescription = @"
Usage: nkdl <URL> [Options]
Options:
    --from      First page to download
    --to        Last page to download
";

    public static ProgramArgs FromParse(string[] arg)
    {
        var result = new ProgramArgs();

        for (int i = 0; i < arg.Length;)
        {
            string nextRead()
            {
                i++;
                return arg[i - 1];
            }

            result = processNext(i, nextRead, result);
        }

        if (string.IsNullOrWhiteSpace(result.Url)) throw new ArgumentException("URL not specified");

        return result;
    }

    private static ProgramArgs processNext(int nextIndex, Func<string> nextRead, ProgramArgs result)
    {
        if (nextIndex == 0)
        {
            return result with { Url = nextRead() };
        }
        else
        {
            return processNextOption(nextRead, result);
        }
    }

    private static ProgramArgs processNextOption(Func<string> nextRead, ProgramArgs result)
    {
        return nextRead() switch
        {
            "--from" => result with { FirstPage = int.Parse(nextRead()) },
            "--to" => result with { LastPage = int.Parse(nextRead()) },
            "--title" => result with { NovelTitle = nextRead() },
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}