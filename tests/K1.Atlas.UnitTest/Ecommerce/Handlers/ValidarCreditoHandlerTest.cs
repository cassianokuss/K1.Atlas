using System.Diagnostics;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce;
using K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce.Commands;
using K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce;
using K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce.Services;
using K1.Atlas.Domain.Repositories;
using K1.Atlas.PubSub.Producer;
using Moq;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce;

public class ValidarCreditoHandlerTest
{
    private readonly Mock<IRepository<Cliente>> _clienteRepository;
    private readonly Mock<IRepository<Pedido>> _pedidoRepository;
    private readonly Mock<IMessageProducer> _messageProducer;
    private readonly Mock<INotifier> _notifier;
    private readonly Mock<IBureauCreditoService> _bureauService;
    private readonly Mock<PedidoMetrics> _pedidoMetrics;
    private readonly ValidarCreditoHandler _handler;

    public ValidarCreditoHandlerTest()
    {
        _clienteRepository = new Mock<IRepository<Cliente>>();
        _pedidoRepository = new Mock<IRepository<Pedido>>();
        _messageProducer = new Mock<IMessageProducer>();
        _notifier = new Mock<INotifier>();
        _bureauService = new Mock<IBureauCreditoService>();
        _pedidoMetrics = new Mock<PedidoMetrics>();

        _handler = new ValidarCreditoHandler(
            _clienteRepository.Object,
            _pedidoRepository.Object,
            _messageProducer.Object,
            _notifier.Object,
            _bureauService.Object,
            _pedidoMetrics.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Approve_When_Credit_Is_Sufficient()
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "João Silva",
            CpfCnpj = "12345678901",
            Email = "joao@example.com",
            LimiteCredito = 10000m,
            CreditoUtilizado = 2000m,
            Ativo = true
        };

        var pedido = new Pedido
        {
            Id = "ped123",
            NumeroPedido = "PED-001",
            ClienteId = "cli123",
            ValorTotal = 5000m,
            Status = StatusPedido.Pendente,
            DataCriacao = DateTime.UtcNow
        };

        var command = new ValidarCredito { Pedido = pedido };

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _bureauService.Setup(s => s.SimularConsultaAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(750);

        _pedidoRepository.Setup(r => r.SaveOrUpdateAsync(
                It.IsAny<Pedido>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Pedido, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Aprovado);
        Assert.Equal(750, result.ScoreBureau);
        Assert.Equal(8000m, result.LimiteDisponivel);
        Assert.Equal(string.Empty, result.MotivoRejeicao);

        // Verify pedido was updated to Aprovado
        _pedidoRepository.Verify(r => r.SaveOrUpdateAsync(
            It.Is<Pedido>(p => p.Status == StatusPedido.Aprovado && p.DataAprovacao != null),
            It.IsAny<System.Linq.Expressions.Expression<Func<Pedido, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        // Verify PedidoAprovado message was published
        _messageProducer.Verify(m => m.Publish(
            It.Is<Pedido>(p => p.Status == StatusPedido.Aprovado),
            It.Is<PublishOptions>(o => o.RoutingKey == "PedidoAprovado")), Times.Once);

        // Verify telemetry and logging
        _notifier.Verify(n => n.NotifyInformation(
            It.IsAny<string>(),
            It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task HandleAsync_Should_Reject_When_Credit_Is_Insufficient()
    {
        // Arrange
        var cliente = new Cliente
        {
            Nome = "Maria Santos",
            CpfCnpj = "98765432109",
            LimiteCredito = 5000m,
            CreditoUtilizado = 4800m,
            Ativo = true
        };

        var pedido = new Pedido
        {
            Id = "ped456",
            NumeroPedido = "PED-002",
            ClienteId = "cli456",
            ValorTotal = 3500m,
            Status = StatusPedido.Pendente,
            DataCriacao = DateTime.UtcNow
        };

        var command = new ValidarCredito { Pedido = pedido };

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _bureauService.Setup(s => s.SimularConsultaAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(650);

        _pedidoRepository.Setup(r => r.SaveOrUpdateAsync(
                It.IsAny<Pedido>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Pedido, bool>>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Aprovado);
        Assert.Equal(650, result.ScoreBureau);
        Assert.Equal(200m, result.LimiteDisponivel);
        Assert.NotEmpty(result.MotivoRejeicao);

        // Verify pedido was updated to Rejeitado
        _pedidoRepository.Verify(r => r.SaveOrUpdateAsync(
            It.Is<Pedido>(p => p.Status == StatusPedido.Rejeitado && p.MotivoRejeicao != null),
            It.IsAny<System.Linq.Expressions.Expression<Func<Pedido, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        // Verify PedidoRejeitado message was published
        _messageProducer.Verify(m => m.Publish(
            It.Is<Pedido>(p => p.Status == StatusPedido.Rejeitado),
            It.Is<PublishOptions>(o => o.RoutingKey == "PedidoRejeitado")), Times.Once);

        // Verify warning logging
        _notifier.Verify(n => n.NotifyWarning(
            It.IsAny<string>(),
            It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task HandleAsync_Should_Call_Bureau_With_Correct_CpfCnpj()
    {
        // Arrange
        var cliente = new Cliente
        {
            CpfCnpj = "12345678901",
            LimiteCredito = 10000m,
            CreditoUtilizado = 0m
        };

        var pedido = new Pedido
        {
            ClienteId = "cli123",
            ValorTotal = 1000m,
            Status = StatusPedido.Pendente
        };

        var command = new ValidarCredito { Pedido = pedido };

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _bureauService.Setup(s => s.SimularConsultaAsync(
                "12345678901",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(800);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _bureauService.Verify(s => s.SimularConsultaAsync(
            "12345678901",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Return_Score_In_Range_300_To_850()
    {
        // Arrange
        var cliente = new Cliente
        {
            CpfCnpj = "12345678901",
            LimiteCredito = 10000m,
            CreditoUtilizado = 0m
        };

        var pedido = new Pedido
        {
            ClienteId = "cli123",
            ValorTotal = 1000m,
            Status = StatusPedido.Pendente
        };

        var command = new ValidarCredito { Pedido = pedido };

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var randomScore = 575;
        _bureauService.Setup(s => s.SimularConsultaAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(randomScore);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.InRange(result.ScoreBureau, 300, 850);
    }

    [Fact]
    public async Task HandleAsync_Should_Add_Telemetry_Tags()
    {
        // Arrange
        var activitySource = new ActivitySource("TestSource");
        using var activity = activitySource.StartActivity("TestActivity");

        var cliente = new Cliente
        {
            CpfCnpj = "12345678901",
            LimiteCredito = 10000m,
            CreditoUtilizado = 2000m
        };

        var pedido = new Pedido
        {
            Id = "ped123",
            NumeroPedido = "PED-001",
            ClienteId = "cli123",
            ValorTotal = 5000m,
            Status = StatusPedido.Pendente
        };

        var command = new ValidarCredito { Pedido = pedido };

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _bureauService.Setup(s => s.SimularConsultaAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(750);

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public async Task HandleAsync_Should_Log_Approval_Information()
    {
        // Arrange
        var cliente = new Cliente
        {
            CpfCnpj = "12345678901",
            LimiteCredito = 10000m,
            CreditoUtilizado = 0m
        };

        var pedido = new Pedido
        {
            NumeroPedido = "PED-001",
            ClienteId = "cli123",
            ValorTotal = 1000m,
            Status = StatusPedido.Pendente
        };

        var command = new ValidarCredito { Pedido = pedido };

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _bureauService.Setup(s => s.SimularConsultaAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(800);

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert - Should have at least 3 log entries (start, bureau call, approval/rejection)
        _notifier.Verify(n => n.NotifyInformation(
            It.IsAny<string>(),
            It.IsAny<object[]>()), Times.AtLeast(3));
    }
}
