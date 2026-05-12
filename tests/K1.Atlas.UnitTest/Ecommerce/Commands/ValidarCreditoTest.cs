using K1.Atlas.Ecommerce.WorkerValidacao.Features.ValidarCredito;
using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Ecommerce.Contracts.ValueObjects;
using K1.Atlas.Domain.ResultPattern;
using MediatR;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Commands;

public class ValidarCreditoTest
{
    [Fact]
    public void Constructor_Should_Initialize_Pedido_Property()
    {
        var pedido = new Pedido
        {
            Id = "507f1f77bcf86cd799439011",
            ClienteId = "cli123",
            NumeroPedido = "PED-001",
            ValorTotal = 1000.00m
        };

        var command = new ValidarCredito
        {
            Pedido = pedido
        };

        Assert.NotNull(command.Pedido);
        Assert.Equal("507f1f77bcf86cd799439011", command.Pedido.Id);
        Assert.Equal("cli123", command.Pedido.ClienteId);
        Assert.Equal(1000.00m, command.Pedido.ValorTotal);
    }

    [Fact]
    public void Command_Should_Implement_IRequest_Of_ResultT_ResultadoValidacao()
    {
        var command = new ValidarCredito();

        Assert.IsAssignableFrom<IRequest<ResultT<ResultadoValidacao>>>(command);
    }

    [Fact]
    public void Command_Should_Have_Pedido_Property()
    {
        var command = new ValidarCredito();
        var pedido = new Pedido { NumeroPedido = "PED-002" };
        
        command.Pedido = pedido;

        Assert.Equal("PED-002", command.Pedido.NumeroPedido);
    }
}
