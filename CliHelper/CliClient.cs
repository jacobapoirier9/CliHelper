using CliHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Data.Common;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;

public sealed class CliClient
{

    /// <summary>
    /// The core collection of controller/actions that have been registered.
    /// </summary>
    private List<ControllerContext> _controllers;

    private IServiceCollection _serviceCollection;
    private IServiceProvider _serviceProvider;

    /// <summary>
    /// The default, if any, controller to use for the CLI. If one is specified, no other controllers can be added to the collection.
    /// </summary>
    private ControllerContext _primaryControllerOverride;

    private readonly CliClientOptions _options;

    private CliClient()
    {
        _controllers = new List<ControllerContext>();
        _serviceCollection = new ServiceCollection();
        _options = new CliClientOptions
        {
            SwitchPrefix = "--"
        };
    }

    private string ResolveControllerReference(ControllerContext controller)
    {
        var controllerReference =
            controller.CliAttribute?.Alias
            ?? controller.Type.Name.Replace(nameof(Controller), string.Empty).Replace("Controller", string.Empty);

        return controllerReference;
    }

    private string ResolveActionReference(ActionContext action)
    {
        var actionReference =
            action.CliAttribute?.Alias
            ?? action.MethodInfo.Name;

        return actionReference;
    }

    private string ResolveParameterReference(ParameterContext parameter)
    {
        var parameterReference =
            parameter.CliAttribute?.Alias
            ?? _options.SwitchPrefix + parameter.ParameterInfo.Name;

        return parameterReference;
    }

    private string ResolvePropertyReference(PropertyInfo property)
    {
        var propertyReference =
            property.GetCustomAttribute<CliAttribute>()?.Alias
            ?? _options.SwitchPrefix + property.Name;

        return propertyReference;
    }

