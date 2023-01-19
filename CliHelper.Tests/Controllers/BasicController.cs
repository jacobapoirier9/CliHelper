using CliHelper.Tests.Services;
using System.Reflection;

namespace CliHelper.Tests.Controllers;

[Cli("basic")]
public class BasicAliasController : CliController
{
    [Cli("index-one")]
    public string IndexOne() => nameof(IndexOne);

    [Cli("index-two")]
    public string IndexTwo() => nameof(IndexTwo);
}

public class BasicNoAliasController : CliController
{
    [Cli("index-one")]
    public string IndexOne() => nameof(IndexOne);

    public string IndexTwo() => nameof(IndexTwo);
}

public class BasicNotImplementedController
{

}

[Cli("dependency-injection")]
public class DependencyInjectionController : CliController
{
    private readonly ITestService _testService;
    public DependencyInjectionController(ITestService testService)
    {
        _testService = testService;
    }

    [Cli("constructor")]
    public string ReadFromConstructor()
    {
        return _testService.GetResponse();
    }

    [Cli("parameter")]
    public string ReadFromParameter(ITestService testService)
    {
        return testService.GetResponse();
    }
}

public class SimpleParametersController
{
    [Cli("as-int")]
    public string MethodWithInt(int number)
    {
        return number.ToString();
    }
}