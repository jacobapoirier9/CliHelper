using CliHelper;

internal static class Program
{
    private static void Main(string[] args)
    {
        var client = Client.Create()
            .AddControllers()
            .Run(args);
    }
}