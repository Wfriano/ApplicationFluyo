using FluentValidation;
using FluyoV2.Features.Transactions.Dtos;

namespace FluyoV2.Validators;

public class CreateTransactionWithRecurrenceRequestValidator
    : AbstractValidator<CreateTransactionWithRecurrenceRequest>
{
    public CreateTransactionWithRecurrenceRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("La cuenta es obligatoria");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("La categoría es obligatoria");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El valor debe ser mayor a cero");

        When(x => x.Recurrence != null, () =>
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

            RuleFor(x => x.Recurrence!.AccountId)
                .NotEmpty()
                .WithMessage("La cuenta de la recurrencia es obligatoria");
        });
    }
}
