using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce;
using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Commands;
using K1.Atlas.PubSub.Consumer;
using MediatR;
using Moq;
using System.Diagnostics;
using K1.Atlas.Telemetry.Logging;

namespace K1.Atlas.UnitTest.Ecommerce.Subscriptions;

public class ReservarEstoqueSubscriptionTest
{
    private readonly Mock<ISender> _mockSender;
    private readonly Mock<IMessageContext> _mockContext;
    private readonly Mock<INotifier> _mockNotifier;
    private readonly ReservarEstoqueSubscription _subscription;

    public ReservarEstoqueSubscriptionTest()
    {
        _mockSender = new Mock<ISender>();
        _mockContext = new Mock<IMessageContext>();
        _mockNotifier = new Mock<INotifier>();
        _subscription = new ReservarEstoqueSubscription(_mockSender.Object, _mockNotifier.Object);
    }

    [Fact]
    public async Task ConsumeAsync_WithValidPedido_ShouldSendReservarEstoqueCommand()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "507f1f77bcf86cd799439011",
            NumeroPedido = "PED-001",
            ClienteId = "607f1f77bcf86cd799439012",
            Status = StatusPedido.Aprovado,
            Itens = new List<ItemPedido>
            {
                new ItemPedido { ProdutoId = "prod1", Quantidade = 2 },
                new ItemPedido { ProdutoId = "prod2", Quantidade = 1 }
            }
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<ReservarEstoque>(), cancellationToken))
            .ReturnsAsync(new ReservaEstoque());

        // Act
        await _subscription.ConsumeAsync(pedido, _mockContext.Object, cancellationToken);

        // Assert
        _mockSender.Verify(
            s => s.SendAsync(
                It.Is<ReservarEstoque>(cmd => cmd.Pedido == pedido && cmd.PedidoId == pedido.Id),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ConsumeAsync_AfterProcessing_ShouldAcknowledgeMessage()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "507f1f77bcf86cd799439011",
            NumeroPedido = "PED-002"
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<ReservarEstoque>(), cancellationToken))
            .ReturnsAsync(new ReservaEstoque());

        // Act
        await _subscription.ConsumeAsync(pedido, _mockContext.Object, cancellationToken);

        // Assert
        _mockContext.Verify(c => c.AckAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ConsumeAsync_WhenMediatorThrows_ShouldAcknowledgeToAvoidRequeue()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "507f1f77bcf86cd799439011",
            NumeroPedido = "PED-004"
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<ReservarEstoque>(), cancellationToken))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        await _subscription.ConsumeAsync(pedido, _mockContext.Object, cancellationToken);

        // Assert - Should still acknowledge to prevent infinite requeue
        _mockContext.Verify(c => c.AckAsync(cancellationToken), Times.Once);
    }
}
