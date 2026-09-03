using FluentValidation;
using FluyoV2.Features.Goals.Dtos;

namespace FluyoV2.Validators;

public class CompleteGoalRequestValidator : AbstractValidator<CompleteGoalRequest>
{
    public CompleteGoalRequestValidator()
    {
        RuleFor(x => x.AccountId)
            .NotEmpty()
            .WithMessage("La cuenta es obligatoria");

        RuleFor(x => x.Category)
            .NotEmpty()
            .WithMessage("La categoría es obligatoria");
    }
}
