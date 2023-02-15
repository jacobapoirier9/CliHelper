using CliHelper;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

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
            .AddServices(services =>
            {
                services.AddTransient<IService, Service>();
            })
            .Run(args);
    }

}
public interface IService
{

}
public class Service : IService
{

}

[Cli("advanced")]
public class AdvancedController : Controller
{
    public AdvancedController(IService service)
    {

    }

    public string Name { get; set; }

    public void Index(AdvancedController request, IService service)
    {
        Console.WriteLine("Here!");
    }

}

public class Request
{
    [Cli("bool")]
    public bool ImplicitTrue { get; set; }

    [Cli("bool-not-found")]
    public bool ImplicitFalse { get; set; }

    [Cli("bool-not-found")]
    public bool? ImplicitNull { get; set; }

    [Cli("bool-true")]
    public bool ExplicitTrue { get; set; }

    [Cli("bool-yes")]
    public bool ExplicitYes { get; set; }

    [Cli("bool-y")]
    public bool ExplicitY { get; set; }

    [Cli("bool-false")]
    public bool ExplicitFalse { get; set; }

    [Cli("bool-no")]
    public bool ExplicitNo { get; set; }

    [Cli("bool-n")]
    public bool ExplicitN { get; set; }




    [Cli("string-raw")]
    public string StringRaw { get; set; }

    [Cli("string-raw-path")]
    public string StringRawPath { get; set; }

    [Cli("string-single")]
    public string StringSingleQuotes { get; set; }

    [Cli("string-single-path")]
    public string StringSingleQuotesPath { get; set; }

    [Cli("string-double")]
    public string StringDoubleQuotes { get; set; }

    [Cli("string-double-path")]
    public string StringDoubleQuotesPath { get; set; }

    [Cli("string-not-found")]
    public string StringNull { get; set; }




    [Cli("int-value")]
    public int IntValue { get; set; }

    [Cli("int-not-found")]
    public int IntDefault { get; set; }

    [Cli("int-null-value")]
    public int? IntNullValue { get; set; }

    [Cli("int-null-not-found")]
    public int? IntNullDefault { get; set; }




    [Cli("byte-value")]
    public byte byteValue { get; set; }

    [Cli("byte-not-found")]
    public byte byteDefault { get; set; }

    [Cli("byte-null-value")]
    public byte? byteNullValue { get; set; }

    [Cli("byte-null-not-found")]
    public byte? byteNullDefault { get; set; }




    [Cli("short-value")]
    public short shortValue { get; set; }

    [Cli("short-not-found")]
    public short shortDefault { get; set; }

    [Cli("short-null-value")]
    public short? shortNullValue { get; set; }

    [Cli("short-null-not-found")]
    public short? shortNullDefault { get; set; }




    [Cli("long-value")]
    public long longValue { get; set; }

    [Cli("long-not-found")]
    public long longDefault { get; set; }

    [Cli("long-null-value")]
    public long? longNullValue { get; set; }

    [Cli("long-null-not-found")]
    public long? longNullDefault { get; set; }




    [Cli("double-value")]
    public double doubleValue { get; set; }

    [Cli("double-not-found")]
    public double doubleDefault { get; set; }

    [Cli("double-null-value")]
    public double? doubleNullValue { get; set; }

    [Cli("double-null-not-found")]
    public double? doubleNullDefault { get; set; }




    [Cli("float-value")]
    public float floatValue { get; set; }

    [Cli("float-not-found")]
    public float floatDefault { get; set; }

    [Cli("float-null-value")]
    public float? floatNullValue { get; set; }

    [Cli("float-null-not-found")]
    public float? floatNullDefault { get; set; }




    [Cli("decimal-value")]
    public decimal decimalValue { get; set; }

    [Cli("decimal-not-found")]
    public decimal decimalDefault { get; set; }

    [Cli("decimal-null-value")]
    public decimal? decimalNullValue { get; set; }

    [Cli("decimal-null-not-found")]
    public decimal? decimalNullDefault { get; set; }




    [Cli("timespan-value")]
    public TimeSpan timespanValue { get; set; }

    [Cli("timespan-not-found")]
    public TimeSpan timespanDefault { get; set; }

    [Cli("timespan-null-value")]
    public TimeSpan? timespanNullValue { get; set; }

    [Cli("timespan-null-not-found")]
    public TimeSpan? timespanNullDefault { get; set; }




    [Cli("datetime-value")]
    public DateTime datetimeValue { get; set; }

    [Cli("datetime-not-found")]
    public DateTime datetimeDefault { get; set; }

    [Cli("datetime-null-value")]
    public DateTime? datetimeNullValue { get; set; }

    [Cli("datetime-null-not-found")]
    public DateTime? datetimeNullDefault { get; set; }
}