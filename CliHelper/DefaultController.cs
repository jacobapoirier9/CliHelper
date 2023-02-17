namespace CliHelper;

internal sealed class DefaultController : Controller
{
    private readonly List<CommandContext> _commandContexts;
    public DefaultController(List<CommandContext> commandContexts)
    {
        _commandContexts = commandContexts;
    }

    [Cli("help")]
    public void Help()
    {
        var groups = _commandContexts
            .Where(r => r.Type != typeof(DefaultController))
            .GroupBy(r => r.Type, r => r)
            .OrderBy(g => g.Key.Name);

        foreach (var group in groups)
        {
            var firstCommandContext = _commandContexts.First(r => r.Type == group.Key);
            Console.WriteLine(firstCommandContext?.TypeAttribute?.Alias ?? firstCommandContext.Type.Name);

            foreach (var commandContext in group.OrderBy(r => r.Method.Name))
            {
                Console.Write("  ");
                Console.WriteLine(commandContext?.MethodAttribute?.Alias ?? commandContext.Method.Name);
            }

            Console.WriteLine();
        }
    }
}