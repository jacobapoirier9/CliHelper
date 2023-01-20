using CliHelper;
using System.Reflection;

internal class ActionContext
{
    public MethodInfo ActionMethod { get; set; }

    public CliAttribute ActionAttribute { get; set; }


    public List<ParameterContext> Parameters { get; set; }
}
