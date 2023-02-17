namespace CliHelper;

public class Configuration
{
    public bool RequireControllerName { get; set; }

    public bool RequireActionName { get; set; }

    /// <summary>
    /// By default, if an empty args[] is passed to the application, it will enter an interactive shell. This option allows you to throw an exception instead.
    /// </summary>
    public bool DisableInteractiveShell { get; set; }
}
