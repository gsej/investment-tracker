using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FileReaders.JsonConverters;

public class StringConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            var numericValue = reader.GetDecimal();
            return numericValue.ToString(CultureInfo.InvariantCulture);
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
