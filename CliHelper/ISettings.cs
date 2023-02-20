namespace CliHelper;

public interface ISettings
{
    /// <summary>
    /// Throw an exception if a registered action name was not specified in the shell.
    /// Default is false.
    /// </summary>
    public bool RequireControllerName { get; set; }

    /// <summary>
    /// Throw an exception if a registered controller name was not specified in the shell.
    /// Default is false.
    /// </summary>
    public bool RequireActionName { get; set; }

    /// <summary>
    /// Throw an exception instead of entering an interactive shell when an empty args[] is passed.
    /// Default is false.
    /// </summary>
    public bool DisableInteractiveShell { get; set; }

    /// <summary>
    /// Adjust the prompt shown for each command line. <see cref="DisableInteractiveShell"/> must be set to false in order for this to work.
    /// Default is " > ".
    /// </summary>
    public string InteractiveShellPrompt { get; set; }

    /// <summary>
    /// Adjust the banner shown at the start of a command line session.
    /// Default is false.
    /// </summary>
    public string InteractiveShellBanner { get; set; }

    /// <summary>
    /// What should happen if an exception is thrown during an interactive shell session.
    /// Default is null.
    /// </summary>
    public Action<Exception> InteractiveShellHandleErrors { get; set; }

    /// <summary>
    /// When a boolean type is being parsed, what strings can be used to represent true.
    /// Default is { true, yes, y, 1 }.
    /// </summary>
    public string[] ConsiderTrueStrings { get; set; }

    /// <summary>
    /// When a boolean type is being parsed, what strings can be used to represent false.
    /// Default is { false, no, n, 0 }.
    /// </summary>
    public string[] ConsiderFalseStrings { get; set; }

    /// <summary>
    /// When an invalid command is entered in an interactive shell, deterime whether or not to automaticaly run the help command.
    /// Default is true.
    /// </summary>
    public bool InteractiveShellShowHelpOnInvalidCommand { get; set; }

    /// <summary>
    /// Strings to use as a prefix to a switch.
    /// Default is { /, -- }.
    /// </summary>
    public string[] CommandSwitchPrefixes { get; set; }
}