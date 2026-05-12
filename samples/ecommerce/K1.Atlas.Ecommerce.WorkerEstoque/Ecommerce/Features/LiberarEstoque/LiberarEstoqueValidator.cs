using FluentValidation;

namespace K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Features.LiberarEstoque;

public class LiberarEstoqueValidator : AbstractValidator<LiberarEstoque>
{
    public LiberarEstoqueValidator()
    {
        RuleFor(x => x.PedidoId)
            .NotEmpty()
            .WithMessage("PedidoId é obrigatório");
    }
}
