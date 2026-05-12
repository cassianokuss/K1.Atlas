using FluentValidation;

namespace K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Features.EmitirNotaFiscal;

public class EmitirNotaFiscalValidator : AbstractValidator<EmitirNotaFiscal>
{
    public EmitirNotaFiscalValidator()
    {
        RuleFor(x => x.PedidoId)
            .NotEmpty()
            .WithMessage("PedidoId é obrigatório");

        RuleFor(x => x.ReservaId)
            .NotEmpty()
            .WithMessage("ReservaId é obrigatório");
    }
}
