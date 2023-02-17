using CliHelper.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CliHelper;

public class CommandService : ICommandService
{
    private IConfiguration _configuration;
    private IServiceProvider _serviceProvider;

    public CommandService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
    }

    public void HandleInputString(string args)
    {
        if (string.IsNullOrEmpty(args))
            HandleInteractiveShell();
        else
            HandleNonInteractiveShell<object>(args);
    }

    public void HandleInteractiveShell()
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
                HandleNonInteractiveShell<object>(args);
            }
            catch (Exception ex) // TODO: allows clients to handle the exception thrown here?
            {
                Console.WriteLine("Invalid command");
            }
        } while (true);
    }

    public T HandleNonInteractiveShell<T>(string args)
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