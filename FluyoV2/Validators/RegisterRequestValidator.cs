using FluentValidation;
using FluyoV2.Features.Auth.Dtos;

namespace FluyoV2.Validators;

public class RegisterRequestValidator
    : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("El nombre es obligatorio")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("El correo es obligatorio")
            .EmailAddress()
            .WithMessage("El correo no tiene un formato válido");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("La contraseña es obligatoria")
            .MinimumLength(6)
            .WithMessage("La contraseña debe tener mínimo 6 caracteres");
    }
}