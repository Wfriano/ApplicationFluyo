using FluentValidation;
using FluyoV2.Features.Commitments.Dtos;

namespace FluyoV2.Validators;

public class UpdateCommitmentRequestValidator
    : AbstractValidator<UpdateCommitmentRequest>
{
    public UpdateCommitmentRequestValidator()
    {
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

        RuleFor(x => x.DayOfMonth)
            .InclusiveBetween(1, 31)
            .WithMessage("El día de pago debe estar entre 1 y 31");
    }
}