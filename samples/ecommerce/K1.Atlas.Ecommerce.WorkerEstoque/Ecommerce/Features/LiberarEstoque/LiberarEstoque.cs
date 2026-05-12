using K1.Atlas.Domain.Repositories;
using K1.Atlas.Domain.ResultPattern;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Ecommerce.Contracts.Entities;
using MediatR;

namespace K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Features.LiberarEstoque;

public class LiberarEstoque : IRequest<Result>
{
    public string PedidoId { get; set; } = string.Empty;
}

public class LiberarEstoqueHandler : IRequestHandler<LiberarEstoque, Result>
{
    private readonly IRepository<ReservaEstoque> _reservaEstoqueRepository;
    private readonly INotifier _notifier;

    public LiberarEstoqueHandler(
        IRepository<ReservaEstoque> reservaEstoqueRepository,
        INotifier notifier)
    {
        _reservaEstoqueRepository = reservaEstoqueRepository;
        _notifier = notifier;
    }

    public async Task<Result> HandleAsync(LiberarEstoque request, CancellationToken cancellationToken = default)
    {
        var reserva = await _reservaEstoqueRepository.FirstOrDefaultAsync(
            builder => builder.Where(r => r.PedidoId == request.PedidoId),
            cancellationToken);

        if (reserva == null)
        {
            _notifier.NotifyWarning(
                "Reserva de estoque não encontrada para liberar. {PedidoId}",
                request.PedidoId);
            return Error.NotFound(
                "RESERVA.NOT_FOUND",
                $"Reserva de estoque não encontrada para pedido {request.PedidoId}");
        }

        if (reserva.Status == StatusReserva.Liberada)
        {
            _notifier.NotifyInformation(
                "Reserva já estava liberada (idempotente). {PedidoId} {ReservaId} {Status}",
                request.PedidoId, reserva.Id, reserva.Status.ToString());
            return Result.Success();
        }

        reserva.Status = StatusReserva.Liberada;

        await _reservaEstoqueRepository.SaveOrUpdateAsync(
            reserva,
            r => r.Id == reserva.Id,
            cancellationToken);

        _notifier.NotifyInformation(
            "Estoque liberado com sucesso. {PedidoId} {ReservaId} {Status} {TotalItens}",
            request.PedidoId, reserva.Id, reserva.Status.ToString(), reserva.Itens?.Count ?? 0);

        return Result.Success();
    }
}
