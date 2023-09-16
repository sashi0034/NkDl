namespace NkDl;

static class EntryPoint
{
    static void Main(string[] args)
    {
        ProgramArgs programArgs;
        try
        {
            programArgs = args.Length == 0
                ? ProgramArgs.FromStdin()
                : ProgramArgs.FromParse(args);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            Console.WriteLine(ProgramArgs.UsageDescription);
            return;
        }

        try
        {
            var dl = DownloaderFactory.Create(programArgs);
            Console.WriteLine("Detected: " + dl.PlatformName);
            dl.Execute();
            Console.WriteLine("Finished all tasks");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return;
        }
    }
}