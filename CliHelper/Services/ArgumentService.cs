using CliHelper.Services.Interfaces;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CliHelper.Services;

public class ArgumentService : IArgumentService
{
    private readonly List<CommandContext> _commandContexts;
    private readonly ISettings _settings;
    private readonly IServiceProvider _serviceProvider;

    public ArgumentService(List<CommandContext> commandContexts, ISettings settings, IServiceProvider serviceProvider)
    {
        _commandContexts = commandContexts;
        _settings = settings;
        _serviceProvider = serviceProvider;
    }

    public CommandContext ExtractCommandContext(ref string args)
    {
        var registeredTypes = _commandContexts.Select(r => r.TypeAttribute?.Alias ?? r.Type.Name).OrderByDescending(r => r.Length).ToList();
        var registeredMethods = _commandContexts.Select(r => r.MethodAttribute?.Alias ?? r.Method.Name).OrderByDescending(r => r.Length).ToList();

        var regex = new Regex($"^(?<Controller>{string.Join('|', registeredTypes)})? *(?<Action>{string.Join('|', registeredMethods)})? *", RegexOptions.IgnoreCase);
        var match = regex.Match(args);

        if (match.Success)
        {
            args = regex.Replace(args, m => string.Empty);

            var controller = match.Groups["Controller"].Value;
            if (_settings.RequireControllerName && string.IsNullOrEmpty(controller))
                throw new ApplicationException("Must provide a valid controller name");

            var action = match.Groups["Action"].Value;
            if (_settings.RequireActionName && string.IsNullOrEmpty(action))
                throw new ApplicationException("Must provide a valid action name");

            var filtered = _commandContexts.ToList();

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

    public object ExtractSimpleTypeInstance(Type targetType, string targetName, ref string args)
    {
        // TODO: Parse Anonymous Parameters?
        // Boolean Regex:       (?<Prefix>--|\/)(?<ArgumentName>[\w-]*)(?<ArgumentNameTerminator>[\s:=]+(?<ArgumentValue>false|true|yes|no|y|n)?|$)
        // Named Regex:         (?<Prefix>--|\/)(?<ArgumentName>[\w-]*)(?<ArgumentNameTerminator>[\s:=]+)(?<ArgumentValue>[\w:\\.-]+|"[\w\s:\\.-]*"|'[\w\s:\\.-]*')
        // Anonymous Regex:     (?<AnonymousArgument>[\w:\\.-]+|"[\w\s:\\.-]*"|'[\w\s:\\.-]*')
        // TODO: How should boolean values be parsed?
        // Option 1 is to use switch presence as an indicator to set to true.
        // Option 2 is to use values such as Y/N to set to true/false accordingly.

        var switchPrefixSubRegexPattern = string.Join('|', _settings.CommandSwitchPrefixes.OrderByDescending(s => s.Length).Select(s => Regex.Escape(s)));

        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            var booleanValues = _settings.ConsiderTrueStrings.Concat(_settings.ConsiderFalseStrings).OrderByDescending(s => s.Length).ToList();
            var regex = new Regex($@"(?<Prefix>{switchPrefixSubRegexPattern})(?<ArgumentName>{targetName})(?<ArgumentNameTerminator>[\s:=]+(?<ArgumentValue>{string.Join('|', booleanValues)})?|$)", RegexOptions.IgnoreCase);
            var match = regex.Match(args);

            // The boolean switch is present.
            if (match.Success)
            {
                args = regex.Replace(args, m => string.Empty);

                var group = match.Groups["ArgumentValue"];

                // The boolean switch is present, and has been provided a value.
                if (group.Success)
                    return MasterConvertSimpleType(targetType, group.Value);
                // The boolean switch is present, and has not been provided a value.
                else
                    return true;
            }
            else
                return null;
        }
        else
        {
            var validStringValueRegex = @"[\w\s:\\.-{}]";
            var regex = new Regex($@"(?<Prefix>{switchPrefixSubRegexPattern})(?<ArgumentName>{targetName})(?<ArgumentNameTerminator>[\s:=]+)(?<ArgumentValue>{validStringValueRegex}+|""{validStringValueRegex}*""|'{validStringValueRegex}*')", RegexOptions.IgnoreCase);
            var match = regex.Match(args);

            if (match.Success)
            {
                args = regex.Replace(args, m => string.Empty);

                var group = match.Groups["ArgumentValue"];
                var stringValue = group.Value.Trim('\'', '"', ' ');
                var converted = MasterConvertSimpleType(targetType, stringValue);
                return converted;
            }

            return null;
        }
    }

    public object ExtractSpecialInstance(Type targetType)
    {
        if (targetType == typeof(TextReader))
            return Console.In;

        else
            return null;
    }

    public object ExtractStronglyTypedInstance(Type targetType, ref string args)
    {
        var instance = _serviceProvider.GetService(targetType) ?? Activator.CreateInstance(targetType);
        foreach (var property in targetType.GetProperties())
        {
            var attribute = property.GetCustomAttribute<CliAttribute>();

            var value = ExtractSimpleTypeInstance(property.PropertyType, attribute?.Alias ?? property.Name, ref args);
            if (value is null)
                value = ExtractSpecialInstance(property.PropertyType);

            if (value is not null)
                property.SetValue(instance, value);
        }

        return instance;
    }

    public object[] ExtractMethodParameters(MethodInfo method, ref string args)
    {
        var parameters = method.GetParameters();
        var result = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var attribute = parameter.GetCustomAttribute<CliAttribute>();

            var value = ExtractSimpleTypeInstance(parameter.ParameterType, attribute?.Alias ?? parameter.Name, ref args);
            if (value is null)
                value = ExtractSpecialInstance(parameter.ParameterType);

            if (value is null)
                value = ExtractStronglyTypedInstance(parameter.ParameterType, ref args);

            result[i] = value;
        }

        return result;
    }
    public object MasterConvertSimpleType(Type targetType, string stringValue)
    {
        var converted = default(object);

