﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.Design;
using System.Reflection;

namespace CliHelper;

public sealed class Client
{
    private readonly Configuration _configuration;

    private readonly List<CommandContext> _commandContexts = new List<CommandContext>();

    private readonly IServiceCollection _serviceCollection;
    private IServiceProvider _serviceProvider;

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

        _serviceCollection.AddSingleton(_configuration);
        _serviceCollection.AddSingleton(_commandContexts);
        _serviceCollection.AddSingleton<IArgumentService, ArgumentService>();

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
        var argumentParser = _serviceProvider.GetRequiredService<IArgumentService>();

        var commandContext = argumentParser.ExtractCommandContext(ref args);
        var instance = _serviceProvider.GetRequiredService(commandContext.Type);

        if (commandContext.Type.IsSubclassOf(typeof(Controller)))
        {
            var selectedCommandContextProperty = typeof(Controller).GetProperty(nameof(Controller.SelectedCommandContext));
            selectedCommandContextProperty.SetValue(instance, commandContext);

            var configurationProperty = typeof(Controller).GetProperty(nameof(Controller.Configuration));
            configurationProperty.SetValue(instance, _configuration);
        }

        var parameters = argumentParser.ExtractMethodParameters(commandContext.Method, ref args);
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
}
