using CliHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Reflection;
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

    private CliClient()
    {
        _controllers = new List<ControllerContext>();
        _serviceCollection = new ServiceCollection();
    }

    private string ResolveControllerReference(ControllerContext controller)
    {
        var controllerReference = 
            controller.ControllerAttribute?.Alias 
            ?? controller.ControllerType.Name.Replace(nameof(CliController), string.Empty).Replace("Controller", string.Empty);

        return controllerReference;
    }

    private string ResolveActionReference(ActionContext action)
    {
        var actionReference =
            action.ActionAttribute?.Alias
            ?? action.ActionMethod.Name;

        return actionReference;
    }
    
    private void RegisterAssembly(Assembly controllersAssembly)
    {
        var controllerTypes = controllersAssembly.GetTypes()
            .Where(t => t.IsSubclassOf(typeof(CliController)))
            .ToList();

        foreach (var controllerType in controllerTypes)
        {
            RegisterController(controllerType);
        }
    }

    private void RegisterController(Type controllerType)
    {
        if (!controllerType.IsSubclassOf(typeof(CliController)))
            throw new NotImplementedException($"Controller {controllerType.Name} must implement {typeof(CliController)}");

        _serviceCollection.AddTransient(controllerType);

        var controller = new ControllerContext
        {
            ControllerType = controllerType,
            ControllerAttribute = controllerType.GetCustomAttribute<CliAttribute>(),
            Actions = controllerType.GetMethods()
                .Where(m => m.IsPublic && m.DeclaringType == controllerType)
                .Select(methodInfo => new ActionContext
                {
                    ActionMethod = methodInfo,
                    ActionAttribute = methodInfo.GetCustomAttribute<CliAttribute>(),
                    Parameters = methodInfo.GetParameters().Select(p => new ParameterContext
                    {
                        ActionParameter = p,
                        ActionParameterAttribute = p.GetCustomAttribute<CliAttribute>()
                    }).ToList()
                }).ToList()
        };

        _controllers.Add(controller);
    }

    /// <summary>
    /// Searches the assembly for child classes of type <see cref="CliController"/> and adds them to the controller/action collection.
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
    /// Adds action single controller to the controller/action collection.
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

        var remainingArgs = args.GetRange(2, args.Count - 2);






        // Copied from V1 and could be simplified.



        // Enumerator will be used to pull misc args as the process maps the named args.
        var remainingArgsEnumerator = remainingArgs.GetEnumerator();
        var methodParameters = new List<object>();

        // Loop through the paramters on the selected methods.
        foreach (var parameter in action.ActionMethod.GetParameters())
        {
            // Non-Nullable types are required from the user.
            if (parameter.ParameterType.In(
                typeof(bool), typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)
            ))
            {
                if (remainingArgsEnumerator.MoveNext())
                {
                    var cliValue = remainingArgsEnumerator.Current.ToString();
                    var parsedValue = ArgumentHelper.ConvertValue(parameter.ParameterType, cliValue);
                    methodParameters.Add(parsedValue);
                }
                else
                {
                    throw new ApplicationException($"Unable to resolve a {parameter.ParameterType} for parameter {parameter.Name}");
                }
            }
            // Nullable should have action value provided, but is not required. Generally, this should be at the end of the parameter list or removed all together.
            else if (parameter.ParameterType.In(
                typeof(string), typeof(bool?), typeof(short?), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?)
            ))
            {
                if (remainingArgsEnumerator.MoveNext())
                {
                    var cliValue = remainingArgsEnumerator.Current.ToString();
                    var parsedValue = ArgumentHelper.ConvertValue(parameter.ParameterType, cliValue);
                    methodParameters.Add(parsedValue);
                }
                else
                {
                    methodParameters.Add(null);
                }
            }
            // Otherwise, we will either resolve from dependency injection or attempt to bind named arguments to action model.
            else
            {
                var service = _serviceProvider.GetService(parameter.ParameterType);
                if (service is not null)
                    methodParameters.Add(service);
                else
                {
                    // A potential flaw is that if action binded parameter comes before action simple parameter, the simple parameter might be read as the binded parameter name.
                    // For now, binded parameters should appear last in the parameter list.
                    // If we bind action parameter, we may want to leave this loop early.
                    methodParameters.Add(ArgumentHelper.Bind(parameter.ParameterType, remainingArgs.ToArray()));
                    break;
                }
            }
        }

        // Invoke command and handle the response. If the target method is async, it will be handled here.
        var instance = _serviceProvider.GetRequiredService(controller.ControllerType);
        var returned = action.ActionMethod.Invoke(instance, methodParameters.ToArray());

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