        if (targetType == typeof(string))
            converted = stringValue;

        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            var lower = stringValue.ToLower();

            if (_settings.ConsiderTrueStrings.Contains(lower))
                converted = true;
            else if (_settings.ConsiderFalseStrings.Contains(lower))
                converted = false;
            else
                return Activator.CreateInstance(targetType);
        }

        else if (targetType == typeof(byte))
            converted = byte.Parse(stringValue);
        else if (targetType == typeof(byte?))
            converted = byte.TryParse(stringValue, out var outValue) ? outValue : null;

        else if (targetType == typeof(short))
            converted = short.Parse(stringValue);
        else if (targetType == typeof(short?))
            converted = short.TryParse(stringValue, out var outValue) ? outValue : null;

        else if (targetType == typeof(int))
            converted = int.Parse(stringValue);
        else if (targetType == typeof(int?))
            converted = int.TryParse(stringValue, out var outValue) ? outValue : null;

        else if (targetType == typeof(long))
            converted = long.Parse(stringValue);
        else if (targetType == typeof(long?))
            converted = long.TryParse(stringValue, out var outValue) ? outValue : null;

        else if (targetType == typeof(double))
            converted = double.Parse(stringValue);
        else if (targetType == typeof(double?))
            converted = double.TryParse(stringValue, out var outValue) ? outValue : null;

        else if (targetType == typeof(float))
            converted = float.Parse(stringValue);
        else if (targetType == typeof(float?))
            converted = float.TryParse(stringValue, out var outValue) ? outValue : null;

        else if (targetType == typeof(decimal))
            converted = decimal.Parse(stringValue);
        else if (targetType == typeof(decimal?))
            converted = decimal.TryParse(stringValue, out var outValue) ? outValue : null;

        else if (targetType == typeof(TimeSpan))
            converted = TimeSpan.Parse(stringValue);
        else if (targetType == typeof(TimeSpan?))
            converted = TimeSpan.TryParse(stringValue, out var outValue) ? outValue : null;

        else if (targetType == typeof(DateTime))
            converted = DateTime.Parse(stringValue);
        else if (targetType == typeof(DateTime?))
            converted = DateTime.TryParse(stringValue, out var outValue) ? outValue : null;

        return converted;
    }
}