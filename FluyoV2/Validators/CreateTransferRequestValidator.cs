using FluentValidation;
using FluyoV2.Features.Transfers.Dtos;

namespace FluyoV2.Validators;

public class CreateTransferRequestValidator
    : AbstractValidator<CreateTransferRequest>
{
    public CreateTransferRequestValidator()
    {
        RuleFor(x => x.FromAccountId)
            .NotEmpty()
            .WithMessage("La cuenta origen es obligatoria");

        RuleFor(x => x.ToAccountId)
            .NotEmpty()
            .WithMessage("La cuenta destino es obligatoria");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("El monto debe ser mayor a cero");
    }
}
