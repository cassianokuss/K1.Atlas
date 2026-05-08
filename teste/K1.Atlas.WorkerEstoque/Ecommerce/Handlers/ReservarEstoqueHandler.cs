using System.Diagnostics;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.WorkerEstoque.Ecommerce.Commands;
using K1.Atlas.WorkerEstoque.Ecommerce.Exceptions;
using K1.Atlas.Domain.Repositories;
using K1.Atlas.PubSub.Producer;
using MediatR;

namespace K1.Atlas.WorkerEstoque.Ecommerce.Handlers;

public class ReservarEstoqueHandler : IRequestHandler<ReservarEstoque, ReservaEstoque>
{
    private readonly IRepository<Produto> _produtoRepository;
    private readonly IRepository<ReservaEstoque> _reservaEstoqueRepository;
    private readonly IMessageProducer _messageProducer;
    private readonly INotifier _notifier;

    public ReservarEstoqueHandler(
        IRepository<Produto> produtoRepository,
        IRepository<ReservaEstoque> reservaEstoqueRepository,
        IMessageProducer messageProducer,
        INotifier notifier)
    {
        _produtoRepository = produtoRepository;
        _reservaEstoqueRepository = reservaEstoqueRepository;
        _messageProducer = messageProducer;
        _notifier = notifier;
    }

    public async Task<ReservaEstoque> HandleAsync(ReservarEstoque request, CancellationToken cancellationToken = default)
    {
        var pedido = request.Pedido;
        
        _notifier.NotifyInformation(
            "Iniciando reserva de estoque. {PedidoId} {NumeroPedido} {TotalItens}",
            pedido.Id,
            pedido.NumeroPedido,
            pedido.Itens.Count);

        // Step 1 & 2: Load and validate products
        var reserva = new ReservaEstoque
        {
            PedidoId = pedido.Id,
            ClienteId = pedido.ClienteId,
            Status = StatusReserva.Ativa,
            DataReserva = DateTime.UtcNow,
            DataExpiracao = DateTime.UtcNow.AddHours(24)
        };

        foreach (var item in pedido.Itens)
        {
            // Load produto
            var produto = await _produtoRepository.FirstOrDefaultAsync(
                builder => builder.Where(p => p.Id == item.ProdutoId),
                cancellationToken);

            if (produto == null)
            {
                _notifier.NotifyError(
                    "Produto não encontrado. {ProdutoId} {PedidoId}",
                    item.ProdutoId,
                    pedido.Id);
                
                throw new ProdutoNaoEncontradoException(item.ProdutoId);
            }

            // Validate stock availability
            if (!produto.TemEstoque(item.Quantidade))
            {
                _notifier.NotifyError(
                    "Estoque insuficiente. {ProdutoCodigo} {ProdutoId} {QuantidadeRequerida} {QuantidadeDisponivel} {PedidoId}",
                    produto.Codigo,
                    item.ProdutoId,
                    item.Quantidade,
                    produto.EstoqueDisponivel,
                    pedido.Id);
                
                throw new EstoqueInsuficienteException(
                    produto.Codigo,
                    item.Quantidade,
                    produto.EstoqueDisponivel);
            }

            // Add to reservation
            reserva.Itens.Add(new ItemReservado
            {
                ProdutoId = produto.Id,
                Quantidade = item.Quantidade,
                QuantidadeReservada = item.Quantidade
            });
        }

        // Step 4: Persist reservation to MongoDB
        await _reservaEstoqueRepository.SaveOrUpdateAsync(
            reserva,
            r => r.Id == reserva.Id,
            cancellationToken);

        // Step 5: Publish "EstoqueReservado" message to RabbitMQ
        await _messageProducer.Publish(
            reserva,
            PublishOptions.RoutingTo("EstoqueReservado").ToExchange("Pedidos"));

        // Step 6: Add OpenTelemetry tags (6+ tags as per spec)

        // Step 7: Structured logging (success)
        _notifier.NotifyInformation(
            "Estoque reservado com sucesso. {PedidoId} {NumeroPedido} {ReservaId} {TotalItens} {DataExpiracao}",
            pedido.Id,
            pedido.NumeroPedido,
            reserva.Id,
            reserva.Itens.Count,
            reserva.DataExpiracao);

        return reserva;
    }
}
