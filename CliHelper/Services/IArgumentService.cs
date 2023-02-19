using System.Reflection;

namespace CliHelper.Services;

public interface IArgumentService
{
    /// <summary>
    /// Use regex to parse <paramref name="args"/> for a controller/action pair.
    /// </summary>
    /// <param name="args"></param>
    /// <returns>The <see cref="CommandContext"/> that was found.</returns>
    public CommandContext ExtractCommandContext(ref string args);

    /// <summary>
    /// Use regex to parse <paramref name="args"/> for switch <paramref name="targetName"/> of type <paramref name="targetType"/>.
    /// </summary>
    public object ExtractSimpleTypeInstance(Type targetType, string targetName, ref string args);

    /// <summary>
    /// Returns a custom instance of <paramref name="targetType"/> if supported, otherwise null.
    /// </summary>
    public object ExtractSpecialInstance(Type targetType);

    /// <summary>
    /// Use regex to parse <paramref name="args"/> for each property on <paramref name="targetType"/> and returns the result.
    /// The instance of <paramref name="targetType"/> in the <see cref="IServiceProvider"/> will be used first. If none is registered the default constructor will be used.
    /// </summary>
    public object ExtractStronglyTypedInstance(Type targetType, ref string args);

    /// <summary>
    /// Use regex to parse <paramref name="args"/> to create a <see cref="object[]"/> used to invoke <paramref name="method"/>.
    /// </summary>
    public object[] ExtractMethodParameters(MethodInfo method, ref string args);


    /// <summary>
    /// Converts <paramref name="stringValue"/> to <paramref name="targetType"/>.
    /// </summary>
    /// <exception cref="InvalidCastException"></exception>
    public object MasterConvertSimpleType(Type targetType, string stringValue);
}
