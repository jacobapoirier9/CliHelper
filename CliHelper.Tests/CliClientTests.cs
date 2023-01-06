
using CliHelper.Tests.Controllers;
using CliHelper.Tests.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CliHelper.Tests;

public class CliClientTests
{
    #region Controller/Action Selection
    [Fact]
    public void ExecuteAction_NotAliased_NoParameters_MultipleControllers()
    {
        var args = new string[] { "basicnoalias", "indextwo" };

        var client = CliClient.Create()
            .AddControllers();

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
            .AddControllers();

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
    #endregion

    #region Create CliClient Tests
    [Fact]
    public void CreateCliClient_MustImplementCliController()
    {
        Assert.Throws<NotImplementedException>(() =>
        {
            var client = CliClient.Create()
                .AddPrimaryController(typeof(BasicNotImplementedController));
        });
    }

    [Fact]
    public void CreateCliClient_CannotAddControllersIfPrimaryControllerHasAlreadyBeenAdded()
    {
        Assert.Throws<ControllerAlreadyAddedException>(() =>
        {
            var client = CliClient.Create()
                .AddControllers(typeof(CliClientTests).Assembly)
                .AddPrimaryController(typeof(BasicAliasController));
        });
    }

    [Fact]
    public void CreateCliClient_CannotAddPrimaryControllerIfControllersHaveAleadyBeenAdded()
    {
        Assert.Throws<ControllerAlreadyAddedException>(() =>
        {
            var client = CliClient.Create()
                .AddPrimaryController(typeof(BasicAliasController))
                .AddControllers(typeof(CliClientTests).Assembly);
        });
    }

    [Fact]
    public void CreateCliClient_InjectsToConstructor()
    {
        var args = new string[] { "dependency-injection", "constructor" };

        var client = CliClient.Create()
            .AddControllers()
            .AddServices(services =>
            {
                services.AddTransient<ITestService, TestService>();
            });

        var response = client.Run<string>(args);
        Assert.Equal(TestService.Response, response);
    }

    [Fact]
    public void CreateCliClient_InjectsToParameter()
    {
        var args = new string[] { "dependency-injection", "parameter" };

        var client = CliClient.Create()
            .AddControllers()
            .AddServices(services =>
            {
                services.AddTransient<ITestService, TestService>();
            });

        var response = client.Run<string>(args);
        Assert.Equal(TestService.Response, response);
    }
    #endregion
}