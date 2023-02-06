using CliHelper;

internal class ControllerContext
{
    public Type Type { get; set; }

    public CliAttribute CliAttribute { get; set; }

    public List<ActionContext> Actions { get; set; }
}
