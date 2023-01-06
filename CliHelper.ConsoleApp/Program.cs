using CliHelper;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

internal static class Program
{
    private static void Main(string[] args)
    {
        args = new string[] { "index" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(AdvancedController))
            .AddServices(services =>
            {
            })
            .Run(args);

    }
}

[Cli("advanced")]
public class AdvancedController : CliController
{
    [Cli("index")]
    public void Index()
    {
        Console.WriteLine("Here!");
    }
}