
using CliHelper.Tests.Controllers;
using CliHelper.Tests.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CliHelper.Tests;

public class CliClientTests
{
    #region Controller/Action Execution Tests
    [Fact]
    public void ExecuteAction_NotAliased_NoParameters_MultipleControllers()
    {
        var args = new string[] { "basicnoalias", "indextwo" };

        var client = CliClient.Create()
            .AddControllers(typeof(BasicNoAliasController));

        var response = client.Run<string>(args);
        Assert.Equal(nameof(BasicNoAliasController.IndexTwo), response);
    }

    [Fact]
    public void ExecuteAction_NotAliased_NoParameters_PrimaryController()
    {
        var args = new string[] { "indextwo" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(BasicNoAliasController));

        var response = client.Run<string>(args);
        Assert.Equal(nameof(BasicNoAliasController.IndexTwo), response);
    }

    [Fact]
    public void ExecuteAction_Aliased_NoParameters_MultipleControllers()
    {
        var args = new string[] { "basic", "index-one" };

        var client = CliClient.Create()
            .AddControllers(typeof(BasicAliasController));

        var response = client.Run<string>(args);
        Assert.Equal(nameof(BasicAliasController.IndexOne), response);
    }

    [Fact]
    public void ExecuteAction_Aliased_NoParameters_PrimaryController()
    {
        var args = new string[] { "index-one" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(BasicAliasController));

        var response = client.Run<string>(args);
        Assert.Equal(nameof(BasicAliasController.IndexOne), response);
    }

    [Fact]
    public void ExecuteAction_DependencyInjection_Constructor()
    {
        var args = new string[] { "dependency-injection", "constructor" };

        var client = CliClient.Create()
            .AddControllers(typeof(DependencyInjectionController))
            .AddServices(services =>
            {
                services.AddTransient<ITestService, TestService>();
            });

        var response = client.Run<string>(args);
        Assert.Equal(TestService.Response, response);
    }

    //[Fact]
    //public void ExecuteAction_DependencyInjection_Method()
    //{
    //    var args = new string[] { "dependency-injection", "parameter" };

    //    var client = CliClient.Create()
    //        .AddControllers(typeof(DependencyInjectionController))
    //        .AddServices(services =>
    //        {
    //            services.AddTransient<ITestService, TestService>();
    //        });

    //    var response = client.Run<string>(args);
    //    Assert.Equal(TestService.Response, response);
    //}

    [Fact]
    public void ExecuteAction_SimpleParameters_Method_Int()
    {
        var args = new string[] { "as-int", "--number", "4" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(SimpleParametersController))
            .AddServices(services =>
            {
            });

        var response = client.Run<string>(args);
        Assert.Equal("4", response);
    }

    [Fact]
    public void ExecuteAction_ComplexeParameter_MapCorrectly()
    {
        var args = new string[] { "index", "-age", "21", "--name", "Jake" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(ComplexParameterController));

        var response = client.Run<string>(args);
        Assert.Equal("Jake-21", response);
    }

    //[Cli("map-multiple")]
    //public string MultipleParameters(string name, int? number, bool repeat)
    //{
    //    if (number.HasValue)
    //    {
    //        if (!repeat)
    //            number = 1;

    //        var list = new List<string>();
    //        for (var i = 0; i < number; i++)
    //            list.Add(name);

    //        return number.ToString() + '-' + string.Join('-', list);
    //    }

    //    return name;
    //}
    [Fact]
    public void ExecutionAction_SimpleNamedParameters_NoAlias_MapCorrectly_1()
    {
        var args = new string[] { "map-multiple", "--repeat", "--number", "2", "--name", "Jake" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(SimpleParametersController));

        var response = client.Run<string>(args);
        Assert.Equal("2-Jake-Jake", response);
    }

    [Fact]
    public void ExecutionAction_SimpleNamedParameters_NoAlias_MapCorrectly_2()
    {
        var args = new string[] { "map-multiple", "--number", "2", "--name", "Jake" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(SimpleParametersController));

        var response = client.Run<string>(args);
        Assert.Equal("1-Jake", response);
    }

