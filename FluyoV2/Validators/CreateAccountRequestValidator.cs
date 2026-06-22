using FluentValidation;
using FluyoV2.Features.Accounts.Dtos;

namespace FluyoV2.Validators;

public class CreateAccountRequestValidator
    : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre de la cuenta es obligatorio")
            .MaximumLength(100);

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("El tipo de cuenta es obligatorio");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("La moneda es obligatoria");

        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("El saldo inicial no puede ser negativo");
    }
}