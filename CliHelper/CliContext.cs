internal class CliArguments
{
    public string CliController { get; set; }

    public string CliAction { get; set; }

    public string[] RemainingArgs { get; set; }
}