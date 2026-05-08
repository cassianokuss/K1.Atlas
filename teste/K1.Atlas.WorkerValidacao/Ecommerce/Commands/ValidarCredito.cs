using MediatR;

namespace K1.Atlas.WorkerValidacao.Ecommerce.Commands;

public class ValidarCredito : IRequest<ResultadoValidacao>
{
    public Pedido Pedido { get; set; } = null!;
}
