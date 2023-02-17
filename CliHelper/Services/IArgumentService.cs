using System.Reflection;

namespace CliHelper.Services;

public interface IArgumentService
{
    public CommandContext ExtractCommandContext(ref string args);

    /// <summary>
    /// Uses regex to parse through <paramref name="args"/> for key/value pair <paramref name="targetName"/> and converts the result to <paramref name="targetType"/>
    /// </summary>
    public object ExtractSimpleTypeInstance(Type targetType, string targetName, ref string args);

    /// <summary>
    /// If <paramref name="targetType"/> has a special implementation defined in this library, it will return an instance. Otherwise, it will return null.
    /// </summary>
    public object ExtractSpecialInstance(Type targetType);

    /// <summary>
    /// If the service collection contains an item of type <paramref name="targetType"/>, it will return the instance. Otherwise, it will return an instance using the default constructor.
    /// </summary>
    public object ExtractStronglyTypedInstance(Type targetType, ref string args);

    /// <summary>
    /// Returns an array of parameters that should be passed to the <see cref="MethodInfo"/>, which is determined in a previous step.
    /// </summary>
    public object[] ExtractMethodParameters(MethodInfo method, ref string args);
}
