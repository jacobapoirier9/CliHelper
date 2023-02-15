using CliHelper;

internal static class Program
{
    private static void Main(string[] args)
    {
        //using (var reader = Console.In)
        //{
        //    var text = reader.ReadToEnd();
        //    Console.WriteLine(text);
        //}

        args = new string[]
        {
            "advanced", "--name Jake --bool"
        };

        var client = Client.Create()
            .AddControllers()
            .Run(args);
    }
}

[Cli("advanced")]
public class AdvancedController : Controller
{
    public void Index()
    {
        Console.WriteLine("Here!");
    }

}