using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FluyoV2.Features.Transactions.Dtos;

public class CreateRecurrenceRequestJsonConverter : JsonConverter<CreateRecurrenceRequest>
{
    public override CreateRecurrenceRequest? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        if (reader.TokenType == JsonTokenType.False || reader.TokenType == JsonTokenType.True)
        {
            // treat boolean false/true as absence of recurrence
            // consume the boolean value
            _ = reader.GetBoolean();
            return null;
        }

        if (reader.TokenType == JsonTokenType.StartObject)
        {
            // delegate to default deserialization for the object
            return JsonSerializer.Deserialize<CreateRecurrenceRequest?>(ref reader, options);
        }

        // Unexpected token
        throw new JsonException($"Unable to convert JSON token {reader.TokenType} to CreateRecurrenceRequest");
    }

    public override void Write(Utf8JsonWriter writer, CreateRecurrenceRequest? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            // write boolean false to keep compatibility with clients that expect false
            writer.WriteBooleanValue(false);
            return;
        }

        JsonSerializer.Serialize(writer, value, options);
    }
}
