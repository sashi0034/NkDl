#nullable enable

using NkDl.Utils;

namespace NkDl;

public class InputStream
{
    private readonly ProgramArgs? _programArgs;

    public InputStream(ProgramArgs? programArgs)
    {
        _programArgs = programArgs;
    }

    public string ReadUrl()
    {
        if (_programArgs != null) return _programArgs.Url;

        Console.WriteLine("Input URL for download novels:");
        while (true)
        {
            var url = Console.ReadLine();
            if (url != null) return url;
        }
    }

    public IntRange ReadDownloadRange(int pageSize)
    {
        if (_programArgs != null)
        {
            return new IntRange(
                _programArgs.FirstPage,
                _programArgs.LastPage >= 1 ? _programArgs.LastPage : pageSize);
        }

        var firstPage = inputFirstPage(pageSize);
        var lastPage = inputLastPage(pageSize, firstPage);
        return new IntRange(firstPage, lastPage);
    }

    private static int inputFirstPage(int pageSize)
    {
        int firstPage = 1;
        while (true)
        {
            Console.WriteLine($"Input download start page: [1, {pageSize}] (Default: {firstPage})");
            var input = Console.ReadLine();
            if (input.IsNullOrWhiteSpace()) break;
            if (int.TryParse(input, out firstPage)) break;
            Console.Error.WriteLine("Invalid format");
        }

        firstPage = new IntRange(1, pageSize).Clamp(firstPage);
        return firstPage;
    }

    private static int inputLastPage(int pageSize, int firstPage)
    {
        int lastPage = pageSize;
        while (true)
        {
            Console.WriteLine($"Input download end page: [{firstPage}, {pageSize}] (Default: {lastPage})");
            var input = Console.ReadLine();
            if (input.IsNullOrWhiteSpace()) break;
            if (int.TryParse(input, out lastPage)) break;
            Console.Error.WriteLine("Invalid format");
        }

        lastPage = new IntRange(firstPage, pageSize).Clamp(lastPage);
        return lastPage;
    }
}