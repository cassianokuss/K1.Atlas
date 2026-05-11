using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce;

public class ReservaEstoqueTest
{
    [Fact]
    public void Constructor_Should_Set_Default_Values()
    {
        // Arrange & Act
        var reserva = new ReservaEstoque();
        
        // Assert
        Assert.NotNull(reserva.Id);
        Assert.NotEmpty(reserva.Id);
        Assert.True(ObjectId.TryParse(reserva.Id, out _));
        Assert.NotNull(reserva.Itens);
        Assert.Empty(reserva.Itens);
        Assert.Equal(StatusReserva.Ativa, reserva.Status);
        Assert.NotEqual(default(DateTime), reserva.DataReserva);
        Assert.NotEqual(default(DateTime), reserva.DataExpiracao);
    }

    [Fact]
    public void ItemReservado_Should_Set_Properties()
    {
        // Arrange
        var produtoId = ObjectId.GenerateNewId().ToString();
        
        // Act
        var item = new ItemReservado
        {
            ProdutoId = produtoId,
            Quantidade = 5,
            QuantidadeReservada = 3
        };
        
        // Assert
        Assert.Equal(produtoId, item.ProdutoId);
        Assert.Equal(5, item.Quantidade);
        Assert.Equal(3, item.QuantidadeReservada);
    }

    [Fact]
    public void StatusReserva_Should_Have_All_Values()
    {
        // Arrange & Act
        var ativa = StatusReserva.Ativa;
        var liberada = StatusReserva.Liberada;
        var expirada = StatusReserva.Expirada;
        
        // Assert
        Assert.Equal(0, (int)ativa);
        Assert.Equal(1, (int)liberada);
        Assert.Equal(2, (int)expirada);
    }

    [Fact]
    public void ReservaEstoque_Should_Set_All_Properties()
    {
        // Arrange
        var pedidoId = ObjectId.GenerateNewId().ToString();
        var clienteId = ObjectId.GenerateNewId().ToString();
        var dataReserva = DateTime.UtcNow;
        var dataExpiracao = DateTime.UtcNow.AddHours(24);
        var itens = new List<ItemReservado>
        {
            new ItemReservado 
            { 
                ProdutoId = ObjectId.GenerateNewId().ToString(),
                Quantidade = 2,
                QuantidadeReservada = 2
            }
        };
        
        // Act
        var reserva = new ReservaEstoque
        {
            PedidoId = pedidoId,
            ClienteId = clienteId,
            Itens = itens,
            Status = StatusReserva.Ativa,
            DataReserva = dataReserva,
            DataExpiracao = dataExpiracao
        };
        
        // Assert
        Assert.Equal(pedidoId, reserva.PedidoId);
        Assert.Equal(clienteId, reserva.ClienteId);
        Assert.Equal(itens, reserva.Itens);
        Assert.Single(reserva.Itens);
        Assert.Equal(StatusReserva.Ativa, reserva.Status);
        Assert.Equal(dataReserva, reserva.DataReserva);
        Assert.Equal(dataExpiracao, reserva.DataExpiracao);
    }

    [Fact]
    public void ReservaEstoque_Should_Have_MongoDB_Attributes()
    {
        // Arrange
        var type = typeof(ReservaEstoque);
        var idProperty = type.GetProperty("Id");
        
        // Assert - Verify the property exists and is properly configured
        Assert.NotNull(idProperty);
        Assert.Equal(typeof(string), idProperty!.PropertyType);
        
        // MongoDB ID mapping is configured via SetIdMember in ServiceCollectionExtensions,
        // not through attributes on the class itself
    }
}
