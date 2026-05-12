using K1.Atlas.Ecommerce.WorkerValidacao;
using K1.Atlas.Ecommerce.WorkerValidacao.Features.ValidarCredito;
using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Ecommerce.Contracts.ValueObjects;
using K1.Atlas.PubSub.Consumer;
using MediatR;
using Moq;
using System.Diagnostics;
using K1.Atlas.Telemetry.Logging;

namespace K1.Atlas.UnitTest.Ecommerce.Subscriptions;

public class PedidoCriadoSubscriptionTest
{
    private readonly Mock<ISender> _mockSender;
    private readonly Mock<IMessageContext> _mockContext;
    private readonly Mock<INotifier> _mockNotifier;
    private readonly PedidoCriadoSubscription _subscription;

    public PedidoCriadoSubscriptionTest()
    {
        _mockSender = new Mock<ISender>();
        _mockContext = new Mock<IMessageContext>();
        _mockNotifier = new Mock<INotifier>();
        _subscription = new PedidoCriadoSubscription(_mockSender.Object, _mockNotifier.Object);
    }

    [Fact]
    public async Task ConsumeAsync_WithValidPedido_ShouldSendValidarCredito()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "507f1f77bcf86cd799439011",
            NumeroPedido = "PED-001",
            ClienteId = "607f1f77bcf86cd799439012",
            Status = StatusPedido.Pendente,
            ValorTotal = 1500.00m
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<ValidarCredito>(), cancellationToken))
            .ReturnsAsync(new ResultadoValidacao());

        // Act
        await _subscription.ConsumeAsync(pedido, _mockContext.Object, cancellationToken);

        // Assert
        _mockSender.Verify(
            s => s.SendAsync(
                It.Is<ValidarCredito>(cmd => cmd.Pedido == pedido),
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
            NumeroPedido = "PED-002",
            ClienteId = "607f1f77bcf86cd799439012"
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<ValidarCredito>(), cancellationToken))
            .ReturnsAsync(new ResultadoValidacao());

        // Act
        await _subscription.ConsumeAsync(pedido, _mockContext.Object, cancellationToken);

        // Assert
        _mockContext.Verify(c => c.AckAsync(cancellationToken), Times.Once);
    }

    [Fact]
    public async Task ConsumeAsync_ShouldLogInformationAtStart()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "507f1f77bcf86cd799439011",
            NumeroPedido = "PED-004",
            ClienteId = "607f1f77bcf86cd799439012",
            ValorTotal = 3000.00m
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<ValidarCredito>(), cancellationToken))
            .ReturnsAsync(new ResultadoValidacao());

        // Act
        await _subscription.ConsumeAsync(pedido, _mockContext.Object, cancellationToken);

        // Assert - Should log at start of processing
        _mockNotifier.Verify(
            n => n.NotifyInformation(
                It.Is<string>(msg => msg.Contains("Iniciando validação de crédito")),
                It.IsAny<object[]>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ConsumeAsync_ShouldLogInformationAtEnd()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "507f1f77bcf86cd799439011",
            NumeroPedido = "PED-005",
            ClienteId = "607f1f77bcf86cd799439012"
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<ValidarCredito>(), cancellationToken))
            .ReturnsAsync(new ResultadoValidacao());

        // Act
        await _subscription.ConsumeAsync(pedido, _mockContext.Object, cancellationToken);

        // Assert - Should log at end of processing
        _mockNotifier.Verify(
            n => n.NotifyInformation(
                It.Is<string>(msg => msg.Contains("Validação de crédito concluída")),
                It.IsAny<object[]>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task ConsumeAsync_WhenMediatorThrows_ShouldLogError()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "507f1f77bcf86cd799439011",
            NumeroPedido = "PED-006",
            ClienteId = "607f1f77bcf86cd799439012"
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<ValidarCredito>(), cancellationToken))
            .ThrowsAsync(new Exception("Bureau service error"));

        // Act
        await _subscription.ConsumeAsync(pedido, _mockContext.Object, cancellationToken);

        // Assert - Should log error
        _mockNotifier.Verify(
            n => n.NotifyError(
                It.Is<string>(msg => msg.Contains("Erro ao processar validação de crédito")),
                It.IsAny<object[]>()),
            Times.Once);
    }

    [Fact]
    public async Task ConsumeAsync_WhenMediatorThrows_ShouldAcknowledgeToAvoidRequeue()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "507f1f77bcf86cd799439011",
            NumeroPedido = "PED-007",
            ClienteId = "607f1f77bcf86cd799439012"
        };

        var cancellationToken = CancellationToken.None;

        _mockSender
            .Setup(s => s.SendAsync(It.IsAny<ValidarCredito>(), cancellationToken))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        await _subscription.ConsumeAsync(pedido, _mockContext.Object, cancellationToken);

        // Assert - Should still acknowledge to prevent infinite requeue
        _mockContext.Verify(c => c.AckAsync(cancellationToken), Times.Once);
    }
}
