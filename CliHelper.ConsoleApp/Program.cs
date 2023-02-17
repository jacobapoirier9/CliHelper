using CliHelper;
using System;
using System.Diagnostics;
using System.Reflection.PortableExecutable;

internal static class Program
{
    private static void Main(string[] args)
    {
        args = new string[] { "indextwo" };

        var client = Client.Create()
            .AddControllers()
            .AddControllers(typeof(ControllerOne))
            .Configure(options =>
            {
            })
            .Run(args);
    }
}

public class ControllerOne
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
        var result = UserChoice("Hello");
        Console.WriteLine(result);
        Console.WriteLine("two");
    }
}