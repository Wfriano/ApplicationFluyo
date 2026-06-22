using FluentValidation;
using FluyoV2.Features.Goals.Dtos;

namespace FluyoV2.Validators;

public class CreateGoalRequestValidator
    : AbstractValidator<CreateGoalRequest>
{
    public CreateGoalRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre de la meta es obligatorio")
            .MaximumLength(100);

        RuleFor(x => x.TargetAmount)
            .GreaterThan(0)
            .WithMessage("El valor objetivo debe ser mayor a cero");
    }
}