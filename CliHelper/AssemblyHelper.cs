using CliHelper;
using System.Reflection;

internal static class AssemblyHelper
{
    internal static List<CliExecutionContext> FindCliActions(Assembly assembly)
    {
        var cliControllerActionWrappers = new List<CliExecutionContext>();

        var controllerTypes = assembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(CliController)))
            .ToList();

        foreach (var controllerType in controllerTypes)
        {
            cliControllerActionWrappers.AddRange(FindCliActions(controllerType));
        }

        return cliControllerActionWrappers;
    }

    internal static List<CliExecutionContext> FindCliActions(Type controllerType)
    {
        var cliControllerActionWrappers = new List<CliExecutionContext>();

        var controllerAttribute = controllerType.GetCustomAttribute<CliAttribute>();

        var actionMethods = controllerType.GetMethods()
            .Where(m => m.IsPublic && m.DeclaringType == controllerType)
            .ToList();

        foreach (var actionMethod in actionMethods)
        {
            var actionAttribute = actionMethod.GetCustomAttribute<CliAttribute>();

            cliControllerActionWrappers.Add(new CliExecutionContext
            {
                ControllerType = controllerType,
                ControllerAttribute = controllerAttribute,
                ActionMethod = actionMethod,
                ActionAttribute = actionAttribute
            });
        }

        return cliControllerActionWrappers;
    }
}
