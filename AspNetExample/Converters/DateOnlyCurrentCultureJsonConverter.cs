using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetExample.Converters;

public sealed class DateOnlyCurrentCultureJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateOnly.Parse(reader.GetString()!, CultureInfo.CurrentCulture);
    }

    public override DateOnly ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateOnly.Parse(reader.GetString()!, CultureInfo.CurrentCulture);
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(CultureInfo.CurrentCulture));
    }

    public override void WriteAsPropertyName(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WritePropertyName(value.ToString(CultureInfo.CurrentCulture));
    }
}