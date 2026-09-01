using FluentValidation;
using FluyoV2.Features.Commitments.Dtos;
using System.Text.RegularExpressions;

namespace FluyoV2.Validators;

public class CreateCommitmentRequestValidator
    : AbstractValidator<CreateCommitmentRequest>
{
    public CreateCommitmentRequestValidator()
    {
        // AccountId is optional: a commitment can be created without selecting an account
        // RuleFor(x => x.AccountId)
        //     .NotEmpty()
        //     .WithMessage("La cuenta es obligatoria");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre del compromiso es obligatorio")
            .MaximumLength(100);

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("La categoría es obligatoria");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El valor del compromiso debe ser mayor a cero");

        // PaymentDate is optional; when provided ensure it's a valid date
        When(x => x.PaymentDate.HasValue, () =>
        {
            RuleFor(x => x.PaymentDate.Value)
                .LessThan(DateTime.MaxValue);
        });

        // Notes optional but limit length
        When(x => !string.IsNullOrEmpty(x.Notes), () =>
        {
            RuleFor(x => x.Notes).MaximumLength(500);
        });

        // Optional recurrence validation: only validate when the client provided meaningful recurrence data
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

        // If recurrence is marked as paid, require a valid account id
        When(x => x.Recurrence != null && x.Recurrence.IsPaid, () =>
        {
            RuleFor(x => x.Recurrence!.AccountId)
                .NotEmpty()
                .WithMessage("La cuenta de la recurrencia es obligatoria cuando está marcada como pagada")
                .Must(id => !string.IsNullOrWhiteSpace(id) && System.Text.RegularExpressions.Regex.IsMatch(id, "^[0-9a-fA-F]{24}$"))
                .WithMessage("AccountId de la recurrencia no es un ObjectId válido");
        });
    }
}