using K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce;
using K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Commands;
using K1.Atlas.PubSub.Consumer;
using MediatR;
using Moq;
using System.Diagnostics;
using K1.Atlas.Telemetry.Logging;

namespace K1.Atlas.UnitTest.Ecommerce.Subscriptions;

public class EstoqueReservadoSubscriptionTest
{
    private readonly Mock<ISender> _mockSender;
    private readonly Mock<IMessageContext> _mockContext;
    private readonly Mock<INotifier> _mockNotifier;
    private readonly EstoqueReservadoSubscription _subscription;

    public EstoqueReservadoSubscriptionTest()
    {
        _mockSender = new Mock<ISender>();
        _mockContext = new Mock<IMessageContext>();
        _mockNotifier = new Mock<INotifier>();
        _subscription = new EstoqueReservadoSubscription(_mockSender.Object, _mockNotifier.Object);
    }

    [Fact]
    public async Task ConsumeAsync_WithValidReservaEstoque_ShouldSendEmitirNotaFiscalCommand()
    {
        // Arrange
        var reserva = new ReservaEstoque
        {
            Id = "507f1f77bcf86cd799439011",
            PedidoId = "607f1f77bcf86cd799439012",
            ClienteId = "707f1f77bcf86cd799439013",
            Status = StatusReserva.Ativa,
            Itens = new List<ItemReservado>
            {
                new ItemReservado { ProdutoId = "prod1", Quantidade = 2, QuantidadeReservada = 2 },
                new ItemReservado { ProdutoId = "prod2", Quantidade = 1, QuantidadeReservada = 1 }
            }
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<EmitirNotaFiscal>(), cancellationToken))
            .ReturnsAsync(new NotaFiscal());

        // Act
        await _subscription.ConsumeAsync(reserva, _mockContext.Object, cancellationToken);

        // Assert
        _mockSender.Verify(
            s => s.SendAsync(
                It.Is<EmitirNotaFiscal>(cmd => 
                    cmd.PedidoId == reserva.PedidoId && 
                    cmd.ReservaId == reserva.Id),
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task ConsumeAsync_AfterProcessing_ShouldAcknowledgeMessage()
    {
        // Arrange
        var reserva = new ReservaEstoque
        {
            Id = "507f1f77bcf86cd799439011",
            PedidoId = "607f1f77bcf86cd799439012",
            ClienteId = "707f1f77bcf86cd799439013"
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<EmitirNotaFiscal>(), cancellationToken))
            .ReturnsAsync(new NotaFiscal());

        // Act
        await _subscription.ConsumeAsync(reserva, _mockContext.Object, cancellationToken);

        // Assert
        _mockContext.Verify(c => c.AckAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ConsumeAsync_WithTelemetryActive_ShouldSetRequiredTags()
    {
        // Arrange
        using var activity = new Activity("TestActivity").Start();
        
        var reserva = new ReservaEstoque
        {
            Id = "507f1f77bcf86cd799439011",
            PedidoId = "607f1f77bcf86cd799439012",
            ClienteId = "707f1f77bcf86cd799439013",
            Itens = new List<ItemReservado>
            {
                new ItemReservado { ProdutoId = "prod1", Quantidade = 2, QuantidadeReservada = 2 },
                new ItemReservado { ProdutoId = "prod2", Quantidade = 3, QuantidadeReservada = 3 }
            }
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<EmitirNotaFiscal>(), cancellationToken))
            .ReturnsAsync(new NotaFiscal());

        // Act
        await _subscription.ConsumeAsync(reserva, _mockContext.Object, cancellationToken);

        // Assert - Must have at least 3 tags: PedidoId, ReservaId, Action
        var tags = activity.Tags.ToList();
        Assert.Contains(tags, t => t.Key == "PedidoId" && t.Value == reserva.PedidoId);
        Assert.Contains(tags, t => t.Key == "ReservaId" && t.Value == reserva.Id);
        Assert.Contains(tags, t => t.Key == "Action" && t.Value == "EmitirNotaFiscal");
    }

    [Fact]
    public async Task ConsumeAsync_ShouldLogInformationMessages()
    {
        // Arrange
        var reserva = new ReservaEstoque
        {
            Id = "507f1f77bcf86cd799439011",
            PedidoId = "607f1f77bcf86cd799439012",
            ClienteId = "707f1f77bcf86cd799439013"
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<EmitirNotaFiscal>(), cancellationToken))
            .ReturnsAsync(new NotaFiscal());

        // Act
        await _subscription.ConsumeAsync(reserva, _mockContext.Object, cancellationToken);

        // Assert - Should log at least 2 information messages (start and end)
        _mockNotifier.Verify(
            n => n.NotifyInformation(It.IsAny<string>(), It.IsAny<object[]>()),
            Times.AtLeast(2));
    }

    [Fact]
    public async Task ConsumeAsync_WhenMediatorThrows_ShouldLogErrorAndAcknowledge()
    {
        // Arrange
        var reserva = new ReservaEstoque
        {
            Id = "507f1f77bcf86cd799439011",
            PedidoId = "607f1f77bcf86cd799439012",
            ClienteId = "707f1f77bcf86cd799439013"
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<EmitirNotaFiscal>(), cancellationToken))
            .ThrowsAsync(new Exception("Fiscal service unavailable"));

        // Act
        await _subscription.ConsumeAsync(reserva, _mockContext.Object, cancellationToken);

        // Assert - Should log error
        _mockNotifier.Verify(
            n => n.NotifyError(It.IsAny<string>(), It.IsAny<object[]>()),
            Times.Once);

        // Assert - Should still acknowledge to prevent infinite requeue
        _mockContext.Verify(c => c.AckAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ConsumeAsync_WhenMediatorThrows_ShouldSetErrorStatusOnActivity()
    {
        // Arrange
        using var activity = new Activity("TestActivity").Start();
        
        var reserva = new ReservaEstoque
        {
            Id = "507f1f77bcf86cd799439011",
            PedidoId = "607f1f77bcf86cd799439012",
            ClienteId = "707f1f77bcf86cd799439013"
        };

        var cancellationToken = CancellationToken.None;
        var errorMessage = "Fiscal service unavailable";

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<EmitirNotaFiscal>(), cancellationToken))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        await _subscription.ConsumeAsync(reserva, _mockContext.Object, cancellationToken);

        // Assert
        Assert.Equal(ActivityStatusCode.Error, activity.Status);
        Assert.Equal(errorMessage, activity.StatusDescription);
    }
}
