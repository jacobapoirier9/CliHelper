internal static class Configuration
{
    public static readonly Type[] SimpleTypes = new Type[]
    {
        typeof(string),

        typeof(bool), typeof(char), typeof(DateTime), typeof(TimeSpan),
        typeof(bool?), typeof(char?), typeof(DateTime?), typeof(TimeSpan?),

        typeof(byte), typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(decimal),
        typeof(byte?), typeof(short?), typeof(int?), typeof(long?), typeof(float?), typeof(double?), typeof(decimal?),
    };
}
