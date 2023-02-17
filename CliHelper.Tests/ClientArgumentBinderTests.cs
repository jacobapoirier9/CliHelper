//using System.Text.Json;

//namespace CliHelper.Tests;

//public class ClientArgumentBinderTests
//{
//    private static readonly string[] _args = new string[]
//    {
//        "--bool --bool-true:true --bool-yes=yes --bool-y y --bool-false:false --bool-no=no --bool-n n",
//        "--string-raw value --string-raw-path D:\\MyFile_{}.txt.bak",
//        "--string-single 'Value Value' --string-single-path 'D:\\MyFile_{}.txt.bak'",
//        "--string-double \"Value Value\" --string-double-path \"D:\\MyFile_{}.txt.bak\"",
//        "--int-value 5 --int-null-value 5",
//        "--byte-value 5 --byte-null-value 5",
//        "--short-value 5 --short-null-value 5",
//        "--long-value 5 --long-null-value 5",
//        "--double-value 5 --double-null-value 5",
//        "--float-value 5 --float-null-value 5",
//        "--decimal-value 5 --decimal-null-value 5",
//        "--timespan-value '10:10:10' --timespan-null-value '10:10:10'",
//        "--datetime-value '2/14/2023' --datetime-null-value '2/14/2023'",
//    };

//    private Request ParseRequest()
//    {
//        var client = Client.Create();
//        var argsAsString = string.Join(' ', _args);
//        var request = client.ExtractStronglyTypedInstance(typeof(Request), ref argsAsString) as Request;
//        return request;
//    }

//    [Fact]
//    public void ParseBooleanCorrectly()
//    {
//        var request = ParseRequest();

//        Assert.True(request.ImplicitTrue);
//        Assert.False(request.ImplicitFalse);
//        Assert.Null(request.ImplicitNull);
//        Assert.True(request.ExplicitTrue);
//        Assert.True(request.ExplicitYes);
//        Assert.True(request.ExplicitY);
//        Assert.False(request.ExplicitFalse);
//        Assert.False(request.ExplicitNo);
//        Assert.False(request.ExplicitN);
//    }

//    [Fact]
//    public void ParseStringCorrectly()
//    {
//        var request = ParseRequest();

//        Assert.Equal(@"value", request.StringRaw);
//        Assert.Equal(@"D:\MyFile_{}.txt.bak", request.StringRawPath);
//        Assert.Equal(@"Value Value", request.StringSingleQuotes);
//        Assert.Equal(@"D:\MyFile_{}.txt.bak", request.StringSingleQuotesPath);
//        Assert.Equal(@"Value Value", request.StringDoubleQuotes);
//        Assert.Equal(@"D:\MyFile_{}.txt.bak", request.StringDoubleQuotesPath);
//        Assert.Null(request.StringNull);
//    }

//    [Fact]
//    public void ParseByteCorrectly()
//    {
//        var request = ParseRequest();

//        Assert.Equal(5, request.byteValue);
//        Assert.Equal(0, request.byteDefault);
//        Assert.Equal((byte?)5, request.byteNullValue);
//        Assert.Null(request.byteNullDefault);
//    }

//    [Fact]
//    public void ParseShortCorrectly()
//    {
//        var request = ParseRequest();

//        Assert.Equal(5, request.shortValue);
//        Assert.Equal(0, request.shortDefault);
//        Assert.Equal((short?)5, request.shortNullValue);
//        Assert.Null(request.shortNullDefault);
//    }

//    [Fact]
//    public void ParseIntCorrectly()
//    {
//        var request = ParseRequest();

//        Assert.Equal(5, request.IntValue);
//        Assert.Equal(0, request.IntDefault);
//        Assert.Equal(5, request.IntNullValue);
//        Assert.Null(request.IntNullDefault);
//    }

//    [Fact]
//    public void ParseLongCorrectly()
//    {
//        var request = ParseRequest();

//        Assert.Equal(5, request.longValue);
//        Assert.Equal(0, request.longDefault);
//        Assert.Equal(5, request.longNullValue);
//        Assert.Null(request.longNullDefault);
//    }

//    [Fact]
//    public void ParseDoubleCorrectly()
//    {
//        var request = ParseRequest();

//        Assert.Equal(5, request.doubleValue);
//        Assert.Equal(0, request.doubleDefault);
//        Assert.Equal(5, request.doubleNullValue);
//        Assert.Null(request.doubleNullDefault);
//    }

//    [Fact]
//    public void ParseFloatCorrectly()
//    {
//        var request = ParseRequest();

//        Assert.Equal(5, request.floatValue);
//        Assert.Equal(0, request.floatDefault);
//        Assert.Equal(5, request.floatNullValue);
//        Assert.Null(request.floatNullDefault);
//    }

//    [Fact]
//    public void ParseDecimalCorrectly()
//    {
//        var request = ParseRequest();

//        Assert.Equal(5, request.decimalValue);
//        Assert.Equal(0, request.decimalDefault);
//        Assert.Equal(5, request.decimalNullValue);
//        Assert.Null(request.decimalNullDefault);
//    }

//    [Fact]
//    public void ParseTimeSpanCorrectly()
//    {
//        var request = ParseRequest();

