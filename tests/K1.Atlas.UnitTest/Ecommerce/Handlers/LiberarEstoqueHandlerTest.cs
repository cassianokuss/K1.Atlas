using System.Diagnostics;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce;
using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Commands;
using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce;
using K1.Atlas.Domain.Repositories;
using Moq;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce;

public class LiberarEstoqueHandlerTest
{
    private readonly Mock<IRepository<ReservaEstoque>> _reservaEstoqueRepository;
    private readonly Mock<INotifier> _notifier;
    private readonly LiberarEstoqueHandler _handler;

    public LiberarEstoqueHandlerTest()
    {
        _reservaEstoqueRepository = new Mock<IRepository<ReservaEstoque>>();
        _notifier = new Mock<INotifier>();
        
        _handler = new LiberarEstoqueHandler(
            _reservaEstoqueRepository.Object,
            _notifier.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Release_Stock_Successfully()
    {
        // Arrange
        var pedidoId = "pedido123";
        var reserva = new ReservaEstoque
        {
            Id = "reserva123",
            PedidoId = pedidoId,
            ClienteId = "cli123",
            Status = StatusReserva.Ativa,
            DataReserva = DateTime.UtcNow.AddHours(-1),
            Itens = new List<ItemReservado>
            {
                new ItemReservado 
                { 
                    ProdutoId = "prod1", 
                    Quantidade = 2,
                    QuantidadeReservada = 2
                },
                new ItemReservado 
                { 
                    ProdutoId = "prod2", 
                    Quantidade = 1,
                    QuantidadeReservada = 1
                }
            }
        };

        _reservaEstoqueRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<ReservaEstoque>, IQueryable<ReservaEstoque>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(reserva);

        _reservaEstoqueRepository.Setup(r => r.SaveOrUpdateAsync(
            It.IsAny<ReservaEstoque>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<ReservaEstoque, bool>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new LiberarEstoque
        {
            PedidoId = pedidoId
        };

        using var activity = new Activity("TestActivity").Start();

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result);

        _reservaEstoqueRepository.Verify(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<ReservaEstoque>, IQueryable<ReservaEstoque>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _reservaEstoqueRepository.Verify(r => r.SaveOrUpdateAsync(
            It.Is<ReservaEstoque>(re => re.Status == StatusReserva.Liberada),
            It.IsAny<System.Linq.Expressions.Expression<Func<ReservaEstoque, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _notifier.Verify(n => n.NotifyInformation(
            It.IsAny<string>(),
            It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_False_When_Reservation_Not_Found()
    {
        // Arrange
        var pedidoId = "pedido999";

        _reservaEstoqueRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<ReservaEstoque>, IQueryable<ReservaEstoque>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((ReservaEstoque)null!);

        var command = new LiberarEstoque
        {
            PedidoId = pedidoId
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result);

        _reservaEstoqueRepository.Verify(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<ReservaEstoque>, IQueryable<ReservaEstoque>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _reservaEstoqueRepository.Verify(r => r.SaveOrUpdateAsync(
            It.IsAny<ReservaEstoque>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<ReservaEstoque, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);

        _notifier.Verify(n => n.NotifyWarning(
            It.IsAny<string>(),
            It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Be_Idempotent_Already_Released()
    {
        // Arrange
        var pedidoId = "pedido123";
        var reserva = new ReservaEstoque
        {
            Id = "reserva123",
            PedidoId = pedidoId,
            ClienteId = "cli123",
            Status = StatusReserva.Liberada, // Already released
            DataReserva = DateTime.UtcNow.AddHours(-2),
            Itens = new List<ItemReservado>
            {
                new ItemReservado 
                { 
                    ProdutoId = "prod1", 
                    Quantidade = 2,
                    QuantidadeReservada = 2
                }
            }
        };

        _reservaEstoqueRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<ReservaEstoque>, IQueryable<ReservaEstoque>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(reserva);

        var command = new LiberarEstoque
        {
            PedidoId = pedidoId
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result); // Idempotent - returns true even if already released

        _reservaEstoqueRepository.Verify(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<ReservaEstoque>, IQueryable<ReservaEstoque>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        // Should not update if already released
        _reservaEstoqueRepository.Verify(r => r.SaveOrUpdateAsync(
            It.IsAny<ReservaEstoque>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<ReservaEstoque, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Never);

        _notifier.Verify(n => n.NotifyInformation(
            It.IsAny<string>(),
            It.IsAny<object[]>()), Times.Once);
    }
}
