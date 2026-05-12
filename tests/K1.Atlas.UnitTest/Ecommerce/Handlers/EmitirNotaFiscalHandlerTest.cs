using System.Diagnostics;
using K1.Atlas.Domain.ResultPattern;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Ecommerce.WorkerFiscal.Features.EmitirNotaFiscal;
using K1.Atlas.Ecommerce.WorkerFiscal.Features.EmitirNotaFiscal.Infrastructure;
using K1.Atlas.Ecommerce.WorkerFiscal;
using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Domain.Repositories;
using K1.Atlas.PubSub.Producer;
using Moq;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce;

public class EmitirNotaFiscalHandlerTest
{
    private readonly Mock<IRepository<Pedido>> _pedidoRepository;
    private readonly Mock<IRepository<Cliente>> _clienteRepository;
    private readonly Mock<IRepository<NotaFiscal>> _notaFiscalRepository;
    private readonly Mock<IMessageProducer> _messageProducer;
    private readonly Mock<ISefazRetryPolicy> _sefazRetryPolicy;
    private readonly Mock<INotifier> _notifier;
    private readonly Mock<NotaFiscalMetrics> _notaFiscalMetrics;
    private readonly EmitirNotaFiscalHandler _handler;

    public EmitirNotaFiscalHandlerTest()
    {
        _pedidoRepository = new Mock<IRepository<Pedido>>();
        _clienteRepository = new Mock<IRepository<Cliente>>();
        _notaFiscalRepository = new Mock<IRepository<NotaFiscal>>();
        _messageProducer = new Mock<IMessageProducer>();
        _sefazRetryPolicy = new Mock<ISefazRetryPolicy>();
        _notifier = new Mock<INotifier>();
        _notaFiscalMetrics = new Mock<NotaFiscalMetrics>();
        
        _handler = new EmitirNotaFiscalHandler(
            _pedidoRepository.Object,
            _clienteRepository.Object,
            _notaFiscalRepository.Object,
            _messageProducer.Object,
            _sefazRetryPolicy.Object,
            _notifier.Object,
            _notaFiscalMetrics.Object);
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
        _sefazRetryPolicy.Setup(s => s.ExecutarComRetryAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SefazResult("PROT123456789", 1));

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
        Assert.Equal(180m, result.Value.ValorICMS);
        Assert.Equal(16.5m, result.Value.ValorPIS);
        Assert.Equal(76m, result.Value.ValorCOFINS);
        Assert.Equal(100m, result.Value.ValorIPI);
        
        // ValorTotal should include all taxes
        var expectedTotal = 1000m + 180m + 16.5m + 76m + 100m;
        Assert.Equal(expectedTotal, result.Value.ValorTotal);
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
        _sefazRetryPolicy.Setup(s => s.ExecutarComRetryAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(Error.Failure("SEFAZ.MaxRetries", "Max retries reached"));

        var command = new EmitirNotaFiscal
        {
            PedidoId = pedido.Id,
            ReservaId = "reserva123"
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(StatusNotaFiscal.Rejeitada, result.Value.Status);
        Assert.Equal(3, result.Value.TentativasEnvio);
        Assert.Null(result.Value.ProtocoloAutorizacao);

        _sefazRetryPolicy.Verify(s => s.ExecutarComRetryAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<CancellationToken>()), Times.Once);
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

        _sefazRetryPolicy.Setup(s => s.ExecutarComRetryAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SefazResult("PROT123456789", 1));

        var command = new EmitirNotaFiscal
        {
            PedidoId = pedido.Id,
            ReservaId = "reserva123"
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result.Value.ChaveAcesso);
        Assert.Equal(44, result.Value.ChaveAcesso.Length);
        Assert.All(result.Value.ChaveAcesso, c => Assert.True(char.IsDigit(c), "ChaveAcesso should contain only digits"));
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

        _sefazRetryPolicy.Setup(s => s.ExecutarComRetryAsync(
            It.IsAny<NotaFiscal>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SefazResult("PROT123456789", 1));

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
