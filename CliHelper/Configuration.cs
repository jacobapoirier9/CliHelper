namespace CliHelper;

public sealed class Configuration
{
    /// <summary>
    /// Throw an exception if a registered action name was not specified in the shell.
    /// </summary>
    public bool RequireControllerName { get; set; }

    /// <summary>
    /// Throw an exception if a registered controller name was not specified in the shell.
    /// </summary>
    public bool RequireActionName { get; set; }

    /// <summary>
    /// By default, if an empty args[] is passed to the application, it will enter an interactive shell. This option allows you to throw an exception instead.
    /// </summary>
    public bool DisableInteractiveShell { get; set; }

    /// <summary>
    /// Adjust the prompt shown for each command line. <see cref="DisableInteractiveShell"/> must be set to false in order for this to work.
    /// </summary>
    public string InteractiveShellPrompt { get; set; }

    /// <summary>
    /// Adjust the banner shown at the start of a command line session.
    /// </summary>
    public string InteractiveShellBanner { get; set; }
}
