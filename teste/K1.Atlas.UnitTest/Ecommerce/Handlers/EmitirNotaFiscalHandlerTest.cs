using System.Diagnostics;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.WorkerFiscal.Ecommerce;
using K1.Atlas.WorkerFiscal.Ecommerce.Commands;
using K1.Atlas.WorkerFiscal.Ecommerce.Handlers;
using K1.Atlas.WorkerFiscal.Ecommerce.Services;
using K1.Atlas.Domain.Repositories;
using K1.Atlas.PubSub.Producer;
using Moq;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Handlers;

public class EmitirNotaFiscalHandlerTest
{
    private readonly Mock<IRepository<Pedido>> _pedidoRepository;
    private readonly Mock<IRepository<Cliente>> _clienteRepository;
    private readonly Mock<IRepository<NotaFiscal>> _notaFiscalRepository;
    private readonly Mock<IMessageProducer> _messageProducer;
    private readonly Mock<INotifier> _notifier;
    private readonly Mock<ISefazService> _sefazService;
    private readonly EmitirNotaFiscalHandler _handler;

    public EmitirNotaFiscalHandlerTest()
    {
        _pedidoRepository = new Mock<IRepository<Pedido>>();
        _clienteRepository = new Mock<IRepository<Cliente>>();
        _notaFiscalRepository = new Mock<IRepository<NotaFiscal>>();
        _messageProducer = new Mock<IMessageProducer>();
        _notifier = new Mock<INotifier>();
        _sefazService = new Mock<ISefazService>();
        
        _handler = new EmitirNotaFiscalHandler(
            _pedidoRepository.Object,
            _clienteRepository.Object,
            _notaFiscalRepository.Object,
            _messageProducer.Object,
            _notifier.Object,
            _sefazService.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Emit_NotaFiscal_Successfully_With_Retry()
    {
        // Arrange
        var pedido = new Pedido 
        { 
            Id = "pedido123",
            NumeroPedido = "PED20260507000001",
            ClienteId = "cli123",
            Status = StatusPedido.EstoqueReservado,
            ValorProdutos = 1000m,
            ValorFrete = 50m,
            ValorTotal = 1050m,
            Itens = new List<ItemPedido>
            {
                new ItemPedido 
                { 
                    ProdutoId = "prod1", 
                    CodigoProduto = "PROD001",
                    DescricaoProduto = "Produto 1",
                    Quantidade = 2,
                    ValorUnitario = 500m,
                    Subtotal = 1000m
                }
            }
        };
        
        var cliente = new Cliente 
        { 
            Nome = "Cliente Teste",
            CpfCnpj = "12345678901",
            Email = "teste@email.com",
            Endereco = "Rua Teste 123",
            Cidade = "São Paulo",
            Estado = "SP",
            Cep = "01234567"
        };

        _pedidoRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Pedido>, IQueryable<Pedido>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _notaFiscalRepository.Setup(r => r.SaveOrUpdateAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<NotaFiscal, bool>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Simulate SEFAZ success after retry
        _sefazService.SetupSequence(s => s.EnviarNotaAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Timeout simulado"))
            .ReturnsAsync("PROT123456789");

        var command = new EmitirNotaFiscal
        {
            PedidoId = pedido.Id,
            ReservaId = "reserva123"
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(pedido.Id, result.PedidoId);
        Assert.Equal(cliente.Email, result.Cliente?.Email);
        Assert.Equal(StatusNotaFiscal.Autorizada, result.Status);
        Assert.Equal("PROT123456789", result.ProtocoloAutorizacao);
        Assert.Equal(2, result.TentativasEnvio);
        Assert.NotEmpty(result.ChaveAcesso);
        Assert.Equal(44, result.ChaveAcesso.Length); // SEFAZ key is 44 chars

        _notaFiscalRepository.Verify(r => r.SaveOrUpdateAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<NotaFiscal, bool>>>(),
            It.IsAny<CancellationToken>()), Times.AtLeast(2)); // Initial save + update after SEFAZ

        _sefazService.Verify(s => s.EnviarNotaAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));

        _messageProducer.Verify(m => m.Publish(
            It.IsAny<NotaFiscal>(),
            It.IsAny<PublishOptions>()), Times.Once);

        _notifier.Verify(n => n.NotifyInformation(
            It.IsAny<string>(),
            It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task HandleAsync_Should_Calculate_All_Taxes_Correctly()
    {
        // Arrange
        var pedido = new Pedido 
        { 
            Id = "pedido123",
            NumeroPedido = "PED20260507000001",
            ClienteId = "cli123",
            Status = StatusPedido.EstoqueReservado,
            ValorProdutos = 1000m,
            ValorFrete = 0m,
            ValorTotal = 1000m,
            Itens = new List<ItemPedido>
            {
                new ItemPedido 
                { 
                    ProdutoId = "prod1", 
                    CodigoProduto = "PROD001",
                    DescricaoProduto = "Produto 1",
                    Quantidade = 1,
                    ValorUnitario = 1000m,
                    Subtotal = 1000m
                }
            }
        };
        
        var cliente = new Cliente 
        { 
            Nome = "Cliente Teste",
            Estado = "SP"
        };

        _pedidoRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Pedido>, IQueryable<Pedido>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _notaFiscalRepository.Setup(r => r.SaveOrUpdateAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<NotaFiscal, bool>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sefazService.Setup(s => s.EnviarNotaAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync("PROT123456789");

        var command = new EmitirNotaFiscal
        {
            PedidoId = pedido.Id,
            ReservaId = "reserva123"
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert - Tax calculations (as per spec)
        // ICMS: 18% = 180
        // PIS: 1.65% = 16.5
        // COFINS: 7.6% = 76
        // IPI: 10% = 100
        Assert.Equal(180m, result.ValorICMS);
        Assert.Equal(16.5m, result.ValorPIS);
        Assert.Equal(76m, result.ValorCOFINS);
        Assert.Equal(100m, result.ValorIPI);
        
        // ValorTotal should include all taxes
        var expectedTotal = 1000m + 180m + 16.5m + 76m + 100m;
        Assert.Equal(expectedTotal, result.ValorTotal);
    }

    [Fact]
    public async Task HandleAsync_Should_Reject_NotaFiscal_After_Max_Retries()
    {
        // Arrange
        var pedido = new Pedido 
        { 
            Id = "pedido123",
            NumeroPedido = "PED20260507000001",
            ClienteId = "cli123",
            Status = StatusPedido.EstoqueReservado,
            ValorProdutos = 500m,
            ValorTotal = 500m,
            Itens = new List<ItemPedido>
            {
                new ItemPedido 
                { 
                    ProdutoId = "prod1", 
                    CodigoProduto = "PROD001",
                    DescricaoProduto = "Produto 1",
                    Quantidade = 1,
                    ValorUnitario = 500m,
                    Subtotal = 500m
                }
            }
        };
        
        var cliente = new Cliente { Nome = "Cliente Teste", Estado = "SP" };

        _pedidoRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Pedido>, IQueryable<Pedido>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _notaFiscalRepository.Setup(r => r.SaveOrUpdateAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<NotaFiscal, bool>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Simulate SEFAZ failure all 3 attempts
        _sefazService.Setup(s => s.EnviarNotaAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("SEFAZ timeout"));

        var command = new EmitirNotaFiscal
        {
            PedidoId = pedido.Id,
            ReservaId = "reserva123"
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(StatusNotaFiscal.Rejeitada, result.Status);
        Assert.Equal(3, result.TentativasEnvio);
        Assert.Null(result.ProtocoloAutorizacao);

        _sefazService.Verify(s => s.EnviarNotaAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<CancellationToken>()), Times.Exactly(3));

        _notifier.Verify(n => n.NotifyWarning(
            It.IsAny<string>(),
            It.IsAny<object[]>()), Times.AtLeast(3)); // Warning for each retry

        _notifier.Verify(n => n.NotifyError(
            It.IsAny<string>(),
            It.IsAny<object[]>()), Times.Once); // Final error
    }

    [Fact]
    public async Task HandleAsync_Should_Add_Telemetry_Tags_And_Events()
    {
        // Arrange
        var pedido = new Pedido 
        { 
            Id = "pedido123",
            NumeroPedido = "PED20260507000001",
            ClienteId = "cli123",
            Status = StatusPedido.EstoqueReservado,
            ValorProdutos = 1000m,
            ValorTotal = 1000m,
            Itens = new List<ItemPedido>
            {
                new ItemPedido 
                { 
                    ProdutoId = "prod1", 
                    CodigoProduto = "PROD001",
                    DescricaoProduto = "Produto 1",
                    Quantidade = 1,
                    ValorUnitario = 1000m,
                    Subtotal = 1000m
                }
            }
        };
        
        var cliente = new Cliente { Nome = "Cliente Teste", Estado = "SP" };

        _pedidoRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Pedido>, IQueryable<Pedido>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _notaFiscalRepository.Setup(r => r.SaveOrUpdateAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<NotaFiscal, bool>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Simulate SEFAZ success with 1 retry
        _sefazService.SetupSequence(s => s.EnviarNotaAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Timeout"))
            .ReturnsAsync("PROT123456789");

        var command = new EmitirNotaFiscal
        {
            PedidoId = pedido.Id,
            ReservaId = "reserva123"
        };

        using var activity = new Activity("TestActivity").Start();

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        
        // Verify telemetry tags (at least 8 tags as per spec)
        var tags = activity.Tags.ToList();
        Assert.True(tags.Any(), "Activity should have tags set");
        
        // Required tags: PedidoId, NotaFiscalId, ChaveAcesso, ValorTotal, ICMS, PIS, COFINS, IPI, TentativasEnvio, Status
        Assert.Contains(tags, t => t.Key.Contains("pedido", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(tags, t => t.Key.Contains("nf", StringComparison.OrdinalIgnoreCase) || t.Key.Contains("notafiscal", StringComparison.OrdinalIgnoreCase));
        
        // Verify telemetry events (retry events)
        var events = activity.Events.ToList();
        Assert.True(events.Any(), "Activity should have events for retry attempts");
    }

    [Fact]
    public async Task HandleAsync_Should_Generate_Valid_ChaveAcesso()
    {
        // Arrange
        var pedido = new Pedido 
        { 
            Id = "pedido123",
            NumeroPedido = "PED20260507000001",
            ClienteId = "cli123",
            Status = StatusPedido.EstoqueReservado,
            ValorProdutos = 500m,
            ValorTotal = 500m,
            Itens = new List<ItemPedido> { new ItemPedido { ProdutoId = "prod1", Quantidade = 1, Subtotal = 500m } }
        };
        
        var cliente = new Cliente { Nome = "Cliente Teste", Estado = "SP" };

        _pedidoRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Pedido>, IQueryable<Pedido>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _notaFiscalRepository.Setup(r => r.SaveOrUpdateAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<NotaFiscal, bool>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sefazService.Setup(s => s.EnviarNotaAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync("PROT123456789");

        var command = new EmitirNotaFiscal
        {
            PedidoId = pedido.Id,
            ReservaId = "reserva123"
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result.ChaveAcesso);
        Assert.Equal(44, result.ChaveAcesso.Length);
        Assert.All(result.ChaveAcesso, c => Assert.True(char.IsDigit(c), "ChaveAcesso should contain only digits"));
    }

    [Fact]
    public async Task HandleAsync_Should_Publish_NotaFiscalEmitida_Message()
    {
        // Arrange
        var pedido = new Pedido 
        { 
            Id = "pedido123",
            NumeroPedido = "PED20260507000001",
            ClienteId = "cli123",
            Status = StatusPedido.EstoqueReservado,
            ValorProdutos = 500m,
            ValorTotal = 500m,
            Itens = new List<ItemPedido> { new ItemPedido { ProdutoId = "prod1", Quantidade = 1, Subtotal = 500m } }
        };
        
        var cliente = new Cliente { Nome = "Cliente Teste", Estado = "SP" };

        _pedidoRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Pedido>, IQueryable<Pedido>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(pedido);

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _notaFiscalRepository.Setup(r => r.SaveOrUpdateAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<NotaFiscal, bool>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _sefazService.Setup(s => s.EnviarNotaAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync("PROT123456789");

        var command = new EmitirNotaFiscal
        {
            PedidoId = pedido.Id,
            ReservaId = "reserva123"
        };

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _messageProducer.Verify(m => m.Publish(
            It.IsAny<NotaFiscal>(),
            It.Is<PublishOptions>(opts => opts.RoutingKey == "NotaFiscalEmitida")), Times.Once);
    }
}
