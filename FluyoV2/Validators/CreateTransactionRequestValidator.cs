using FluentValidation;
using FluyoV2.Features.Transactions.Dtos;

namespace FluyoV2.Validators;

public class CreateTransactionRequestValidator
    : AbstractValidator<CreateTransactionRequest>
{
    public CreateTransactionRequestValidator()
    {
        // AccountId is required only when the transaction is marked as paid.
        When(x => x.IsPaid, () =>
        {
            RuleFor(x => x.AccountId)
                .NotEmpty()
                .WithMessage("La cuenta es obligatoria cuando el movimiento está marcado como pagado");
        });

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("La categoría es obligatoria");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El valor debe ser mayor a cero");

        RuleFor(x => x.TransactionDate)
            .Must(x => x != default)
            .WithMessage("La fecha es obligatoria");

        // Only validate recurrence fields when the client provided meaningful recurrence data.
        // This avoids forcing validation when an empty object ("Recurrence": {}) is sent.
        When(x => x.Recurrence != null && (
            !string.IsNullOrWhiteSpace(x.Recurrence.Frequency) ||
            x.Recurrence.NextDate > DateTime.MinValue ||
            x.Recurrence.Amount > 0 ||
            !string.IsNullOrWhiteSpace(x.Recurrence.Type) ||
            !string.IsNullOrWhiteSpace(x.Recurrence.AccountId)
        ), () =>
        {
            RuleFor(x => x.Recurrence!.Frequency)
                .NotEmpty()
                .WithMessage("La frecuencia de la recurrencia es obligatoria");

            RuleFor(x => x.Recurrence!.NextDate)
                .GreaterThan(DateTime.MinValue)
                .WithMessage("La próxima fecha de recurrencia es obligatoria");

            RuleFor(x => x.Recurrence!.Amount)
                .GreaterThan(0)
                .WithMessage("El valor de la recurrencia debe ser mayor a cero");
        });

        When(x => IsLoanCategory(x.Category), () =>
        {
            RuleFor(x => x.LoanPaymentDay)
                .NotNull()
                .WithMessage("El día de pago es obligatorio para préstamo")
                .InclusiveBetween(1, 31)
                .WithMessage("El día de pago debe estar entre 1 y 31");

            RuleFor(x => x.LoanInstallments)
                .NotNull()
                .WithMessage("El número de cuotas es obligatorio para préstamo")
                .GreaterThan(0)
                .WithMessage("El número de cuotas debe ser mayor a cero");

            RuleFor(x => x.LoanInstallmentAmount)
                .NotNull()
                .WithMessage("El valor de la cuota es obligatorio para préstamo")
                .GreaterThan(0)
                .WithMessage("El valor de la cuota debe ser mayor a cero");
        });
    }

    private static bool IsLoanCategory(string? category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return false;

        return category.Equals("Préstamo", StringComparison.OrdinalIgnoreCase)
            || category.Equals("Prestamo", StringComparison.OrdinalIgnoreCase)
            || category.Equals("Prestamos", StringComparison.OrdinalIgnoreCase)
            || category.Equals("Préstamos", StringComparison.OrdinalIgnoreCase);
    }
}
