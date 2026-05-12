using K1.Atlas.Ecommerce.Api.Ecommerce;
using K1.Atlas.Ecommerce.Contracts.Entities;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce;

public class PedidoTest
{
    [Fact]
    public void Constructor_Should_Initialize_Properties()
    {
        var cliente = new Cliente { Id = "cli123", Nome = "João" };
        var pedido = new Pedido
        {
            NumeroPedido = "PED001",
            ClienteId = cliente.Id,
            Cliente = cliente,
            Status = StatusPedido.Pendente,
            DataCriacao = DateTime.Now
        };

        Assert.Equal("PED001", pedido.NumeroPedido);
        Assert.Equal("cli123", pedido.ClienteId);
        Assert.Equal(StatusPedido.Pendente, pedido.Status);
        Assert.NotNull(pedido.Itens);
        Assert.Empty(pedido.Itens);
    }

    [Fact]
    public void CalcularTotais_Should_Sum_Itens_And_Frete()
    {
        var pedido = new Pedido
        {
            Itens = new List<ItemPedido>
            {
                new ItemPedido { Quantidade = 2, ValorUnitario = 100m, Subtotal = 200m },
                new ItemPedido { Quantidade = 1, ValorUnitario = 50m, Subtotal = 50m }
            },
            ValorFrete = 20m
        };

        pedido.CalcularTotais();

        Assert.Equal(250m, pedido.ValorProdutos);
        Assert.Equal(270m, pedido.ValorTotal);
    }

    [Fact]
    public void ItemPedido_Should_Calculate_Subtotal()
    {
        var item = new ItemPedido
        {
            Quantidade = 3,
            ValorUnitario = 100m
        };

        item.CalcularSubtotal();

        Assert.Equal(300m, item.Subtotal);
    }
}
