namespace CliHelper;

public interface ICommandService
{
    /// <summary>
    /// Calls <see cref="HandleInteractiveShell"/> or <see cref"HandleNonInteractiveShell{T}(string)"/> depending on the value of <paramref name="args"/>.
    /// </summary>
    public void HandleInputString(string args);

    /// <summary>
    /// Enter an interactive shell to enterer repeated commands on the command line.
    /// </summary>
    public void HandleInteractiveShell();

    /// <summary>
    /// Execute a single command from the command line.
    /// </summary>
    public T HandleNonInteractiveShell<T>(string args);
}
