using System.Text.Json.Serialization;

namespace FluyoV2.Features.Transactions.Dtos;

[JsonConverter(typeof(CreateRecurrenceRequestJsonConverter))]
public class CreateRecurrenceRequest
{
    public string TransactionId { get; set; } = string.Empty;

    // Expected values: "Mensual", "Quincenal", "Semanal"
    public string Frequency { get; set; } = string.Empty;

    public DateTime NextDate { get; set; }

    // null => Indefinido
    public DateTime? EndDate { get; set; }

    // Monetary fields and associations
    public decimal Amount { get; set; }

    // "Income" or "Expense"
    public string Type { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    // Account where the scheduled movement will be created by the job
    public string AccountId { get; set; } = string.Empty;

    // Optional "other account" control in UI
    public string? OtherAccountId { get; set; }

    // Flag shown on UI: "No se ha pagado"
    public bool IsPaid { get; set; }

    // Optional note
    public string? Note { get; set; }
}
