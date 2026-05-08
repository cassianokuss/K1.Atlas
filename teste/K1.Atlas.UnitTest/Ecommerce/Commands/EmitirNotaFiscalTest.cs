using K1.Atlas.WorkerFiscal.Ecommerce;
using K1.Atlas.WorkerFiscal.Ecommerce.Commands;
using MediatR;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Commands;

public class EmitirNotaFiscalTest
{
    [Fact]
    public void Command_Should_Have_PedidoId_Property()
    {
        var command = new EmitirNotaFiscal
        {
            PedidoId = "507f1f77bcf86cd799439011"
        };

        Assert.Equal("507f1f77bcf86cd799439011", command.PedidoId);
    }

    [Fact]
    public void Command_Should_Have_ReservaId_Property()
    {
        var command = new EmitirNotaFiscal
        {
            ReservaId = "607f1f77bcf86cd799439012"
        };

        Assert.Equal("607f1f77bcf86cd799439012", command.ReservaId);
    }

    [Fact]
    public void Command_Should_Implement_IRequest_Of_NotaFiscal()
    {
        var command = new EmitirNotaFiscal();

        Assert.IsAssignableFrom<IRequest<NotaFiscal>>(command);
    }

    [Fact]
    public void Command_Should_Initialize_Properties_Correctly()
    {
        var command = new EmitirNotaFiscal
        {
            PedidoId = "pedido123",
            ReservaId = "reserva456"
        };

        Assert.Equal("pedido123", command.PedidoId);
        Assert.Equal("reserva456", command.ReservaId);
    }
}
