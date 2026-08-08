using FluentValidation;
using FluyoV2.Features.Liabilities.Dtos;

namespace FluyoV2.Validators;

public class CreateLiabilityRequestValidator
    : AbstractValidator<CreateLiabilityRequest>
{
    public CreateLiabilityRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("El nombre de la deuda es obligatorio")
            .MaximumLength(100);

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0)
            .WithMessage("El monto total de la deuda debe ser mayor a cero");

        // When IsStillPaying is true, payment details should be provided
        When(x => x.IsStillPaying, () =>
        {
            RuleFor(x => x.PaymentFrequency)
                .NotEmpty()
                .WithMessage("La frecuencia de pago es obligatoria cuando aún se está pagando");

            RuleFor(x => x.InstallmentAmount)
                .GreaterThan(0)
                .WithMessage("El valor de la cuota debe ser mayor a cero cuando aún se está pagando");

            RuleFor(x => x.RemainingInstallments)
                .GreaterThan(0)
                .WithMessage("Las cuotas restantes deben ser mayor a cero cuando aún se está pagando");
        });

        // NextPaymentDate is optional; when provided ensure it's a valid date
        When(x => x.NextPaymentDate.HasValue, () =>
        {
            RuleFor(x => x.NextPaymentDate.Value)
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("La fecha de próximo pago debe ser hoy o una fecha futura");
        });
    }
}