//        Assert.Equal(TimeSpan.Parse("10:10:10"), request.timespanValue);
//        Assert.Equal(TimeSpan.Parse("00:00:00"), request.timespanDefault);
//        Assert.Equal(TimeSpan.Parse("10:10:10"), request.timespanNullValue);
//        Assert.Null(request.timespanNullDefault);

//    }

//    [Fact]
//    public void ParseDateTimeCorrectly()
//    {
//        var request = ParseRequest();

//        Assert.Equal(DateTime.Parse("2/14/2023 12:00:00 AM"), request.datetimeValue);
//        Assert.Equal(DateTime.MinValue, request.datetimeDefault);
//        Assert.Equal(DateTime.Parse("2/14/2023 12:00:00 AM"), request.datetimeNullValue);
//        Assert.Null(request.datetimeNullDefault);
//    }
//}

//public class Request
//{
//    [Cli("bool")]
//    public bool ImplicitTrue { get; set; }

//    [Cli("bool-not-found")]
//    public bool ImplicitFalse { get; set; }

//    [Cli("bool-not-found")]
//    public bool? ImplicitNull { get; set; }

//    [Cli("bool-true")]
//    public bool ExplicitTrue { get; set; }

//    [Cli("bool-yes")]
//    public bool ExplicitYes { get; set; }

//    [Cli("bool-y")]
//    public bool ExplicitY { get; set; }

//    [Cli("bool-false")]
//    public bool ExplicitFalse { get; set; }

//    [Cli("bool-no")]
//    public bool ExplicitNo { get; set; }

//    [Cli("bool-n")]
//    public bool ExplicitN { get; set; }




//    [Cli("string-raw")]
//    public string StringRaw { get; set; }

//    [Cli("string-raw-path")]
//    public string StringRawPath { get; set; }

//    [Cli("string-single")]
//    public string StringSingleQuotes { get; set; }

//    [Cli("string-single-path")]
//    public string StringSingleQuotesPath { get; set; }

//    [Cli("string-double")]
//    public string StringDoubleQuotes { get; set; }

//    [Cli("string-double-path")]
//    public string StringDoubleQuotesPath { get; set; }

//    [Cli("string-not-found")]
//    public string StringNull { get; set; }




//    [Cli("int-value")]
//    public int IntValue { get; set; }

//    [Cli("int-not-found")]
//    public int IntDefault { get; set; }

//    [Cli("int-null-value")]
//    public int? IntNullValue { get; set; }

//    [Cli("int-null-not-found")]
//    public int? IntNullDefault { get; set; }




//    [Cli("byte-value")]
//    public byte byteValue { get; set; }

//    [Cli("byte-not-found")]
//    public byte byteDefault { get; set; }

//    [Cli("byte-null-value")]
//    public byte? byteNullValue { get; set; }

//    [Cli("byte-null-not-found")]
//    public byte? byteNullDefault { get; set; }




//    [Cli("short-value")]
//    public short shortValue { get; set; }

//    [Cli("short-not-found")]
//    public short shortDefault { get; set; }

//    [Cli("short-null-value")]
//    public short? shortNullValue { get; set; }

//    [Cli("short-null-not-found")]
//    public short? shortNullDefault { get; set; }




//    [Cli("long-value")]
//    public long longValue { get; set; }

//    [Cli("long-not-found")]
//    public long longDefault { get; set; }

//    [Cli("long-null-value")]
//    public long? longNullValue { get; set; }

//    [Cli("long-null-not-found")]
//    public long? longNullDefault { get; set; }




//    [Cli("double-value")]
//    public double doubleValue { get; set; }

//    [Cli("double-not-found")]
//    public double doubleDefault { get; set; }

//    [Cli("double-null-value")]
//    public double? doubleNullValue { get; set; }

//    [Cli("double-null-not-found")]
//    public double? doubleNullDefault { get; set; }




//    [Cli("float-value")]
//    public float floatValue { get; set; }

//    [Cli("float-not-found")]
//    public float floatDefault { get; set; }

//    [Cli("float-null-value")]
//    public float? floatNullValue { get; set; }

//    [Cli("float-null-not-found")]
//    public float? floatNullDefault { get; set; }




//    [Cli("decimal-value")]
//    public decimal decimalValue { get; set; }

//    [Cli("decimal-not-found")]
//    public decimal decimalDefault { get; set; }

//    [Cli("decimal-null-value")]
//    public decimal? decimalNullValue { get; set; }

//    [Cli("decimal-null-not-found")]
//    public decimal? decimalNullDefault { get; set; }




//    [Cli("timespan-value")]
//    public TimeSpan timespanValue { get; set; }

//    [Cli("timespan-not-found")]
//    public TimeSpan timespanDefault { get; set; }

//    [Cli("timespan-null-value")]
//    public TimeSpan? timespanNullValue { get; set; }

//    [Cli("timespan-null-not-found")]
//    public TimeSpan? timespanNullDefault { get; set; }




//    [Cli("datetime-value")]
//    public DateTime datetimeValue { get; set; }

//    [Cli("datetime-not-found")]
//    public DateTime datetimeDefault { get; set; }

//    [Cli("datetime-null-value")]
//    public DateTime? datetimeNullValue { get; set; }

//    [Cli("datetime-null-not-found")]
//    public DateTime? datetimeNullDefault { get; set; }
//}