    private void RegisterAssembly(Assembly controllersAssembly)
    {
        var controllerTypes = controllersAssembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Controller)))
            .ToList();

        foreach (var controllerType in controllerTypes)
        {
            RegisterController(controllerType);
        }
    }

    private void RegisterController(Type controllerType)
    {
        if (!controllerType.IsSubclassOf(typeof(Controller)))
            throw new ControllerException($"{controllerType.Name} must inherit from parent class {typeof(Controller)}");

        var controller = new ControllerContext
        {
            Type = controllerType,
            CliAttribute = controllerType.GetCustomAttribute<CliAttribute>(),
            Actions = new List<ActionContext>()
        };

        var actionMethods = controllerType.GetMethods()
            .Where(m => m.IsPublic && m.DeclaringType == controllerType)
            .ToList();

        if (!actionMethods.Any())
            throw new ControllerException($"{controllerType.Name} has no public executable action methods");

        foreach (var actionMethod in actionMethods)
        {
            var action = new ActionContext
            {
                MethodInfo = actionMethod,
                CliAttribute = actionMethod.GetCustomAttribute<CliAttribute>(),
                Parameters = new List<ParameterContext>()
            };

            if (controller.Actions.Any(a => string.Equals(ResolveActionReference(a), ResolveActionReference(action), StringComparison.OrdinalIgnoreCase)))
                throw new ControllerException($"{ResolveActionReference(action)} cannot be specified more than once");

            var actionParameters = actionMethod.GetParameters().ToList();
            if (actionParameters.Count > 1 && !actionParameters.All(ap => Configuration.SimpleTypes.Contains(ap.ParameterType)))
                throw new ControllerException($"Parameter list must either be simple types or one strong type");

            foreach (var actionParamter in actionParameters)
            {
                var parameter = new ParameterContext
                {
                    ParameterInfo = actionParamter,
                    CliAttribute = actionParamter.GetCustomAttribute<CliAttribute>()
                };

                action.Parameters.Add(parameter);
            }

            controller.Actions.Add(action);
        }

        _controllers.Add(controller);

        _serviceCollection.AddTransient(controllerType);
    }

    /// <summary>
    /// Searches the assembly for child classes of type <see cref="Controller"/> and adds them to the controller/action collection.
    /// </summary>
    public CliClient AddControllers(Assembly assembly = null)
    {
        if (assembly is null)
            assembly = Assembly.GetCallingAssembly();

        if (_primaryControllerOverride is not null)
            throw new ControllerException($"A primary controller has already been registered using {nameof(AddPrimaryController)}. Please remove call to {nameof(AddPrimaryController)} to use {nameof(AddControllers)}");

        RegisterAssembly(assembly);

        return this;
    }

    /// <summary>
    /// Adds a single controller to the controller collection.
    /// If this option is used, action controller can not be specified at the CLI.
    /// </summary>
    public CliClient AddControllers(params Type[] controllerTypes)
    {
        foreach (var controllerType in controllerTypes)
        {
            RegisterController(controllerType);
        }

        return this;
    }

    /// <summary>
    /// Adds a single controller to the controller collection.
    /// If this option is used, action controller can not be specified at the CLI.
    /// </summary>
    public CliClient AddPrimaryController(Type controllerType)
    {
        if (_controllers.Any())
            throw new ControllerException($"{_controllers.Count} controllers have already been registerd using {nameof(AddControllers)}. Please remove call to {nameof(AddControllers)} to use {nameof(AddPrimaryController)}.");

        RegisterController(controllerType);

        _primaryControllerOverride = _controllers.Single();
        return this;
    }

    /// <summary>
    /// Adds services to the <see cref="IServiceCollection"/> that will be used to create instances of controllers.
    /// </summary>
    public CliClient AddServices(Action<IServiceCollection> addServices)
    {
        addServices(_serviceCollection);
        return this;
    }

    /// <summary>
    /// Overrides default settings for the CLI client.
    /// </summary>
    public CliClient ConfigureOptions(Action<CliClientOptions> configureOptoins)
    {
        configureOptoins(_options);
        return this;
    }

    /// <summary>
    /// Executes action controller/action based on user parameters.
    /// </summary>
    public CliClient Run(string[] args)
    {
        RunMaster<object>(args.ToList());
        return this;
    }

    /// <summary>
    /// Executes action controller/action based on user parameters.
    /// </summary>
    public T Run<T>(string[] args)
    {
        return RunMaster<T>(args.ToList());
    }

    private T RunMaster<T>(List<string> args)
    {
        _serviceProvider = _serviceCollection.BuildServiceProvider();

        // If action primary controller has been added, it will be inserted to the array at index [0].
        if (_primaryControllerOverride is not null)
            args.Insert(0, ResolveControllerReference(_primaryControllerOverride));

        var targetController = args.ElementAtOrDefault(0);
        if (targetController is null)
            throw new ApplicationException("Must specify a controller");

        var controller = _controllers.FirstOrDefault(controller => string.Equals(targetController, ResolveControllerReference(controller), StringComparison.OrdinalIgnoreCase));
        if (controller is null)
            throw new ApplicationException($"No controller registered with criteria {targetController}");

        var targetAction = args.ElementAtOrDefault(1);
        if (targetAction is null)
            throw new ApplicationException("Must specify an action");

        var action = controller.Actions.FirstOrDefault(action => string.Equals(targetAction, ResolveActionReference(action), StringComparison.OrdinalIgnoreCase));
        if (action is null)
            throw new ApplicationException($"No action found with criteria {targetAction}");

        var actionParameters = default(List<object>);
        var remainingArgs = args.GetRange(2, args.Count - 2);

        if (action.Parameters.Count == 1 && !Configuration.SimpleTypes.Contains(action.Parameters.First().ParameterInfo.ParameterType))
        {
            var actionParameterType = action.Parameters.First().ParameterInfo.ParameterType;
            var actionParameter = Activator.CreateInstance(actionParameterType);

            var positionalParameter = 0;
            foreach (var property in actionParameterType.GetProperties())
            {
                var propertyAttribute = property.GetCustomAttribute<CliAttribute>();
                var propertyReference = ResolvePropertyReference(property);

                var threadMatch = remainingArgs.LastOrDefault(ra => string.Equals(propertyReference, ra, StringComparison.OrdinalIgnoreCase));
                if (threadMatch is null)
                {
                    // Grab the first argument in the remaining arguments list as the unnamed paramter.
                    var firstValue = remainingArgs.FirstOrDefault();
                    if (firstValue is null)
                        continue;
                    else
                        property.SetValue(actionParameter, ArgumentHelper.ConvertValue(property.PropertyType, firstValue));

                    remainingArgs.Remove(propertyReference);
                    remainingArgs.Remove(firstValue);
                    positionalParameter++;

                    continue;
                }

                // Booleans should be treated as true if the switch is present, otherwise false (language default)
                if (property.PropertyType.In(typeof(bool), typeof(bool?)))
                {
                    remainingArgs.Remove(threadMatch);
                    property.SetValue(actionParameter, true);
                    continue;
                }

                var valueIndex = remainingArgs.IndexOf(threadMatch) + 1;
                var stringValue = remainingArgs.ElementAtOrDefault(valueIndex);

                var convertedValue = ArgumentHelper.ConvertValue(property.PropertyType, stringValue);
                property.SetValue(actionParameter, convertedValue);

                remainingArgs.Remove(threadMatch);
                remainingArgs.Remove(stringValue);
            }

            actionParameters = new List<object> { actionParameter };
        }
        else
        {
            actionParameters = new List<object>(action.Parameters.Count);

            var positionalParameter = 0;
            for (var i = 0; i < action.Parameters.Count; i++)
            {
                var parameter = action.Parameters[i];
                var parameterReference = ResolveParameterReference(parameter);

                var threadMatch = remainingArgs.LastOrDefault(ra => string.Equals(parameterReference, ra, StringComparison.OrdinalIgnoreCase));
                if (threadMatch is null)
                {
                    // Grab the first argument in the remaining arguments list as the unnamed paramter.
                    var firstValue = remainingArgs.FirstOrDefault();
                    if (firstValue is null)
                        actionParameters.Add(null);
                    else
                        actionParameters.Add(ArgumentHelper.ConvertValue(parameter.ParameterInfo.ParameterType, firstValue));

                    remainingArgs.Remove(parameterReference);
                    remainingArgs.Remove(firstValue);
                    positionalParameter++;

                    continue;
                }

                // Booleans should be treated as true if the switch is present, otherwise false (language default)
                if (parameter.ParameterInfo.ParameterType.In(typeof(bool), typeof(bool?)))
                {
                    remainingArgs.Remove(threadMatch);
                    actionParameters.Add(true);
                    continue;
                }

                var valueIndex = remainingArgs.IndexOf(threadMatch) + 1;
                var stringValue = remainingArgs.ElementAtOrDefault(valueIndex);

                var convertedValue = ArgumentHelper.ConvertValue(parameter.ParameterInfo.ParameterType, stringValue);
                actionParameters.Add(convertedValue);

                remainingArgs.Remove(threadMatch);
                remainingArgs.Remove(stringValue);
            }
        }

        // Invoke command and handle the response. If the target method is async, it will be handled here.
        var instance = _serviceProvider.GetRequiredService(controller.Type);
        var returned = action.MethodInfo.Invoke(instance, actionParameters.ToArray());

        if (returned is Task emptyTask)
        {
            emptyTask.Wait();
            return default(T);
        }
        else if (returned is Task<T> typedTask)
        {
            typedTask.Wait();
            return typedTask.Result;
        }

        return (T)returned;
    }

    /// <summary>
    /// Creates an instance of <see cref="CliClient"/>.
    /// </summary>
    /// <returns></returns>
    public static CliClient Create()
    {
        var client = new CliClient();
        return client;
    }
}