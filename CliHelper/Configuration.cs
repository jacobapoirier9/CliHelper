namespace CliHelper;

public sealed class Configuration : IConfiguration
{
    public bool RequireControllerName { get; set; }

    public bool RequireActionName { get; set; }

    public bool DisableInteractiveShell { get; set; }

    public string InteractiveShellPrompt { get; set; }

    public string InteractiveShellBanner { get; set; }

}
