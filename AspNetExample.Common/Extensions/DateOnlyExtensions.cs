namespace AspNetExample.Common.Extensions;

public static class DateOnlyExtensions
{
    public static DateTime ToDateTime(this DateOnly dateOnly)
    {
        return dateOnly.ToDateTime(default);
    }
}