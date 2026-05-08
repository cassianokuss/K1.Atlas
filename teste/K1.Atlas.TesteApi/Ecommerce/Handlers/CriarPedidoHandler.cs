using System.Diagnostics;
using K1.Atlas.Telemetry;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.TesteApi.Ecommerce.Commands;
using K1.Atlas.Domain.Repositories;
using K1.Atlas.PubSub.Producer;
using MediatR;

namespace K1.Atlas.TesteApi.Ecommerce.Handlers;

public class CriarPedidoHandler : IRequestHandler<CriarPedido, Pedido>
{
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Produto> _produtoRepository;
    private readonly IRepository<Pedido> _pedidoRepository;
    private readonly INotifier _notifier;
    private readonly IMessageProducer _messageProducer;
    private static int _numeroPedidoSequence = 0;
    private const string ServiceName = "testeapi";

    public CriarPedidoHandler(
        IRepository<Cliente> clienteRepository,
        IRepository<Produto> produtoRepository,
        IRepository<Pedido> pedidoRepository,
        INotifier notifier,
        IMessageProducer messageProducer)
    {
        _clienteRepository = clienteRepository;
        _produtoRepository = produtoRepository;
        _pedidoRepository = pedidoRepository;
        _notifier = notifier;
        _messageProducer = messageProducer;
    }

    public async Task<Pedido> HandleAsync(CriarPedido request, CancellationToken cancellationToken = default)
    {
        var cliente = await _clienteRepository.FirstOrDefaultAsync(
            builder => builder.Where(c => c.Id == request.ClienteId || c.CpfCnpj == request.ClienteId),
            cancellationToken);

        if (cliente == null)
        {
            throw new InvalidOperationException($"Cliente {request.ClienteId} não encontrado");
        }

        var pedido = new Pedido
        {
            NumeroPedido = GerarNumeroPedido(),
            ClienteId = cliente.Id,
            Cliente = cliente,
            Status = StatusPedido.Pendente,
            DataCriacao = DateTime.Now,
            Itens = new List<ItemPedido>()
        };

        foreach (var itemRequest in request.Itens)
        {
            var produto = await _produtoRepository.FirstOrDefaultAsync(
                builder => builder.Where(p => p.Id == itemRequest.ProdutoId),
                cancellationToken);

            if (produto == null)
            {
                throw new InvalidOperationException($"Produto {itemRequest.ProdutoId} não encontrado");
            }

            if (!produto.Ativo)
            {
                throw new InvalidOperationException($"Produto {produto.Codigo} não está ativo");
            }

            if (!produto.TemEstoque(itemRequest.Quantidade))
            {
                throw new InvalidOperationException(
                    $"Estoque insuficiente para produto {produto.Codigo}. Disponível: {produto.EstoqueDisponivel}, Solicitado: {itemRequest.Quantidade}");
            }

            var item = new ItemPedido
            {
                ProdutoId = produto.Id,
                CodigoProduto = produto.Codigo,
                DescricaoProduto = produto.Descricao,
                Quantidade = itemRequest.Quantidade,
                ValorUnitario = produto.ValorUnitario
            };
            
            item.CalcularSubtotal();
            pedido.Itens.Add(item);
        }

        pedido.CalcularTotais();

        await _pedidoRepository.SaveOrUpdateAsync(
            pedido, 
            p => p.NumeroPedido == pedido.NumeroPedido,
            cancellationToken);

        await _messageProducer.Publish(
            pedido,
            PublishOptions.RoutingTo("PedidoCriado").ToExchange("Pedidos"));

        _notifier.NotifyInformation(
            "Pedido criado com sucesso. {NumeroPedido} {ClienteId} {ClienteNome} {ValorTotal} {QuantidadeItens}",
            pedido.NumeroPedido,
            pedido.ClienteId,
            pedido.Cliente.Nome,
            pedido.ValorTotal,
            pedido.Itens.Count);

        EcommerceMetrics.IncrementPedidoCriado(ServiceName, pedido.ClienteId);

        return pedido;
    }

    private static string GerarNumeroPedido()
    {
        var numero = Interlocked.Increment(ref _numeroPedidoSequence);
        return $"PED{DateTime.Now:yyyyMMdd}{numero:D6}";
    }
}
