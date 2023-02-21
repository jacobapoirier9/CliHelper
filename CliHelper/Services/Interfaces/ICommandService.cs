namespace CliHelper.Services.Interfaces;

public interface ICommandService
{
    /// <summary>
    /// Enter an interactive shell to enterer repeated commands on the command line.
    /// </summary>
    public void RunInteractiveShell();

    /// <summary>
    /// Execute a single command from the command line.
    /// </summary>
    public T RunCommand<T>(string args);

    /// <summary>
    /// Execute a single command from the command line.
    /// </summary>
    public object RunCommand(string args) => RunCommand<object>(args);
}
