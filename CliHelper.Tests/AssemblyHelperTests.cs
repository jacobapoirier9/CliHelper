using CliHelper;
using NuGet.Frameworks;
using System.Reflection;

namespace CliHelper.Tests;

public class AssemblyHelpersTests
{
    [Fact]
    public void FindCliActions_DefaultsToCurrentAssembly()
    {
        var thisAssembly = Assembly.GetExecutingAssembly();

        var actions = AssemblyHelper.FindCliActions(typeof(AssemblyHelpersTests).Assembly);

        Assert.All(actions, action => Assert.Equal(thisAssembly, action.ControllerType.Assembly));
    }

    [Fact]
    public void FindCliActions_ReturnsCorrectNumberOfRecords()
    {
        var actions = AssemblyHelper.FindCliActions(typeof(AssemblyHelpersTests).Assembly);
        Assert.Equal(6, actions.Count);
    }
}
