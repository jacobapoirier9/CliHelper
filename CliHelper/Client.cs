using CliHelper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.Design;
using System.Reflection;

namespace CliHelper;

public sealed class Client
{
    private readonly ISettings _settings = new Settings()
    {
        RequireControllerName = false,
        RequireActionName = false,
        DisableInteractiveShell = false,
        InteractiveShellPrompt = " > ",
        InteractiveShellBanner = null,
        InteractiveShellHandleErrors = null,
        ConsiderTrueStrings = new string[] { "true", "yes", "y", "1" },
        ConsiderFalseStrings = new string[] { "false", "no", "n", "0" },
        InteractiveShellShowHelpOnInvalidCommand = true
    };

    private readonly List<CommandContext> _commandContexts = new List<CommandContext>();

    private readonly IServiceCollection _serviceCollection = new ServiceCollection();
    private IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new <see cref="Client"/>.
    /// </summary>
    /// <returns></returns>
    public static Client Create()
    {
        var client = new Client();
        return client;
    }

    /// <summary>
    /// Searches <paramref name="assembly"/> for all types that inherit <see cref="Controller"/> and adds to the command collection.
    /// </summary>
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
    public Client AddControllers(params Type[] types)
    {
        foreach (var type in types)
        {
            _serviceCollection.AddTransient(type);

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

                _commandContexts.Add(commandContext);
            }
        }

        return this;
    }

    /// <summary>
    /// Adjust default configuration settings.
    /// </summary>
    public Client Configure(Action<ISettings> configure)
    {
        configure(_settings);
        return this;
    }

    /// <summary>
    /// Add custom services to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="configureServices"></param>
    /// <returns></returns>
    public Client AddServices(Action<IServiceCollection> configureServices)
    {
        configureServices(_serviceCollection);
        return this;
    }

    /// <summary>
    /// Start the client with arguments from the command line.
    /// </summary>
    public Client Run(string[] args)
    {
        AddControllers(typeof(DefaultController));

        _serviceCollection.AddSingleton(_settings);
        _serviceCollection.AddSingleton(_commandContexts);
        _serviceCollection.AddSingleton<IArgumentService, ArgumentService>();
        _serviceCollection.AddSingleton<ICommandService, CommandService>();

        _serviceProvider = _serviceCollection.BuildServiceProvider();

        var shellService = _serviceProvider.GetRequiredService<ICommandService>();
        shellService.HandleInputString(string.Join(' ', args));

        return this;
    }
}
