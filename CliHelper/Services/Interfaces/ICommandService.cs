namespace CliHelper.Services.Interfaces;

public interface ICommandService
{
    /// <summary>
    /// Enter an interactive shell to enterer repeated commands on the command line.
    /// </summary>
    public void HandleInteractiveShell();

    /// <summary>
    /// Execute a single command from the command line.
    /// </summary>
    public T HandleNonInteractiveShell<T>(string args);
}
