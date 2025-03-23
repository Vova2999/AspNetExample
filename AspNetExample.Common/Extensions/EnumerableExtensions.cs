namespace AspNetExample.Common.Extensions;

public static class EnumerableExtensions
{
    public static void ForEach<TValue>(this IEnumerable<TValue> values, Action<TValue> action)
    {
        foreach (var value in values)
            action(value);
    }

    public static IEnumerable<TValue> EmptyIfNull<TValue>(this IEnumerable<TValue>? values)
    {
        return values ?? [];
    }
}