    [Fact]
    public void ExecutionAction_SimpleNamedParameters_NoAlias_MapCorrectly_3()
    {
        var args = new string[] { "map-multiple", "--name", "Jake" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(SimpleParametersController));

        var response = client.Run<string>(args);
        Assert.Equal("Jake", response);
    }

    [Fact]
    public void ExecutionAction_SimpleNamedParameters_Alias_MapCorrectly_1()
    {
        var args = new string[] { "map-multiple-alias", "--repeat", "--number", "2", "--name", "Jake" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(SimpleParametersController));

        var response = client.Run<string>(args);
        Assert.Equal("2-Jake-Jake", response);
    }

    [Fact]
    public void ExecutionAction_SimpleNamedParameters_Alias_MapCorrectly_2()
    {
        var args = new string[] { "map-multiple-alias", "--number", "2", "--name", "Jake" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(SimpleParametersController));

        var response = client.Run<string>(args);
        Assert.Equal("1-Jake", response);
    }

    [Fact]
    public void ExecutionAction_SimpleNamedParameters_Alias_MapCorrectly_3()
    {
        var args = new string[] { "map-multiple-alias", "--name", "Jake" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(SimpleParametersController));

        var response = client.Run<string>(args);
        Assert.Equal("Jake", response);
    }


    [Fact]
    public void ExecuteAction_SimpleUnnamedParameters_NoAlias_MapCorrectly_1()
    {
        var args = new string[] { "map-multiple-alias", "Jake", "3", "true" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(SimpleParametersController));

        var response = client.Run<string>(args);
        Assert.Equal("3-Jake-Jake-Jake", response);
    }

    [Fact]
    public void ExecuteAction_SimpleUnnamedParameters_NoAlias_MapCorrectly_2()
    {
        var args = new string[] { "map-multiple-alias", "Jake", "2" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(SimpleParametersController));

        var response = client.Run<string>(args);
        Assert.Equal("1-Jake", response);
    }

    [Fact]
    public void ExecuteAction_SimpleUnnamedParameters_NoAlias_MapCorrectly_3()
    {
        var args = new string[] { "map-multiple-alias", "--name", "Jake" };

        var client = CliClient.Create()
            .AddPrimaryController(typeof(SimpleParametersController));

        var response = client.Run<string>(args);
        Assert.Equal("Jake", response);
    }
    #endregion

    #region Controller Registration Tests
    [Fact]
    public void CreateCliClient_MustImplementCliController()
    {
        Assert.Throws<ControllerException>(() =>
        {
            var client = CliClient.Create()
                .AddPrimaryController(typeof(BasicNotImplementedController));
        });
    }

    [Fact]
    public void CreateCliClient_CannotAddControllersIfPrimaryControllerHasAlreadyBeenAdded()
    {
        Assert.Throws<ControllerException>(() =>
        {
            var client = CliClient.Create()
                .AddControllers(typeof(CliClientTests).Assembly)
                .AddPrimaryController(typeof(BasicAliasController));
        });
    }

    [Fact]
    public void CreateCliClient_CannotAddPrimaryControllerIfControllersHaveAleadyBeenAdded()
    {
        Assert.Throws<ControllerException>(() =>
        {
            var client = CliClient.Create()
                .AddPrimaryController(typeof(BasicAliasController))
                .AddControllers(typeof(CliClientTests).Assembly);
        });
    }

    [Fact]
    public void CreateCliClient_ControllerMustHaveAtLeastOneActionMethod()
    {
        Assert.Throws<ControllerException>(() =>
        {
            var client = CliClient.Create()
                .AddPrimaryController(typeof(DuplicateActionController));
        });
    }

    [Fact]
    public void CreateCliClient_ControllerActionMustBeAllSimpleTypesOrOneComplexType()
    {
        Assert.Throws<ControllerException>(() =>
        {
            var client = CliClient.Create()
                .AddPrimaryController(typeof(SimpleAndComplexController));
        });
    }
    #endregion
}