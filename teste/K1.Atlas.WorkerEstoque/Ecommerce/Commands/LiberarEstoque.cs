using MediatR;

namespace K1.Atlas.WorkerEstoque.Ecommerce.Commands;

public class LiberarEstoque : IRequest<bool>
{
    public string PedidoId { get; set; } = string.Empty;
}
