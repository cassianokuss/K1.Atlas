using MediatR;

namespace K1.Atlas.TesteApi.Ecommerce.Commands;

public class CriarPedido : IRequest<Pedido>
{
    public string ClienteId { get; set; } = default!;
    public List<ItemPedidoRequest> Itens { get; set; } = new();
}

public class ItemPedidoRequest
{
    public string ProdutoId { get; set; } = default!;
    public int Quantidade { get; set; }
}
