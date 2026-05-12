using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Features.ReservarEstoque.Domain;
using K1.Atlas.Ecommerce.Contracts.Entities;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Domain;

public class CriadorReservaTest
{
    [Fact]
    public void Criar_ComPedidoValido_DeveCriarReservaComStatusAtiva()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "PED123",
            ClienteId = "CLI456",
            Itens = new List<ItemPedido>()
        };
        var produto = new Produto
        {
            Id = "PROD1",
            Codigo = "COD1",
            Descricao = "Produto 1",
            ValorUnitario = 100m,
            EstoqueDisponivel = 10
        };
        var itens = new List<(Produto produto, int quantidade)>
        {
            (produto, 2)
        };

        // Act
        var reserva = CriadorReserva.Criar(pedido, itens);

        // Assert
        Assert.NotNull(reserva);
        Assert.NotEmpty(reserva.Id);
        Assert.Equal(pedido.Id, reserva.PedidoId);
        Assert.Equal(pedido.ClienteId, reserva.ClienteId);
        Assert.Equal(StatusReserva.Ativa, reserva.Status);
    }

    [Fact]
    public void Criar_DeveCriarItensReservadosParaCadaItem()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "PED123",
            ClienteId = "CLI456",
            Itens = new List<ItemPedido>()
        };
        var produto1 = new Produto { Id = "PROD1", Codigo = "COD1", Descricao = "Produto 1", ValorUnitario = 100m };
        var produto2 = new Produto { Id = "PROD2", Codigo = "COD2", Descricao = "Produto 2", ValorUnitario = 50m };
        var itens = new List<(Produto produto, int quantidade)>
        {
            (produto1, 2),
            (produto2, 3)
        };

        // Act
        var reserva = CriadorReserva.Criar(pedido, itens);

        // Assert
        Assert.Equal(2, reserva.Itens.Count);
        Assert.Equal("PROD1", reserva.Itens[0].ProdutoId);
        Assert.Equal(2, reserva.Itens[0].Quantidade);
        Assert.Equal(2, reserva.Itens[0].QuantidadeReservada);
        Assert.Equal("PROD2", reserva.Itens[1].ProdutoId);
        Assert.Equal(3, reserva.Itens[1].Quantidade);
    }

    [Fact]
    public void Criar_DeveDefinirDataReservaEExpiracao()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "PED123",
            ClienteId = "CLI456",
            Itens = new List<ItemPedido>()
        };
        var produto = new Produto { Id = "PROD1", Codigo = "COD1", Descricao = "Produto 1", ValorUnitario = 100m };
        var itens = new List<(Produto produto, int quantidade)> { (produto, 1) };
        var agora = DateTime.UtcNow;

        // Act
        var reserva = CriadorReserva.Criar(pedido, itens);

        // Assert
        Assert.True(reserva.DataReserva >= agora);
        Assert.True(reserva.DataReserva <= DateTime.UtcNow);
        Assert.True(reserva.DataExpiracao > reserva.DataReserva);
        Assert.True(reserva.DataExpiracao <= DateTime.UtcNow.AddHours(24).AddSeconds(1));
    }

    [Fact]
    public void Criar_ComListaItensVazia_DeveCriarReservaComListaVazia()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "PED123",
            ClienteId = "CLI456",
            Itens = new List<ItemPedido>()
        };
        var itens = new List<(Produto produto, int quantidade)>();

        // Act
        var reserva = CriadorReserva.Criar(pedido, itens);

        // Assert
        Assert.Empty(reserva.Itens);
        Assert.Equal(StatusReserva.Ativa, reserva.Status);
    }
}
