using CliHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace CliHelper;

public sealed class Client
{
    private readonly List<Registration> _registrations;

    private bool _hadAddedControllers;
    private readonly InputConfiguration _inputConfiguration;

    private readonly IServiceCollection _serviceCollection;
    private IServiceProvider _serviceProvider;

    private Client()
    {
        _registrations = new List<Registration>();
        _inputConfiguration = new InputConfiguration()
        {
            RequireControllerName = false,
            RequireActionName = false
        };
        _serviceCollection = new ServiceCollection();
    }

    private void RegisterType(Type type)
    {
        if (!type.IsSubclassOf(typeof(Controller)))
            throw new NotImplementedException($"{type.Name} must inherit from base class {typeof(Controller)}");

        var typeAttribute = type.GetCustomAttribute<CliAttribute>();

        foreach (var method in type.GetMethods().Where(m => m.IsPublic && m.DeclaringType == type && !m.IsSpecialName))
        {
            var methodAttribute = method.GetCustomAttribute<CliAttribute>();

            var registration = new Registration
            {
                Type = type,
                TypeAttribute = typeAttribute,
                Method = method,
                MethodAttribute = methodAttribute
            };

            _registrations.Add(registration);
            _serviceCollection.AddTransient(type);
        }
    }

    public Client AddControllers(Assembly assembly = null)
    {
        if (_hadAddedControllers)
            throw new ApplicationException($"Controller have already been added.");

        if (assembly is null)
            assembly = Assembly.GetCallingAssembly();

        foreach (var type in assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(Controller))))
        {
            RegisterType(type);
        }

        _hadAddedControllers = true;
        return this;
    }

    public Client AddControllers(params Type[] types)
    {
        if (_hadAddedControllers)
            throw new ApplicationException($"Controller have already been added.");

        foreach (var type in types)
        {
            RegisterType(type);
        }

        _hadAddedControllers = true;
        return this;
    }

    public Client ConfigureInput(Action<InputConfiguration> configure)
    {
        configure(_inputConfiguration);
        return this;
    }

    public Client AddServices(Action<IServiceCollection> configureServices)
    {
        configureServices(_serviceCollection);
        return this;
    }

    public Client Run(string[] args)
    {
        var argsAsString = string.Join(' ', args);
        CoreRun<object>(argsAsString);
        return this;
    }

    private T CoreRun<T>(string args)
    {
        // TODO: Should we need a null check here?
        if (_serviceProvider is null)
            _serviceProvider = _serviceCollection.BuildServiceProvider();

        var registration = ExtractRegistration(ref args);
        var instance = _serviceProvider.GetRequiredService(registration.Type);

        var parameters = ExtractMethodParameters(registration.Method, ref args);
        var output = registration.Method.Invoke(instance, parameters);

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

    internal Registration ExtractRegistration(ref string args)
    {
        var registeredTypes = _registrations.Select(r => r.TypeAttribute?.Alias ?? r.Type.Name).OrderByDescending(r => r.Length).ToList();
        var registeredMethods = _registrations.Select(r => r.MethodAttribute?.Alias ?? r.Method.Name).OrderByDescending(r => r.Length).ToList();

        // TODO: Regex should handle requirements
        // Regex: ^(?<Controller>controller)? *(?<Action>action)? *
        var regex = new Regex($"^(?<Controller>{string.Join('|', registeredTypes)})? *(?<Action>{string.Join('|', registeredMethods)})? *", RegexOptions.IgnoreCase);
        var match = regex.Match(args);

        if (match.Success)
        {
            args = regex.Replace(args, m => string.Empty);

            var controller = match.Groups["Controller"].Value;
            var action = match.Groups["Action"].Value;

            var filtered = _registrations.ToList(); // Effectively make a copy of the registrations list

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

    /// <summary>
    /// If <paramref name="targetType"/> has a special implementation defined in this library, it will return an instance. Otherwise, it will return null.
    /// </summary>
    internal object ExtractSpecialInstance(Type targetType)
    {
        if (targetType == typeof(TextReader))
            return Console.In;

        else
            return null;
    }

    /// <summary>
    /// If the service collection contains an item of type <paramref name="targetType"/>, it will return the instance. Otherwise, it will return an instance using the default constructor.
    /// </summary>
    internal object ExtractInstance(Type targetType, ref string args)
    {
        if (_serviceProvider is null)
            _serviceProvider = _serviceCollection.BuildServiceProvider();

        var instance = _serviceProvider.GetService(targetType) ?? Activator.CreateInstance(targetType);
        foreach (var property in targetType.GetProperties())
        {
            var attribute = property.GetCustomAttribute<CliAttribute>();

            var value = ExtractArgument(attribute?.Alias ?? property.Name, property.PropertyType, ref args);
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
    /// <param name="method"></param>
    /// <param name="args"></param>
    /// <returns></returns>
    internal object[] ExtractMethodParameters(MethodInfo method, ref string args)
    {
        var parameters = method.GetParameters();
        var result = new object[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var attribute = parameter.GetCustomAttribute<CliAttribute>();

            var value = ExtractArgument(attribute?.Alias ?? parameter.Name, parameter.ParameterType, ref args);
            if (value is null)
                value = ExtractSpecialInstance(parameter.ParameterType);

            if (value is null)
                value = ExtractInstance(parameter.ParameterType, ref args);

            result[i] = value;
        }

        return result;
    }

    /// <summary>
    /// Uses regex to parse through <paramref name="args"/> for key/value pair <paramref name="targetName"/> and converts the result to <paramref name="targetType"/>
    /// </summary>
    internal object ExtractArgument(string targetName, Type targetType, ref string args)
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
                    return MasterConvert(targetType, group.Value);
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
                var converted = MasterConvert(targetType, stringValue);
                return converted;
            }

            return null;
        }
    }

    public static Client Create()
    {
        var client = new Client();
        return client;
    }

    private static readonly string[] _trueStringValues = new string[] { "true", "yes", "y", "1" };
    private static readonly string[] _falseStringValues = new string[] { "false", "no", "n", "0" };

    /// <summary>
    /// Converts <paramref name="stringValue"/> to <paramref name="targetType"/>.
    /// </summary>
    /// <exception cref="InvalidCastException"></exception>
    private static object MasterConvert(Type targetType, string stringValue)
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