using MediatR;

namespace K1.Atlas.WorkerEstoque.Ecommerce.Commands;

public class ReservarEstoque : IRequest<ReservaEstoque>
{
    public Pedido Pedido { get; set; } = null!;
    public string PedidoId { get; set; } = string.Empty;
}
