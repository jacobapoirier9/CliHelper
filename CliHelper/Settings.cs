namespace CliHelper;

public sealed class Settings : ISettings
{
    public bool RequireControllerName { get; set; }

    public bool RequireActionName { get; set; }

    public bool DisableInteractiveShell { get; set; }

    public string InteractiveShellPrompt { get; set; }

    public string InteractiveShellBanner { get; set; }

    public Action<Exception> InteractiveShellHandleErrors { get; set; }

    public string[] ConsiderTrueStrings { get; set; }

    public string[] ConsiderFalseStrings { get; set; }
    public bool InteractiveShellShowHelpOnInvalidCommand { get; set; }

}
