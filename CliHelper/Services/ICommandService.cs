namespace CliHelper;

public interface ICommandService
{
    public void HandleInputString(string args);

    public void HandleInteractiveShell();

    public T HandleNonInteractiveShell<T>(string args);
}
