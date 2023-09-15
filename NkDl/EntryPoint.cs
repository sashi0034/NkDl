namespace NkDl;

static class EntryPoint
{
    static void Main(string[] args)
    {
        ProgramArgs programArgs;
        try
        {
            programArgs = ProgramArgs.Parse(args);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            Console.WriteLine(ProgramArgs.UsageDescription);
            return;
        }

        Console.WriteLine(programArgs.Url);
    }
}