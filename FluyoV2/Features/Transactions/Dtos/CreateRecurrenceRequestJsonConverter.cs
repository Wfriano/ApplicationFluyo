using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FluyoV2.Features.Transactions.Dtos;

public class CreateRecurrenceRequestJsonConverter : JsonConverter<CreateRecurrenceRequest?>
{
    public override CreateRecurrenceRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.False || reader.TokenType == JsonTokenType.True)
        {
            // treat boolean false/true as absence of recurrence
            _ = reader.GetBoolean();
            return null;
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // parse object into a JsonDocument to avoid recursive converter calls

            using var doc = JsonDocument.ParseValue(ref reader);
            var json = doc.RootElement.GetRawText();

            // use a new options instance without this converter to avoid recursion
            var newOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive
            };

            return JsonSerializer.Deserialize<CreateRecurrenceRequest?>(json, newOptions);
        }

        throw new JsonException($"Unable to convert JSON token {reader.TokenType} to CreateRecurrenceRequest");
    }

    public override void Write(Utf8JsonWriter writer, CreateRecurrenceRequest? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteBooleanValue(false);
            return;
        }

        // serialize using a fresh options instance to avoid calling this converter again
        var newOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = options.PropertyNameCaseInsensitive
        };

        JsonSerializer.Serialize(writer, value, newOptions);
    }
}
