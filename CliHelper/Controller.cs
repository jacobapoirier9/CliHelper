using CliHelper.Services;
using System.Text.RegularExpressions;

namespace CliHelper;

public abstract class Controller
{
    public CommandContext SelectedCommandContext { get; internal set; }

    public ISettings Settings { get; internal set; }
}
