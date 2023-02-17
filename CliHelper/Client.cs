using CliHelper.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.Design;
using System.Reflection;

namespace CliHelper;

public sealed class Client
{
    private readonly IConfiguration _configuration = new Configuration()
    {
        RequireControllerName = false,
        RequireActionName = false,
        DisableInteractiveShell = false,
        InteractiveShellPrompt = " > ",
        InteractiveShellBanner = null,
        InteractiveShellHandleErrors = null
    };

    private readonly List<CommandContext> _commandContexts = new List<CommandContext>();

    private readonly IServiceCollection _serviceCollection = new ServiceCollection();
    private IServiceProvider _serviceProvider;

    public static Client Create()
    {
        var client = new Client();
        return client;
    }

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

    public Client Configure(Action<IConfiguration> configure)
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

        _serviceCollection.AddSingleton(_configuration);
        _serviceCollection.AddSingleton(_commandContexts);
        _serviceCollection.AddSingleton<IArgumentService, ArgumentService>();
        _serviceCollection.AddSingleton<ICommandService, CommandService>();

        _serviceProvider = _serviceCollection.BuildServiceProvider();

        var shellService = _serviceProvider.GetRequiredService<ICommandService>();
        shellService.HandleInputString(string.Join(' ', args));

        return this;
    }
}
