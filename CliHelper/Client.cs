using CliHelper.Services;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.Design;
using System.Reflection;
using System.Text.RegularExpressions;

namespace CliHelper;

public sealed class Client
{
    private readonly Configuration _configuration;

    private readonly IServiceCollection _serviceCollection;
    private IServiceProvider _serviceProvider;

    // TEMP
    // Could this be added as an item registration?
    private readonly ICommandContextProvider _commandContextProvider = new CommandContextProvider();

    // ENDTEMP

    #region Client Building
    private Client()
    {
        _configuration = new Configuration()
        {
            RequireControllerName = false,
            RequireActionName = false,
            DisableInteractiveShell = false,
            InteractiveShellPrompt = " > "
        };
        _serviceCollection = new ServiceCollection();
    }

    public static Client Create()
    {
        var client = new Client();
        return client;
    }
    #endregion

    #region Adding Command Controllers/Modules
    /// <summary>
    /// Searches <paramref name="assembly"/> for all types that inherit <see cref="Controller"/> and adds to the command collection.
    /// </summary>
    /// <param name="assembly">Target assembly to search</param>
    /// <returns></returns>
    public Client AddControllers(Assembly assembly = null)
    {
        if (assembly is null)
            assembly = Assembly.GetCallingAssembly();

        var controllerTypes = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Controller))).ToArray();
        AddControllers(controllerTypes);

        return this;
    }

    /// <summary>
    /// Adds all types in <paramref name="types"/> to the command collection.
    /// </summary>
    /// <param name="types"></param>
    public Client AddControllers(params Type[] types)
    {
        foreach (var type in types)
        {
            _serviceCollection.AddTransient(type);
            _commandContextProvider.RegisterCommandContexts(type);
        }

        return this;
    }
    #endregion

    public Client Configure(Action<Configuration> configure)
    {
        configure(_configuration);
        return this;
    }

    public Client AddServices(Action<IServiceCollection> configureServices)
    {
        configureServices(_serviceCollection);
        return this;
    }

    public Client Run(string[] args)
    {
        AddControllers(typeof(DefaultController));
        _serviceCollection.AddSingleton(_commandContextProvider.CommandContexts);
        _serviceProvider = _serviceCollection.BuildServiceProvider();

        if (args.Any())
            HandleCommandExecution<object>(args);
        else
            HandleCommandShell();

        return this;
    }

    private void HandleCommandShell()
    {
        if (_configuration.InteractiveShellBanner is not null)
            Console.WriteLine(_configuration.InteractiveShellBanner);

        if (_configuration.DisableInteractiveShell)
            throw new ApplicationException("Interactive shell has been disabled. No arguments were passed to the application.");

        do
        {
            try
            {
                Console.Write(_configuration.InteractiveShellPrompt);

                var args = Console.ReadLine();
                HandleCommandExecution<object>(args);
            }
            catch(Exception ex) // TODO: allows clients to handle the exception thrown here?
            {
                Console.WriteLine("Invalid command");
            }
        } while (true);
    }

    private T HandleCommandExecution<T>(string[] args) => HandleCommandExecution<T>(string.Join(' ', args));
    private T HandleCommandExecution<T>(string args)
    {
        var commandContext = _commandContextProvider.ExtractCommandContext(ref args, _configuration);
        var instance = _serviceProvider.GetRequiredService(commandContext.Type);

        if (commandContext.Type.IsSubclassOf(typeof(Controller)))
        {
            var selectedCommandContextProperty = typeof(Controller).GetProperty(nameof(Controller.SelectedCommandContext));
            selectedCommandContextProperty.SetValue(instance, commandContext);
        }

        var parameters = ExtractMethodParameters(commandContext.Method, ref args);
        var output = commandContext.Method.Invoke(instance, parameters);

        if (output is Task emptyTask)
        {
            emptyTask.Wait();
            return default(T);
        }
        else if (output is Task<T> typedTask)
        {
            typedTask.Wait();
            return typedTask.Result;
        }
        else
            return (T)output;
    }

    /// <summary>
    /// If <paramref name="targetType"/> has a special implementation defined in this library, it will return an instance. Otherwise, it will return null.
    /// </summary>
    private object ExtractSpecialInstance(Type targetType)
    {
        if (targetType == typeof(TextReader))
            return Console.In;

        else
            return null;
    }

    /// <summary>
    /// If the service collection contains an item of type <paramref name="targetType"/>, it will return the instance. Otherwise, it will return an instance using the default constructor.
    /// </summary>
    private object ExtractStronglyTypedInstance(Type targetType, ref string args)
    {
        var instance = _serviceProvider.GetService(targetType) ?? Activator.CreateInstance(targetType);
        foreach (var property in targetType.GetProperties())
        {
            var attribute = property.GetCustomAttribute<CliAttribute>();

            var value = ExtractSimpleTypeInstance(attribute?.Alias ?? property.Name, property.PropertyType, ref args);
            if (value is null)
                value = ExtractSpecialInstance(property.PropertyType);

            if (value is not null)
                property.SetValue(instance, value);
        }

        return instance;
    }

    /// <summary>
    /// Returns an array of parameters that should be passed to the <see cref="MethodInfo"/>, which is determined in a previous step.
    /// </summary>
    private object[] ExtractMethodParameters(MethodInfo method, ref string args)
    {
        var parameters = method.GetParameters();
        var result = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var attribute = parameter.GetCustomAttribute<CliAttribute>();

            var value = ExtractSimpleTypeInstance(attribute?.Alias ?? parameter.Name, parameter.ParameterType, ref args);
            if (value is null)
                value = ExtractSpecialInstance(parameter.ParameterType);

            if (value is null)
                value = ExtractStronglyTypedInstance(parameter.ParameterType, ref args);

            result[i] = value;
        }

        return result;
    }

    /// <summary>
    /// Uses regex to parse through <paramref name="args"/> for key/value pair <paramref name="targetName"/> and converts the result to <paramref name="targetType"/>
    /// </summary>
    private object ExtractSimpleTypeInstance(string targetName, Type targetType, ref string args)
    {
        // TODO: Parse Anonymous Parameters?
        // Boolean Regex:       (?<Prefix>--|\/)(?<ArgumentName>[\w-]*)(?<ArgumentNameTerminator>[\s:=]+(?<ArgumentValue>false|true|yes|no|y|n)?|$)
        // Named Regex:         (?<Prefix>--|\/)(?<ArgumentName>[\w-]*)(?<ArgumentNameTerminator>[\s:=]+)(?<ArgumentValue>[\w:\\.-]+|"[\w\s:\\.-]*"|'[\w\s:\\.-]*')
        // Anonymous Regex:     (?<AnonymousArgument>[\w:\\.-]+|"[\w\s:\\.-]*"|'[\w\s:\\.-]*')

        // TODO: How should boolean values be parsed?
        // Option 1 is to use switch presence as an indicator to set to true.
        // Option 2 is to use values such as Y/N to set to true/false accordingly.
        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            var booleanValues = _trueStringValues.Concat(_falseStringValues).OrderByDescending(s => s.Length).ToList();
            var regex = new Regex($@"(?<Prefix>--|\/)(?<ArgumentName>{targetName})(?<ArgumentNameTerminator>[\s:=]+(?<ArgumentValue>{string.Join('|', booleanValues)})?|$)", RegexOptions.IgnoreCase);
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
            var regex = new Regex($@"(?<Prefix>--|\/)(?<ArgumentName>{targetName})(?<ArgumentNameTerminator>[\s:=]+)(?<ArgumentValue>{validStringValueRegex}+|""{validStringValueRegex}*""|'{validStringValueRegex}*')", RegexOptions.IgnoreCase);
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


    private static readonly string[] _trueStringValues = new string[] { "true", "yes", "y", "1" };
    private static readonly string[] _falseStringValues = new string[] { "false", "no", "n", "0" };

    /// <summary>
    /// Converts <paramref name="stringValue"/> to <paramref name="targetType"/>.
    /// </summary>
    /// <exception cref="InvalidCastException"></exception>
    private static object MasterConvertSimpleType(Type targetType, string stringValue)
    {
        var converted = default(object);

        if (targetType == typeof(string))
            converted = stringValue;

        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            var lower = stringValue.ToLower();

            if (_trueStringValues.Contains(lower))
                converted = true;
            else if (_falseStringValues.Contains(lower))
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
