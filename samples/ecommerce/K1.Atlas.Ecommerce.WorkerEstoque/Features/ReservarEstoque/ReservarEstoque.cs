using K1.Atlas.Domain.Repositories;
using K1.Atlas.Domain.ResultPattern;
using K1.Atlas.PubSub.Producer;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Ecommerce.WorkerEstoque.Features.ReservarEstoque.Domain;
using MediatR;

namespace K1.Atlas.Ecommerce.WorkerEstoque.Features.ReservarEstoque;

public class ReservarEstoque : IRequest<ResultT<ReservaEstoque>>
{
    public Pedido Pedido { get; set; } = null!;
    public string PedidoId { get; set; } = string.Empty;
}

public class ReservarEstoqueHandler : IRequestHandler<ReservarEstoque, ResultT<ReservaEstoque>>
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

    public async Task<ResultT<ReservaEstoque>> HandleAsync(ReservarEstoque request, CancellationToken cancellationToken = default)
    {
        var pedido = request.Pedido;
        
        _notifier.NotifyInformation(
            "Iniciando reserva de estoque. {PedidoId} {NumeroPedido} {TotalItens}",
            pedido.Id, pedido.NumeroPedido, pedido.Itens.Count);

        var itensValidados = new List<(Produto produto, int quantidade)>();

        foreach (var item in pedido.Itens)
        {
            var produto = await _produtoRepository.FirstOrDefaultAsync(
                builder => builder.Where(p => p.Id == item.ProdutoId),
                cancellationToken);

            var produtoResult = ValidadorEstoque.ValidarProdutoExiste(produto, item.ProdutoId);
            if (!produtoResult.IsSuccess)
                return produtoResult.Error!;

            var disponibilidadeResult = ValidadorEstoque.ValidarDisponibilidade(produtoResult.Value, item.Quantidade);
            if (!disponibilidadeResult.IsSuccess)
                return disponibilidadeResult.Error!;

            itensValidados.Add((produtoResult.Value, item.Quantidade));
            
            _notifier.NotifyInformation(
                "Produto validado. {ProdutoCodigo} {QuantidadeRequerida} {QuantidadeDisponivel}",
                produtoResult.Value.Codigo, item.Quantidade, produtoResult.Value.EstoqueDisponivel);
        }

        var reserva = CriadorReserva.Criar(pedido, itensValidados);

        await _reservaEstoqueRepository.SaveOrUpdateAsync(
            reserva,
            r => r.Id == reserva.Id,
            cancellationToken);

        await _messageProducer.Publish(
            reserva,
            PublishOptions.RoutingTo("EstoqueReservado").ToExchange("Pedidos"));

        _notifier.NotifyInformation(
            "Estoque reservado com sucesso. {PedidoId} {NumeroPedido} {ReservaId} {TotalItens} {DataExpiracao}",
            pedido.Id, pedido.NumeroPedido, reserva.Id, reserva.Itens.Count, reserva.DataExpiracao);

        return reserva;
    }
}
