using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Features.ReservarEstoque;
using K1.Atlas.Ecommerce.Contracts.Entities;
using MediatR;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Commands;

public class ReservarEstoqueTest
{
    [Fact]
    public void Constructor_Should_Initialize_Pedido_Property()
    {
        var pedido = new Pedido
        {
            Id = "507f1f77bcf86cd799439011",
            ClienteId = "cli123",
            NumeroPedido = "PED-001"
        };

        var command = new ReservarEstoque
        {
            Pedido = pedido
        };

        Assert.NotNull(command.Pedido);
        Assert.Equal("507f1f77bcf86cd799439011", command.Pedido.Id);
        Assert.Equal("cli123", command.Pedido.ClienteId);
    }

    [Fact]
    public void Command_Should_Implement_IRequest_Of_ReservaEstoque()
    {
        var command = new ReservarEstoque();

        Assert.IsAssignableFrom<IRequest<ReservaEstoque>>(command);
    }

    [Fact]
    public void Command_Should_Have_Pedido_Property()
    {
        var command = new ReservarEstoque();
        var pedido = new Pedido { NumeroPedido = "PED-002" };
        
        command.Pedido = pedido;

        Assert.Equal("PED-002", command.Pedido.NumeroPedido);
    }
}
