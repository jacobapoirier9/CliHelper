using CliHelper;
using System;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

internal static class Program
{
    private static void Main(string[] args)
    {
        args = new string[] { "help" };

        var client = Client.Create()
            .AddControllers()
            .Configure(options =>
            {
                options.InteractiveShellBanner = "Welcome to interactive shell";
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