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
    private List<CliExecutionContext> _cliExecutionContexts;

    private IServiceCollection _serviceCollection;
    private IServiceProvider _serviceProvider;

    /// <summary>
    /// The default, if any, controller to use for the CLI. If one is specified, no other controllers can be added to the collection.
    /// </summary>
    private string _primaryControllerOverride;

    private CliClient()
    {
        _cliExecutionContexts = new List<CliExecutionContext>();
        _serviceCollection = new ServiceCollection();
    }

    private string ResolveControllerReference(CliExecutionContext executionContext)
    {
        var controllerReference = 
            executionContext.ControllerAttribute?.Alias 
            ?? executionContext.ControllerType.Name.Replace(nameof(CliController), string.Empty).Replace("Controller", string.Empty);

        return controllerReference;
    }

    private string ResolveActionReference(CliExecutionContext executionContext)
    {
        var actionReference =
            executionContext.ActionAttribute?.Alias
            ?? executionContext.ActionMethod.Name;

        return actionReference;
    }

    private CliExecutionContext GetCliExecutionContext(CliArguments cliArguments)
    {
        var filteredByController = _cliExecutionContexts
            //.Where(ctx => (ctx.ControllerAttribute?.Alias ?? ctx.ControllerType.Name).StartsWith(cliArguments.CliController, StringComparison.OrdinalIgnoreCase))
            .Where(ctx => string.Equals(
                ResolveControllerReference(ctx), 
                cliArguments.CliController, 
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!filteredByController.Any())
            throw new ApplicationException($"Could not find a matching controller for {cliArguments.CliController}");

        var filteredByAction = filteredByController
             .Where(ctx => string.Equals(
                 ResolveActionReference(ctx), 
                 cliArguments.CliAction, 
                 StringComparison.OrdinalIgnoreCase))
             .ToList();

        if (!filteredByAction.Any())
            throw new ApplicationException($"Could not find a matching action for {cliArguments.CliAction} on controller {cliArguments.CliController}");

        // TODO: this should be moved to the AddControllers functionality.
        if (filteredByAction.Many())
            throw new ApplicationException($"Could not resolve a single action for {cliArguments.CliAction} on controller {cliArguments.CliController}");

        var executionContext = filteredByAction.Single();
        return executionContext;
    }
    
    private void RegisterAssembly(Assembly controllersAssembly)
    {
        var cliControllerActionWrappers = new List<CliExecutionContext>();

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
        // Controllers must inherit CliController. This is the flag for controllers and common functionality for controllers could be added in the future similar to MVC.
        if (!controllerType.IsSubclassOf(typeof(CliController)))
            throw new NotImplementedException($"Controller {controllerType.Name} must implement {typeof(CliController)}");

        var controllerAttribute = controllerType.GetCustomAttribute<CliAttribute>();

        var actionMethods = controllerType.GetMethods()
            .Where(m => m.IsPublic && m.DeclaringType == controllerType)
            .ToList();

        foreach (var actionMethod in actionMethods)
        {
            var actionAttribute = actionMethod.GetCustomAttribute<CliAttribute>();

            _cliExecutionContexts.Add(new CliExecutionContext
            {
                ControllerType = controllerType,
                ControllerAttribute = controllerAttribute,
                ActionMethod = actionMethod,
                ActionAttribute = actionAttribute
            });
        }

        _serviceCollection.AddTransient(controllerType);
    }

    /// <summary>
    /// Searches the assembly for child classes of type <see cref="CliController"/> and adds them to the controller/action collection.
    /// </summary>
    public CliClient AddControllers(Assembly assembly = null)
    {
        // Default to the calling assembly so the user does not have to manually provide the assembly but can if needed.
        if (assembly is null)
            assembly = Assembly.GetCallingAssembly();

        // If a primary controller is set, the user should not be able to add any more controllers.
        if (_primaryControllerOverride is not null)
            throw new ControllerAlreadyAddedException($"A primary controller has already been registered using {nameof(AddPrimaryController)}. Please remove call to {nameof(AddPrimaryController)} to use {nameof(AddControllers)}");

        RegisterAssembly(assembly);

        return this;
    }

    /// <summary>
    /// Adds a single controller to the controller/action collection.
    /// If this option is used, a controller can not be specified at the CLI.
    /// </summary>
    public CliClient AddPrimaryController(Type controllerType)
    {
        // If controllers have been added, the user should not be able to set a primary controller.
        if (_cliExecutionContexts.Count > 0)
        {
            var controllerCount = _cliExecutionContexts
                .Select(a => a.ControllerType)
                .Distinct()
                .Count();

            throw new ControllerAlreadyAddedException($"{controllerCount} controllers have already been registerd using {nameof(AddControllers)}. Please remove call to {nameof(AddControllers)} to use {nameof(AddPrimaryController)}.");
        }


        RegisterController(controllerType);

        var executionContext = _cliExecutionContexts.First() ;// TODO: Should we do a null check here to make sure at least one action is added?
        _primaryControllerOverride = ResolveControllerReference(executionContext);

        _serviceCollection.TryAddTransient(controllerType);

        return this;
    }

    /// <summary>
    /// Adds services to the <see cref="IServiceCollection"/> that will be used to create instances of controllers.
    /// </summary>
    public CliClient AddServices(Action<IServiceCollection> addServices)
    {
        addServices(_serviceCollection);
        _serviceProvider = _serviceCollection.BuildServiceProvider();

        return this;
    }

    /// <summary>
    /// Executes a controller/action based on user parameters.
    /// </summary>
    public CliClient Run(string[] args)
    {
        Run<object>(args);
        return this;
    }

    /// <summary>
    /// Executes a controller/action based on user parameters.
    /// </summary>
    public T Run<T>(string[] args)
    {
        if (_serviceProvider is null)
            _serviceProvider = _serviceCollection.BuildServiceProvider();

        // If a primary controller has been added, it will be inserted to the array at index [0].
        if (_primaryControllerOverride is not null)
        {
            args = ArgumentHelper.InsertController(args, _primaryControllerOverride);
        }

        var cliArgs = ArgumentHelper.ParseCliArguments(args);
        var executionContext = GetCliExecutionContext(cliArgs);





        // Copied from V1 and could be simplified.



        // Enumerator will be used to pull misc args as the process maps the named args.
        var remainingArgsEnumerator = cliArgs.RemainingArgs.GetEnumerator();
        var methodParameters = new List<object>();

        // Loop through the paramters on the selected methods.
        foreach (var parameter in executionContext.ActionMethod.GetParameters())
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
            // Nullable should have a value provided, but is not required. Generally, this should be at the end of the parameter list or removed all together.
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
            // Otherwise, we will either resolve from dependency injection or attempt to bind named arguments to a model.
            else
            {
                var service = _serviceProvider.GetService(parameter.ParameterType);
                if (service is not null)
                    methodParameters.Add(service);
                else
                {
                    // A potential flaw is that if a binded parameter comes before a simple parameter, the simple parameter might be read as the binded parameter name.
                    // For now, binded parameters should appear last in the parameter list.
                    // If we bind a parameter, we may want to leave this loop early.
                    methodParameters.Add(ArgumentHelper.Bind(parameter.ParameterType, cliArgs.RemainingArgs));
                    break;
                }
            }
        }

        // Invoke command and handle the response. If the target method is async, it will be handled here.
        var instance = _serviceProvider.GetRequiredService(executionContext.ControllerType);
        var returned = executionContext.ActionMethod.Invoke(instance, methodParameters.ToArray());

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