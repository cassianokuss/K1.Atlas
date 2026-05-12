using FluentValidation;

namespace K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce.Features.ValidarCredito;

public class ValidarCreditoValidator : AbstractValidator<ValidarCredito>
{
    public ValidarCreditoValidator()
    {
        RuleFor(x => x.Pedido)
            .NotNull()
            .WithMessage("Pedido é obrigatório");

        RuleFor(x => x.Pedido.ClienteId)
            .NotEmpty()
            .When(x => x.Pedido != null)
            .WithMessage("ClienteId é obrigatório");

        RuleFor(x => x.Pedido.ValorTotal)
            .GreaterThan(0)
            .When(x => x.Pedido != null)
            .WithMessage("ValorTotal deve ser maior que zero");
    }
}
