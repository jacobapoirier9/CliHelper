using CliHelper;
using System.Reflection;

internal class ActionContext
{
    public MethodInfo MethodInfo { get; set; }

    public CliAttribute CliAttribute { get; set; }

    public List<ParameterContext> Parameters { get; set; }
}
