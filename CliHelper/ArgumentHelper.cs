using CliHelper;
using System.Reflection;

internal static class ArgumentHelper
{
    private static readonly Type[] _nullableTypes = new Type[]
        { typeof(string), typeof(bool?), typeof(byte?), typeof(short?), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?)};

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
}
