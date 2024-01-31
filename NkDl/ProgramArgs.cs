namespace NkDl;

public record ProgramArgs(
    string Url = "")
{
    public const string UsageDescription = @"
Usage: nkdl <url>
";

    public static ProgramArgs FromParse(string[] arg)
    {
        var result = new ProgramArgs();

        for (int i = 0; i < arg.Length; ++i)
        {
            result = processNext(arg, result, i);
        }

        if (string.IsNullOrWhiteSpace(result.Url)) throw new ArgumentException("URL not specified");

        return result;
    }

    private static ProgramArgs processNext(string[] arg, ProgramArgs result, int nextIndex)
    {
        string next = arg[nextIndex];

        if (nextIndex == arg.Length - 1 && next.StartsWith("-") == false)
        {
            result = result with { Url = arg[0] };
        }

        return result;
    }
}