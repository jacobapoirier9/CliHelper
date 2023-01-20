using CliHelper;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

internal static class Program
{
    private static void Main(string[] args)
    {
        args = new string[] { "advanced", "index" };

        var client = CliClient.Create()
            .AddControllers()
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

    public void AllowComplex(Test request)
    {

    }

    public void AllowSimple(string name, int age)
    {

    }

    public void DisallowBoth(Test request, string name, int age)
    {

    }
}

public class Test
{
    public string Name { get; set; }
}