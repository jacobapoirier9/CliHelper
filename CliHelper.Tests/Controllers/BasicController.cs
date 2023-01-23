using CliHelper.Tests.Services;
using System.ComponentModel;
using System.Reflection;

namespace CliHelper.Tests.Controllers;

[Cli("basic")]
public class BasicAliasController : Controller
{
    [Cli("index-one")]
    public string IndexOne() => nameof(IndexOne);

    [Cli("index-two")]
    public string IndexTwo() => nameof(IndexTwo);
}

public class BasicNoAliasController : Controller
{
    [Cli("index-one")]
    public string IndexOne() => nameof(IndexOne);

    public string IndexTwo() => nameof(IndexTwo);
}

public class BasicNotImplementedController
{

}

[Cli("dependency-injection")]
public class DependencyInjectionController : Controller
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

public class SimpleParametersController : Controller
{
    [Cli("as-int")]
    public string MethodWithInt(int number)
    {
        return number.ToString();
    }

    [Cli("map-multiple")]
    public string MultipleParameters(string name, int? number, bool repeat)
    {
        if (number.HasValue)
        {
            if (!repeat)
                number = 1;

            var list = new List<string>();
            for (var i = 0; i < number; i++)
                list.Add(name);

            return number.ToString() + '-' + string.Join('-', list);
        }

        return name;
    }


    [Cli("map-multiple-alias")]
    public string MultipleParameters_CliAlias([Cli("--name")] string p1, [Cli("--number")] int? p2, [Cli("--repeat")] bool p3)
    {
        if (p2.HasValue)
        {
            if (!p3)
                p2 = 1;

            var list = new List<string>();
            for (var i = 0; i < p2; i++)
                list.Add(p1);

            return p2.ToString() + '-' + string.Join('-', list);
        }

        return p1;
    }
}

public class NoActionController : Controller
{

}

public class DuplicateActionController : Controller
{
    [Cli("same-name")]
    public void MethodOne() { }

    [Cli("same-name")]
    public void MethodTwo() { }
}

public class SimpleAndComplexController : Controller
{
    public class Request
    {

    }

    public void Index(Request request, string name) { }
}

[Cli("complex")]
public class ComplexParameterController : Controller
{
    public class Person
    {
        public string Name { get; set; }

        [Cli("-age")]
        public int? Age { get; set; }
    }

    public class MapMultipleNoAlias
    {
        public string Name { get; set; }

        public int? Number { get; set; }

        public bool Repeat { get; set; }
    }

    public class MapMultipleAlias
    {
        [Cli("--name")]
        public string P1 { get; set; }

        [Cli("--number")]
        public int? P2 { get; set; }

        [Cli("--repeat")]
        public bool P3 { get; set; }
    }

    [Cli("index")]
    public string Index(Person person)
    {
        return person.Name + "-" + person.Age;
    }


    [Cli("map-multiple")]
    public string MultipleParameters(MapMultipleNoAlias request)
    {
        if (request.Number.HasValue)
        {
            if (!request.Repeat)
                request.Number = 1;

            var list = new List<string>();
            for (var i = 0; i < request.Number; i++)
                list.Add(request.Name);

            return request.Number.ToString() + '-' + string.Join('-', list);
        }

        return request.Name;
    }

    [Cli("map-multiple-alias")]
    public string MultipleParameters_CliAlias(MapMultipleAlias request)
    {
        if (request.P2.HasValue)
        {
            if (!request.P3)
                request.P2 = 1;

            var list = new List<string>();
            for (var i = 0; i < request.P2; i++)
                list.Add(request.P1);

            return request.P2.ToString() + '-' + string.Join('-', list);
        }

        return request.P1;
    }
}