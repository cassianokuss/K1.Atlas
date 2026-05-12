using FluentValidation;

namespace K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Features.ReservarEstoque;

public class ReservarEstoqueValidator : AbstractValidator<ReservarEstoque>
{
    public ReservarEstoqueValidator()
    {
        RuleFor(x => x.Pedido)
            .NotNull()
            .WithMessage("Pedido é obrigatório");

        RuleFor(x => x.PedidoId)
            .NotEmpty()
            .WithMessage("PedidoId é obrigatório");

        RuleFor(x => x.Pedido.Itens)
            .NotEmpty()
            .When(x => x.Pedido != null)
            .WithMessage("Pedido deve ter pelo menos um item");
    }
}
