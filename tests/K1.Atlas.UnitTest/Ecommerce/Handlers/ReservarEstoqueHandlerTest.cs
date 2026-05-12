using System.Diagnostics;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Ecommerce.WorkerEstoque;
using K1.Atlas.Ecommerce.WorkerEstoque.Features.ReservarEstoque;
using K1.Atlas.Ecommerce.WorkerEstoque.Exceptions;
using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Domain.Repositories;
using K1.Atlas.Domain.ResultPattern;
using K1.Atlas.PubSub.Producer;
using Moq;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce;

public class ReservarEstoqueHandlerTest
{
    private readonly Mock<IRepository<Produto>> _produtoRepository;
    private readonly Mock<IRepository<ReservaEstoque>> _reservaEstoqueRepository;
    private readonly Mock<IMessageProducer> _messageProducer;
    private readonly Mock<INotifier> _notifier;
    private readonly ReservarEstoqueHandler _handler;

    public ReservarEstoqueHandlerTest()
    {
        _produtoRepository = new Mock<IRepository<Produto>>();
        _reservaEstoqueRepository = new Mock<IRepository<ReservaEstoque>>();
        _messageProducer = new Mock<IMessageProducer>();
        _notifier = new Mock<INotifier>();
        
        _handler = new ReservarEstoqueHandler(
            _produtoRepository.Object,
            _reservaEstoqueRepository.Object,
            _messageProducer.Object,
            _notifier.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Create_Reservation_Successfully()
    {
        // Arrange
        var pedido = new Pedido 
        { 
            Id = "pedido123",
            NumeroPedido = "PED20260507000001",
            ClienteId = "cli123",
            Status = StatusPedido.Aprovado,
            Itens = new List<ItemPedido>
            {
                new ItemPedido 
                { 
                    ProdutoId = "prod1", 
                    Quantidade = 2,
                    CodigoProduto = "PROD001"
                },
                new ItemPedido 
                { 
                    ProdutoId = "prod2", 
                    Quantidade = 1,
                    CodigoProduto = "PROD002"
                }
            }
        };
        
        var produto1 = new Produto 
        { 
            Id = "prod1", 
            Codigo = "PROD001",
            Descricao = "Produto 1",
            EstoqueDisponivel = 10,
            Ativo = true
        };
        
        var produto2 = new Produto 
        { 
            Id = "prod2", 
            Codigo = "PROD002",
            Descricao = "Produto 2",
            EstoqueDisponivel = 5,
            Ativo = true
        };

        _produtoRepository.Setup(r => r.FirstOrDefaultAsync(
            It.Is<Func<IQueryable<Produto>, IQueryable<Produto>>>(f => 
                f.ToString().Contains("prod1") || true),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Func<IQueryable<Produto>, IQueryable<Produto>> filter, CancellationToken ct) =>
            {
                var produtos = new[] { produto1, produto2 }.AsQueryable();
                var filtered = filter(produtos);
                return filtered.FirstOrDefault();
            });

        _reservaEstoqueRepository.Setup(r => r.SaveOrUpdateAsync(
            It.IsAny<ReservaEstoque>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<ReservaEstoque, bool>>>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new ReservarEstoque
        {
            Pedido = pedido,
            PedidoId = pedido.Id
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.IsSuccess);
        Assert.Equal(pedido.Id, result.Value.PedidoId);
        Assert.Equal(pedido.ClienteId, result.Value.ClienteId);
        Assert.Equal(2, result.Value.Itens.Count);
        Assert.Equal(StatusReserva.Ativa, result.Value.Status);
        Assert.True(result.Value.DataExpiracao > DateTime.UtcNow.AddHours(23));

        _reservaEstoqueRepository.Verify(r => r.SaveOrUpdateAsync(
            It.IsAny<ReservaEstoque>(),
            It.IsAny<System.Linq.Expressions.Expression<Func<ReservaEstoque, bool>>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _messageProducer.Verify(m => m.Publish(
            It.IsAny<ReservaEstoque>(),
            It.IsAny<PublishOptions>()), Times.Once);

        _notifier.Verify(n => n.NotifyInformation(
            It.IsAny<string>(),
            It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_EstoqueInsuficienteException_When_Stock_Unavailable()
    {
        // Arrange
        var pedido = new Pedido 
        { 
            Id = "pedido123",
            NumeroPedido = "PED20260507000001",
            ClienteId = "cli123",
            Itens = new List<ItemPedido>
            {
                new ItemPedido 
                { 
                    ProdutoId = "prod1", 
                    Quantidade = 20,
                    CodigoProduto = "PROD001"
                }
            }
        };
        
        var produto = new Produto 
        { 
            Id = "prod1", 
            Codigo = "PROD001",
            Descricao = "Produto 1",
            EstoqueDisponivel = 5,
            Ativo = true
        };

        _produtoRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Produto>, IQueryable<Produto>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(produto);

        var command = new ReservarEstoque
        {
            Pedido = pedido,
            PedidoId = pedido.Id
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.ErrorType);
        Assert.Equal("ESTOQUE.INSUFICIENTE", result.Error.Code);
        Assert.Contains("PROD001", result.Error.Description);
        Assert.Contains("20", result.Error.Description);
        Assert.Contains("5", result.Error.Description);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_ProdutoNaoEncontradoException_When_Product_Not_Found()
    {
        // Arrange
        var pedido = new Pedido 
        { 
            Id = "pedido123",
            NumeroPedido = "PED20260507000001",
            ClienteId = "cli123",
            Itens = new List<ItemPedido>
            {
                new ItemPedido 
                { 
                    ProdutoId = "prod999", 
                    Quantidade = 1,
                    CodigoProduto = "PROD999"
                }
            }
        };

        _produtoRepository.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Func<IQueryable<Produto>, IQueryable<Produto>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Produto)null!);

        var command = new ReservarEstoque
        {
            Pedido = pedido,
            PedidoId = pedido.Id
        };

        // Act
        var result = await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.ErrorType);
        Assert.Equal("PRODUTO.NOT_FOUND", result.Error.Code);
        Assert.Contains("prod999", result.Error.Description);
    }
}
