using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

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

        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException($"Unable to convert JSON token {reader.TokenType} to CreateRecurrenceRequest");

        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        // helper to get property case-insensitively
        static bool TryGetPropertyIgnoreCase(JsonElement element, string name, out JsonElement value)
        {
            if (element.ValueKind != JsonValueKind.Object)
            {
                value = default;
                return false;
            }

            if (element.TryGetProperty(name, out value))
                return true;

            foreach (var prop in element.EnumerateObject())
            {
                if (string.Equals(prop.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    value = prop.Value;
                    return true;
                }
            }

            value = default;
            return false;
        }

        var result = new CreateRecurrenceRequest();

        if (TryGetPropertyIgnoreCase(root, "transactionId", out var tId) && tId.ValueKind == JsonValueKind.String)
            result.TransactionId = tId.GetString() ?? string.Empty;

        if (TryGetPropertyIgnoreCase(root, "frequency", out var freq) && freq.ValueKind == JsonValueKind.String)
            result.Frequency = freq.GetString() ?? string.Empty;

        if (TryGetPropertyIgnoreCase(root, "nextDate", out var next) && next.ValueKind != JsonValueKind.Null)
        {
            if (next.ValueKind == JsonValueKind.String)
            {
                var s = next.GetString();
                if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dt))
                    result.NextDate = dt;
            }
            else if (next.ValueKind == JsonValueKind.Number && next.TryGetDateTime(out var dtNum))
            {
                result.NextDate = dtNum;
            }
        }

        if (TryGetPropertyIgnoreCase(root, "endDate", out var end) && end.ValueKind != JsonValueKind.Null)
        {
            if (end.ValueKind == JsonValueKind.String)
            {
                var s = end.GetString();
                if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var dt))
                    result.EndDate = dt;
            }
            else if (end.ValueKind == JsonValueKind.Number && end.TryGetDateTime(out var dtNum))
            {
                result.EndDate = dtNum;
            }
        }

        if (TryGetPropertyIgnoreCase(root, "amount", out var amt) && amt.ValueKind != JsonValueKind.Null)
        {
            if (amt.ValueKind == JsonValueKind.Number && amt.TryGetDecimal(out var dec))
                result.Amount = dec;
            else if (amt.ValueKind == JsonValueKind.String)
            {
                var s = amt.GetString();
                if (decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out var d))
                    result.Amount = d;
            }
        }

        if (TryGetPropertyIgnoreCase(root, "type", out var typeEl) && typeEl.ValueKind == JsonValueKind.String)
            result.Type = typeEl.GetString() ?? string.Empty;

        if (TryGetPropertyIgnoreCase(root, "category", out var cat) && cat.ValueKind == JsonValueKind.String)
            result.Category = cat.GetString() ?? string.Empty;

        if (TryGetPropertyIgnoreCase(root, "description", out var desc) && desc.ValueKind == JsonValueKind.String)
            result.Description = desc.GetString() ?? string.Empty;

        if (TryGetPropertyIgnoreCase(root, "accountId", out var acc) && acc.ValueKind == JsonValueKind.String)
            result.AccountId = acc.GetString() ?? string.Empty;

        if (TryGetPropertyIgnoreCase(root, "otherAccountId", out var oacc) && oacc.ValueKind == JsonValueKind.String)
            result.OtherAccountId = oacc.GetString();

        if (TryGetPropertyIgnoreCase(root, "isPaid", out var isPaid) && isPaid.ValueKind != JsonValueKind.Null)
        {
            if (isPaid.ValueKind == JsonValueKind.True || isPaid.ValueKind == JsonValueKind.False)
                result.IsPaid = isPaid.GetBoolean();
            else if (isPaid.ValueKind == JsonValueKind.String)
            {
                var s = isPaid.GetString();
                if (bool.TryParse(s, out var b)) result.IsPaid = b;
            }
        }

        if (TryGetPropertyIgnoreCase(root, "note", out var note) && note.ValueKind == JsonValueKind.String)
            result.Note = note.GetString();

        return result;
    }

    public override void Write(Utf8JsonWriter writer, CreateRecurrenceRequest? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteBooleanValue(false);
            return;
        }

        writer.WriteStartObject();

        writer.WriteString("transactionId", value.TransactionId);
        writer.WriteString("frequency", value.Frequency);
        writer.WriteString("nextDate", value.NextDate.ToString("o"));
        if (value.EndDate.HasValue)
            writer.WriteString("endDate", value.EndDate.Value.ToString("o"));
        writer.WriteNumber("amount", value.Amount);
        writer.WriteString("type", value.Type);
        writer.WriteString("category", value.Category);
        writer.WriteString("description", value.Description);
        writer.WriteString("accountId", value.AccountId);
        if (!string.IsNullOrWhiteSpace(value.OtherAccountId))
            writer.WriteString("otherAccountId", value.OtherAccountId);
        writer.WriteBoolean("isPaid", value.IsPaid);
        if (!string.IsNullOrWhiteSpace(value.Note))
            writer.WriteString("note", value.Note);

        writer.WriteEndObject();
    }
}
