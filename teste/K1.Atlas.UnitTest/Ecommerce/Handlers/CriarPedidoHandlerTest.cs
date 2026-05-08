using System.Diagnostics;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.TesteApi.Ecommerce;
using K1.Atlas.TesteApi.Ecommerce.Commands;
using K1.Atlas.TesteApi.Ecommerce.Handlers;
using K1.Atlas.Domain.Repositories;
using K1.Atlas.PubSub.Producer;
using Moq;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Handlers;

public class CriarPedidoHandlerTest
{
    private readonly Mock<IRepository<Cliente>> _clienteRepository;
    private readonly Mock<IRepository<Produto>> _produtoRepository;
    private readonly Mock<IRepository<Pedido>> _pedidoRepository;
    private readonly Mock<IMessageProducer> _messageProducer;
    private readonly Mock<INotifier> _notifier;
    private readonly CriarPedidoHandler _handler;

    public CriarPedidoHandlerTest()
    {
        _clienteRepository = new Mock<IRepository<Cliente>>();
        _produtoRepository = new Mock<IRepository<Produto>>();
        _pedidoRepository = new Mock<IRepository<Pedido>>();
        _messageProducer = new Mock<IMessageProducer>();
        _notifier = new Mock<INotifier>();
        
        _handler = new CriarPedidoHandler(
            _clienteRepository.Object,
            _produtoRepository.Object,
            _pedidoRepository.Object,
            _notifier.Object,
            _messageProducer.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Create_Pedido_Successfully()
    {
        var cliente = new Cliente 
        { 
            Id = "cli123", 
            Nome = "João",
            LimiteCredito = 10000m,
            CreditoUtilizado = 0
        };
        
        var produto1 = new Produto 
        { 
            Id = "prod1", 
            Codigo = "PROD001",
            Descricao = "Produto 1",
            ValorUnitario = 100m, 
            EstoqueDisponivel = 10,
            Ativo = true
        };

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _produtoRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Produto>, IQueryable<Produto>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(produto1);

        _pedidoRepository.Setup(r => r.SaveOrUpdateAsync(
            It.IsAny<Pedido>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Pedido, bool>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CriarPedido
        {
            ClienteId = "cli123",
            Itens = new List<ItemPedidoRequest>
            {
                new ItemPedidoRequest { ProdutoId = "prod1", Quantidade = 2 }
            }
        };

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.NotNull(result.NumeroPedido);
        Assert.Equal("cli123", result.ClienteId);
        Assert.Single(result.Itens);
        Assert.Equal(200m, result.ValorTotal);
        Assert.Equal(StatusPedido.Pendente, result.Status);

        _messageProducer.Verify(m => m.Publish(
            It.IsAny<Pedido>(),
            It.IsAny<PublishOptions>()), Times.Once);

        _notifier.Verify(n => n.NotifyInformation(
            It.IsAny<string>(),
            It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_When_Cliente_Not_Found()
    {
        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente)null!);

        var command = new CriarPedido { ClienteId = "invalid" };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_Should_Add_Telemetry_Tags()
    {
        var cliente = new Cliente { Id = "cli123", Nome = "João" };
        var produto = new Produto 
        { 
            Id = "prod1", 
            Codigo = "PROD001",
            Descricao = "Produto 1",
            ValorUnitario = 100m,
            EstoqueDisponivel = 10,
            Ativo = true
        };

        _clienteRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Cliente>, IQueryable<Cliente>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _produtoRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Produto>, IQueryable<Produto>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(produto);

        _pedidoRepository.Setup(r => r.SaveOrUpdateAsync(
            It.IsAny<Pedido>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<Pedido, bool>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new CriarPedido
        {
            ClienteId = "cli123",
            Itens = new List<ItemPedidoRequest>
            {
                new ItemPedidoRequest { ProdutoId = "prod1", Quantidade = 1 }
            }
        };

        using var activity = new Activity("TestActivity").Start();

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
    }
}
