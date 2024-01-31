#nullable enable

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
}