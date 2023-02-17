using CliHelper;
using System.Reflection;

public sealed class CommandContext
{
    public Type Type { get; set; }

    public CliAttribute TypeAttribute { get; set; }

    public MethodInfo Method { get; set; }

    public CliAttribute MethodAttribute { get; set; }
}
