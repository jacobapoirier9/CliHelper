using CliHelper;

internal static class Program
{
    private static void Main(string[] args)
    {
        args = new string[] {  };

        var client = Client.Create()
            .AddControllers()
            .Configure(options =>
            {
            })
            .Run(args);
    }
}

public class ControllerOne : Controller
{
    public void IndexOne()
    {
        Console.WriteLine("one");
    }
}

public class ControllerTwo : Controller
{
    public void IndexTwo()
    {
        Console.WriteLine("two");
    }
}