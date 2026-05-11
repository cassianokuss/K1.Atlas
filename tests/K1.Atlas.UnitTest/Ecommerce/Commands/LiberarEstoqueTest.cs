using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Commands;
using MediatR;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Commands;

public class LiberarEstoqueTest
{
    [Fact]
    public void Constructor_Should_Initialize_PedidoId_Property()
    {
        var command = new LiberarEstoque
        {
            PedidoId = "507f1f77bcf86cd799439011"
        };

        Assert.NotNull(command.PedidoId);
        Assert.Equal("507f1f77bcf86cd799439011", command.PedidoId);
    }

    [Fact]
    public void Command_Should_Implement_IRequest_Of_Bool()
    {
        var command = new LiberarEstoque();

        Assert.IsAssignableFrom<IRequest<bool>>(command);
    }

    [Fact]
    public void Command_Should_Have_PedidoId_Property_With_Default_Value()
    {
        var command = new LiberarEstoque();

        Assert.NotNull(command.PedidoId);
        Assert.Equal(string.Empty, command.PedidoId);
    }

    [Fact]
    public void PedidoId_Property_Should_Be_Settable()
    {
        var command = new LiberarEstoque();
        
        command.PedidoId = "test-pedido-123";

        Assert.Equal("test-pedido-123", command.PedidoId);
    }
}
