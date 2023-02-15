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

        var parameters = BindArguments(registration.Method, ref args);
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

    internal object BindArguments(Type type, ref string args)
    {
        if (_serviceProvider is null)
            _serviceProvider = _serviceCollection.BuildServiceProvider();

        var instance = _serviceProvider.GetService(type) ?? Activator.CreateInstance(type);

        foreach (var property in type.GetProperties())
        {
            var attribute = property.GetCustomAttribute<CliAttribute>();
            var value = ExtractTargetArgument(attribute?.Alias ?? property.Name, property.PropertyType, ref args);
            if (value is not null)
                property.SetValue(instance, value);
        }

        return instance;
    }

    internal object[] BindArguments(MethodInfo method, ref string args)
    {
        var parameters = method.GetParameters();
        var result = new object[parameters.Length];
        for (var i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];
            var attribute = parameter.GetCustomAttribute<CliAttribute>();
            var value = default(object);

            // If we have explicitly defined how to parse and assign a typed parameter, it must be listed here.
            if (parameter.ParameterType.In(
                typeof(bool), typeof(bool?), typeof(string), typeof(TimeSpan), typeof(TimeSpan?), typeof(DateTime), typeof(DateTime?),
                typeof(byte), typeof(byte?), typeof(short), typeof(short?), typeof(int), typeof(int?), typeof(long), typeof(long?),
                typeof(double), typeof(double?), typeof(float), typeof(float?), typeof(decimal), typeof(decimal?)
            ))
            {
                value = ExtractTargetArgument(attribute?.Alias ?? parameter.Name, parameter.ParameterType, ref args);
            }
            // Otherwise, this will assume it is a user defined strongly typed request DTO
            else
            {
                value = BindArguments(parameter.ParameterType, ref args);
            }

            if (value is not null)
                result[i] = value;
        }

        return result;
    }

    // TODO: Parse Anonymous Parameters?
    // Boolean Regex:       (?<Prefix>--|\/)(?<ArgumentName>[\w-]*)(?<ArgumentNameTerminator>[\s:=]+(?<ArgumentValue>false|true|yes|no|y|n)?|$)
    // Named Regex:         (?<Prefix>--|\/)(?<ArgumentName>[\w-]*)(?<ArgumentNameTerminator>[\s:=]+)(?<ArgumentValue>[\w:\\.-]+|"[\w\s:\\.-]*"|'[\w\s:\\.-]*')
    // Anonymous Regex:     (?<AnonymousArgument>[\w:\\.-]+|"[\w\s:\\.-]*"|'[\w\s:\\.-]*')
    internal object ExtractTargetArgument(string targetName, Type targetType, ref string args)
    {
        // TODO: How should boolean values be parsed?
        // Option 1 is to use switch presence as an indicator to set to true.
        // Option 2 is to use values such as Y/N to set to true/false accordingly.
        if (targetType == typeof(bool) || targetType == typeof(bool?))
        {
            // Allowable true/false values need to be configured during client building.
            var regex = new Regex($@"(?<Prefix>--|\/)(?<ArgumentName>{targetName})(?<ArgumentNameTerminator>[\s:=]+(?<ArgumentValue>false|true|yes|no|y|n)?|$)", RegexOptions.IgnoreCase);
            var match = regex.Match(args);

            if (match.Success)
            {
                args = regex.Replace(args, m => string.Empty);

                var group = match.Groups["ArgumentValue"];
                if (group.Success)
                {
                    var value = group.Value.ToLower();
                    if (value.In("true", "yes", "y"))
                        return true;
                    else if (value.In("false", "no", "n")) // TODO: This could probably be an else, since the regex should not return a successful match if the option is not listed.
                        return false;
                    else
                        return null;
                }
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
                var converted = default(object);

                if (targetType == typeof(string))
                    converted = stringValue;



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

            return null;
        }
    }

    public static Client Create()
    {
        var client = new Client();
        return client;
    }
}