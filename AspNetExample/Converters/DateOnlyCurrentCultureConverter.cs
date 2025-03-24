using System.ComponentModel;
using System.Globalization;

namespace AspNetExample.Converters;

public class DateOnlyCurrentCultureConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        return value is string str && DateOnly.TryParse(str, CultureInfo.CurrentCulture, out var result) ? result : base.ConvertFrom(context, culture, value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        return destinationType == typeof(string) && value is DateOnly dateOnly ? dateOnly.ToString(CultureInfo.CurrentCulture) : base.ConvertTo(context, culture, value, destinationType);
    }
}