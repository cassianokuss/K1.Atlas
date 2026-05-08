using K1.Atlas.TesteApi.Ecommerce;
using K1.Atlas.TesteApi.Ecommerce.Commands;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Commands;

public class CriarPedidoTest
{
    [Fact]
    public void Constructor_Should_Initialize_Properties()
    {
        var command = new CriarPedido
        {
            ClienteId = "cli123",
            Itens = new List<ItemPedidoRequest>
            {
                new ItemPedidoRequest { ProdutoId = "prod1", Quantidade = 2 }
            }
        };

        Assert.Equal("cli123", command.ClienteId);
        Assert.Single(command.Itens);
        Assert.Equal("prod1", command.Itens[0].ProdutoId);
        Assert.Equal(2, command.Itens[0].Quantidade);
    }

    [Fact]
    public void ItemPedidoRequest_Should_Validate_Positive_Quantity()
    {
        var item = new ItemPedidoRequest
        {
            ProdutoId = "prod1",
            Quantidade = 5
        };

        Assert.True(item.Quantidade > 0);
    }
}
