using K1.Atlas.Ecommerce.Contracts.Entities;

namespace K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Features.ReservarEstoque.Domain;

public static class CriadorReserva
{
    public static ReservaEstoque Criar(Pedido pedido, List<(Produto produto, int quantidade)> itens)
    {
        var reserva = new ReservaEstoque
        {
            PedidoId = pedido.Id,
            ClienteId = pedido.ClienteId,
            Status = StatusReserva.Ativa,
            DataReserva = DateTime.UtcNow,
            DataExpiracao = DateTime.UtcNow.AddHours(24)
        };

        foreach (var (produto, quantidade) in itens)
        {
            reserva.Itens.Add(new ItemReservado
            {
                ProdutoId = produto.Id,
                Quantidade = quantidade,
                QuantidadeReservada = quantidade
            });
        }

        return reserva;
    }
}
