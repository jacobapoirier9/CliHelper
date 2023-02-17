using System.Reflection;
using System.Text.RegularExpressions;

namespace CliHelper.Services;

internal class CommandContextProvider : ICommandContextProvider
{
    private readonly List<CommandContext> _master = new List<CommandContext>();

    public List<CommandContext> CommandContexts => _master;

    public void RegisterCommandContexts(Type type)
    {
        var typeAttribute = type.GetCustomAttribute<CliAttribute>();
        foreach (var method in type.GetMethods().Where(m => m.IsPublic && m.DeclaringType == type && !m.IsSpecialName))
        {
            var methodAttribute = method.GetCustomAttribute<CliAttribute>();

            var commandContext = new CommandContext
            {
                Type = type,
                TypeAttribute = typeAttribute,
                Method = method,
                MethodAttribute = methodAttribute
            };

            _master.Add(commandContext);
        }
    }

    public CommandContext ExtractCommandContext(ref string args, Configuration configuration = null)
    {
        var registeredTypes = _master.Select(r => r.TypeAttribute?.Alias ?? r.Type.Name).OrderByDescending(r => r.Length).ToList();
        var registeredMethods = _master.Select(r => r.MethodAttribute?.Alias ?? r.Method.Name).OrderByDescending(r => r.Length).ToList();

        // TODO: Regex should handle requirements
        // Regex: ^(?<Controller>controller)? *(?<Action>action)? *
        var regex = new Regex($"^(?<Controller>{string.Join('|', registeredTypes)})? *(?<Action>{string.Join('|', registeredMethods)})? *", RegexOptions.IgnoreCase);
        var match = regex.Match(args);

        if (match.Success)
        {
            args = regex.Replace(args, m => string.Empty);

            var controller = match.Groups["Controller"].Value;
            if (configuration.RequireControllerName && string.IsNullOrEmpty(controller))
                throw new ApplicationException("Must provide a valid controller name");

            var action = match.Groups["Action"].Value;
            if (configuration.RequireActionName && string.IsNullOrEmpty(action))
                throw new ApplicationException("Must provide a valid action name");

            var filtered = _master.ToList(); // Effectively make a copy of the command contexts list

            if (!string.IsNullOrEmpty(controller))
                filtered = filtered.Where(r => string.Equals(r.TypeAttribute?.Alias ?? r.Type.Name, controller, StringComparison.OrdinalIgnoreCase)).ToList();
            if (!string.IsNullOrEmpty(action))
                filtered = filtered.Where(r => string.Equals(r.MethodAttribute?.Alias ?? r.Method.Name, action, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!filtered.Any())
                throw new ApplicationException("Could not find any actions");
            else if (filtered.Count == 1)
                return filtered.First();
            else
                throw new ApplicationException("Could not find a single action");
        }
        else
            throw new ApplicationException("Could not find any action");
    }
}
