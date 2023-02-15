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

[Cli("advanced")]
public class AdvancedController : Controller
{
    public void Index(Execute execute)
    {
        using (var reader = execute.TextReader)
        {
            var text = reader.ReadToEnd();
            Console.WriteLine(text);
        }
    }

}

public class Execute
{
    public TextReader TextReader { get; set; }
}