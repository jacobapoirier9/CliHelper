using CliHelper;

internal class ControllerContext
{
    public Type ControllerType { get; set; }

    public CliAttribute ControllerAttribute { get; set; }

    public List<ActionContext> Actions { get; set; }
}
