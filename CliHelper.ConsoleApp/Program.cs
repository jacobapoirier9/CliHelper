using CliHelper;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

internal static class Program
{
    private static void Main(string[] args)
    {
        args = new string[] { "advanced", "index2", "5", "--name", "Jake" };

        var client = CliClient.Create()
            .AddControllers()
            .AddServices(services =>
            {
            })
            .Run(args);

    }
}

[Cli("advanced")]
public class AdvancedController : Controller
{
    [Cli("index")]
    public void Index(string name, int age)
    {
        Console.WriteLine("Here!");
    }

    [Cli("index2")]
    public void Index(Test request)
    {

    }
}

public class Test
{
    public string Name { get; set; }
    public int Age { get; set; }

}