using CliHelper.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CliHelper;

public class CommandService : ICommandService
{
    private ISettings _settings;
    private IServiceProvider _serviceProvider;

    public CommandService(ISettings settings, IServiceProvider serviceProvider)
    {
        _settings = settings;
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
        if (_settings.InteractiveShellBanner is not null)
            Console.WriteLine(_settings.InteractiveShellBanner);

        if (_settings.DisableInteractiveShell)
            throw new ApplicationException("Interactive shell has been disabled. No arguments were passed to the application.");

        do
        {
            try
            {
                Console.Write(_settings.InteractiveShellPrompt);

                var args = Console.ReadLine();
                HandleNonInteractiveShell<object>(args);
            }
            catch (Exception ex) // TODO: allows clients to handle the exception thrown here?
            {
                if (_settings.InteractiveShellHandleErrors is null)
                    Console.WriteLine("Invalid command");
                else
                    _settings.InteractiveShellHandleErrors(ex);

                if (_settings.InteractiveShellShowHelpOnInvalidCommand)
                {
                    Console.WriteLine();
                    HandleNonInteractiveShell<object>("help");
                }
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

            var configurationProperty = typeof(Controller).GetProperty(nameof(Controller.Settings));
            configurationProperty.SetValue(instance, _settings);
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