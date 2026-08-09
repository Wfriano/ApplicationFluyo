namespace FluyoV2.Features.Transactions.Dtos;

public class CreateTransactionRequest
{
    public string? AccountId { get; set; }

    public string Category { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    // true => apply immediately; false => schedule for first day of selected month
    public bool IsPaid { get; set; } = true;

    // Optional recurrence settings. If present, creates recurrence from this same endpoint.
    public CreateRecurrenceRequest? Recurrence { get; set; }

    // Only for category "Préstamo"
    public int? LoanPaymentDay { get; set; }
    public int? LoanInstallments { get; set; }
    public decimal? LoanInstallmentAmount { get; set; }
}
