using CliHelper;

internal static class Program
{
    private static void Main(string[] args)
    {
        args = new string[] { "controllerone" };

        var client = Client.Create()
            .AddControllers()
            .Configure(options =>
            {
                options.RequireActionName = true;
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