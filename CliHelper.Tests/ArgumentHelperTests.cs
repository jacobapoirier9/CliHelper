namespace CliHelper.Tests;

public class ArgumentHelperTests
{
    [Fact]
    public void ParseCliArguments_ControllerAndAction()
    {
        var args = new string[] { "controller", "action" };

        var context = ArgumentHelper.ParseCliArguments(args);

        Assert.Equal("controller", context.CliController);
        Assert.Equal("action", context.CliAction);
        Assert.Empty(context.RemainingArgs);
    }

    [Fact]
    public void ParseCliArguments_ControllerAndActionAndRemainingArgs()
    {
        var args = new string[] { "controller", "action", "1", "2", "3" };

        var parsed = ArgumentHelper.ParseCliArguments(args);

        Assert.Equal("controller", parsed.CliController);
        Assert.Equal("action", parsed.CliAction);
        Assert.Collection(parsed.RemainingArgs,
            item => Assert.Equal("1", item),
            item => Assert.Equal("2", item),
            item => Assert.Equal("3", item));
    }

    [Fact]
    public void ConvertedValue_String_Value()
    {
        var value = ArgumentHelper.ConvertValue(typeof(string), "Hello");

        Assert.IsType<string>(value);
        Assert.Equal("Hello", value);
    }

    [Fact]
    public void ConvertedValue_String_Null()
    {
        var value = ArgumentHelper.ConvertValue(typeof(string), null);
        Assert.Null(value);
    }

    [Fact]
    public void ConvertedValue_Bool_Value()
    {
        var value = ArgumentHelper.ConvertValue(typeof(bool), "True");

        var result = Assert.IsType<bool>(value);
        Assert.True(result);
    }

    [Fact]
    public void ConvertedValue_Bool_Null()
    {
        var value = ArgumentHelper.ConvertValue(typeof(bool?), null);
        Assert.Null(value);
    }

    [Fact]
    public void ConvertedValue_Byte_Value()
    {
        var value = ArgumentHelper.ConvertValue(typeof(byte), byte.MaxValue.ToString());

        Assert.IsType<byte>(value);
        Assert.Equal(byte.MaxValue, value);
    }
    [Fact]
    public void ConvertedValue_Byte_Null()
    {
        var value = ArgumentHelper.ConvertValue(typeof(byte?), null);
        Assert.Null(value);
    }

    [Fact]
    public void ConvertedValue_Short_Value()
    {
        var value = ArgumentHelper.ConvertValue(typeof(short), short.MaxValue.ToString());

        Assert.IsType<short>(value);
        Assert.Equal(short.MaxValue, value);
    }
    [Fact]
    public void ConvertedValue_Short_Null()
    {
        var value = ArgumentHelper.ConvertValue(typeof(short?), null);
        Assert.Null(value);
    }

    [Fact]
    public void ConvertedValue_Int_Value()
    {
        var value = ArgumentHelper.ConvertValue(typeof(int), int.MaxValue.ToString());

        Assert.IsType<int>(value);
        Assert.Equal(int.MaxValue, value);
    }

    [Fact]
    public void ConvertedValue_Int_Null()
    {
        var value = ArgumentHelper.ConvertValue(typeof(int?), null);
        Assert.Null(value);
    }

    [Fact]
    public void ConvertedValue_Float_Value()
    {
        var value = ArgumentHelper.ConvertValue(typeof(float), float.MaxValue.ToString());

        Assert.IsType<float>(value);
        Assert.Equal(float.MaxValue, value);
    }
    [Fact]
    public void ConvertedValue_Float_Null()
    {
        var value = ArgumentHelper.ConvertValue(typeof(float?), null);
        Assert.Null(value);
    }

    [Fact]
    public void ConvertedValue_Double_Value()
    {
        var value = ArgumentHelper.ConvertValue(typeof(double), double.MaxValue.ToString());

        Assert.IsType<double>(value);
        Assert.Equal(double.MaxValue, value);
    }

    [Fact]
    public void ConvertedValue_Double_Null()
    {
        var value = ArgumentHelper.ConvertValue(typeof(double?), null);
        Assert.Null(value);
    }

    [Fact]
    public void ConvertedValue_Decimal_Value()
    {
        var value = ArgumentHelper.ConvertValue(typeof(decimal), decimal.MaxValue.ToString());

        Assert.IsType<decimal>(value);
        Assert.Equal(decimal.MaxValue, value);
    }
    [Fact]
    public void ConvertedValue_Decimal_Null()
    {
        var value = ArgumentHelper.ConvertValue(typeof(decimal?), null);
        Assert.Null(value);
    }

    [Fact]
    public void Bind_Aliased()
    {
        var args = new string[] { "-name", "Jake", "-salary", "2000", "-times", "20" };

        var dto = ArgumentHelper.Bind<AliasedArgumentsToBind>(args);

        Assert.Equal("Jake", dto.Name);
        Assert.Equal(20, dto.NumberOfTimes);
        Assert.Equal(2000m, dto.Salary);
    }

    [Fact]
    public void Bind_Unaliased()
    {
        var args = new string[] { "name", "Jake", "salary", "2000", "numberoftimes", "20" };

        var dto = ArgumentHelper.Bind<UnaliasedArgumentsToBind>(args);

        Assert.Equal("Jake", dto.Name);
        Assert.Equal(20, dto.NumberOfTimes);
        Assert.Equal(2000m, dto.Salary);
    }
}

public class AliasedArgumentsToBind
{
    [Cli("-name")]
    public string Name { get; set; }

    [Cli("-times")]
    public int NumberOfTimes { get; set; }

    [Cli("-salary")]
    public decimal Salary { get; set; }
}

public class UnaliasedArgumentsToBind
{
    public string Name { get; set; }

    public int NumberOfTimes { get; set; }

    public decimal Salary { get; set; }
}