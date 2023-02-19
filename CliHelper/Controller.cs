using CliHelper.Services;
using System.Text.RegularExpressions;

namespace CliHelper;

public abstract class Controller
{
    private static readonly string[] _trueStringValues = new string[] { "true", "yes", "y", "1" };
    private static readonly string[] _falseStringValues = new string[] { "false", "no", "n", "0" };

    public CommandContext SelectedCommandContext { get; internal set; }

    public Settings Settings { get; internal set; }

    public bool UserChoice(string prompt)
    {
        var booleanValues = _trueStringValues.Concat(_falseStringValues).OrderByDescending(s => s.Length).ToList();
        var regex = new Regex($@"\s*(?<Response>{string.Join('|', booleanValues)})\s*", RegexOptions.IgnoreCase);


        var match = default(Match);
        var result = default(bool);

        do
        {
            Console.WriteLine(prompt);
            Console.Write(Settings.InteractiveShellPrompt);

            var input = Console.ReadLine();

            match = regex.Match(input);
            if (match.Success)
            {
                var group = match.Groups["Response"];
                result = (bool)ArgumentService.MasterConvertSimpleType(typeof(bool), group.Value);
                break;
            }
        } while (match is null || !match.Success);

        return result;
    }
}
