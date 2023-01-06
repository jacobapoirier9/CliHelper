public static class Extensions
{
    internal static bool In<T>(this T instance, params T[] items)
    {
        foreach (var item in items)
        {
            if (object.Equals(instance, item))
                return true;
        }

        return false;
    }

    internal static bool Many<T>(this ICollection<T> items)
    {
        return items.Count > 1;
    }
}