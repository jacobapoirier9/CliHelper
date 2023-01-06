using CliHelper;
using System.Reflection;

internal class CliExecutionContext
{
    public Type ControllerType { get; set; }
    public CliAttribute ControllerAttribute { get; set; }

    public MethodInfo ActionMethod { get; set; }
    public CliAttribute ActionAttribute { get; set; }
}
