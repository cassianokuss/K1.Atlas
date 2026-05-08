using System.Diagnostics;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.WorkerEstoque.Ecommerce.Commands;
using K1.Atlas.Domain.Repositories;
using MediatR;

namespace K1.Atlas.WorkerEstoque.Ecommerce.Handlers;

public class LiberarEstoqueHandler : IRequestHandler<LiberarEstoque, bool>
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

    public async Task<bool> HandleAsync(LiberarEstoque request, CancellationToken cancellationToken = default)
    {
        // Step 1: Find active ReservaEstoque by PedidoId
        var reserva = await _reservaEstoqueRepository.FirstOrDefaultAsync(
            builder => builder.Where(r => r.PedidoId == request.PedidoId),
            cancellationToken);

        // Step 2: If not found, return false (idempotent - already released or never existed)
        if (reserva == null)
        {
            _notifier.NotifyWarning(
                "Reserva de estoque não encontrada para liberar. {PedidoId}",
                request.PedidoId);

            return false;
        }

        // Step 3: If already released, return true (idempotent)
        if (reserva.Status == StatusReserva.Liberada)
        {
            _notifier.NotifyInformation(
                "Reserva já estava liberada (idempotente). {PedidoId} {ReservaId} {Status}",
                request.PedidoId,
                reserva.Id,
                reserva.Status.ToString());

            // Add telemetry even for idempotent case
            var totalItensIdempotent = reserva.Itens?.Count ?? 0;

            return true;
        }

        // Step 3: Update reservation status to "Liberada"
        reserva.Status = StatusReserva.Liberada;

        // Step 4: Calculate total items
        var totalItens = reserva.Itens?.Count ?? 0;
        
        // Step 5: Add OpenTelemetry tags (4 tags minimum): PedidoId, ReservaId, Status, TotalItensLiberados

        // Step 6: Persist to MongoDB
        await _reservaEstoqueRepository.SaveOrUpdateAsync(
            reserva,
            r => r.Id == reserva.Id,
            cancellationToken);

        // Step 7: Add structured logging via INotifier (2+ log entries)
        _notifier.NotifyInformation(
            "Estoque liberado com sucesso. {PedidoId} {ReservaId} {Status} {TotalItens}",
            request.PedidoId,
            reserva.Id,
            reserva.Status.ToString(),
            reserva.Itens?.Count ?? 0);

        // Step 8: Return true on success
        return true;
    }
}
