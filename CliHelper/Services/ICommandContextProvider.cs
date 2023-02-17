namespace CliHelper.Services;

internal interface ICommandContextProvider
{
    public List<CommandContext> CommandContexts { get; }

    public void RegisterCommandContexts(Type type);

    public CommandContext ExtractCommandContext(ref string args, Configuration configuration = null);
}
