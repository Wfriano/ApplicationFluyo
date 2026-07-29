using FluentValidation;
using FluyoV2.Features.Commitments.Dtos;

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
    }
}