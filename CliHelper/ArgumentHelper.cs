using CliHelper;
using System.Reflection;

internal static class ArgumentHelper
{
    private static readonly Type[] _nullableTypes = new Type[]
        { typeof(string), typeof(bool?), typeof(byte?), typeof(short?), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?)};

    private static readonly Type[] _nonNullableTypes = new Type[]
        { typeof(bool), typeof(byte), typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal)};

    /// <summary>
    /// Converts <paramref name="stringValue"/> into the specified <paramref name="targetType"/>
    /// </summary>
    internal static object ConvertValue(Type targetType, string stringValue)
    {
        if (targetType.In(typeof(string)))
            return stringValue;

        else if (targetType.In(typeof(bool), typeof(bool?)))
        {
            if (bool.TryParse(stringValue, out var parsedValue))
                return parsedValue;
        }
        else if (targetType.In(typeof(byte), typeof(byte?)))
        {
            if (byte.TryParse(stringValue, out var parsedValue))
                return parsedValue;
        }
        else if (targetType.In(typeof(short), typeof(short?)))
        {
            if (short.TryParse(stringValue, out var parsedValue))
                return parsedValue;
        }
        else if (targetType.In(typeof(int), typeof(int?)))
        {
            if (int.TryParse(stringValue, out var parsedValue))
                return parsedValue;
        }
        else if (targetType.In(typeof(long), typeof(long?)))
        {
            if (long.TryParse(stringValue, out var parsedValue))
                return parsedValue;
        }
        else if (targetType.In(typeof(float), typeof(float?)))
        {
            if (float.TryParse(stringValue, out var parsedValue))
                return parsedValue;
        }
        else if (targetType.In(typeof(double), typeof(double?)))
        {
            if (double.TryParse(stringValue, out var parsedValue))
                return parsedValue;
        }
        else if (targetType.In(typeof(decimal), typeof(decimal?)))
        {
            if (decimal.TryParse(stringValue, out var parsedValue))
                return parsedValue;
        }

        if (targetType.In(_nullableTypes))
            return null;

        throw new FormatException($"Could not convert {stringValue} to {targetType.FullName}");
    }

    /// <summary>
    /// Binds named arguments from <paramref name="args"/> to the corresponding property on <paramref name="type"/>
    /// </summary>
    internal static object Bind(Type type, string[] args)
    {
        var list = args.ToList();

        var instance = Activator.CreateInstance(type);

        foreach (var property in type.GetProperties())
        {
            var attribute = property.GetCustomAttribute<CliAttribute>();

            // Find named parameter in args.
            var match = list.SingleOrDefault(li => string.Equals(attribute?.Alias ?? property.Name, li, StringComparison.OrdinalIgnoreCase));
            var keyIndex = list.IndexOf(match);
            if (keyIndex < 0)
                continue;

            // When binding arguments to li model, li boolean just has to be present to be binded correctly.
            if (property.PropertyType.In(typeof(bool), typeof(bool?)))
            {
                property.SetValue(instance, true);
                continue;
            }

            var valueIndex = keyIndex + 1;
            var stringValue = list[valueIndex];

            // Set value
            var value = ConvertValue(property.PropertyType, stringValue);
            property.SetValue(instance, value);
        }

        return instance;
    }
}
