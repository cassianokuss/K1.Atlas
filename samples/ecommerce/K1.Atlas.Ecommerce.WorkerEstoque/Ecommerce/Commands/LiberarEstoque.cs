using K1.Atlas.Domain.Repositories;
using K1.Atlas.Telemetry.Logging;
using MediatR;

namespace K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Commands;

public class LiberarEstoque : IRequest<bool>
{
    public string PedidoId { get; set; } = string.Empty;
}

public class LiberarEstoqueHandler(
    IRepository<ReservaEstoque> reservaEstoqueRepository,
    INotifier notifier)
    : IRequestHandler<LiberarEstoque, bool>
{
    public async Task<bool> HandleAsync(LiberarEstoque request, CancellationToken cancellationToken = default)
    {
        // Step 1: Find active ReservaEstoque by PedidoId
        var reserva = await reservaEstoqueRepository.FirstOrDefaultAsync(
            builder => builder.Where(r => r.PedidoId == request.PedidoId),
            cancellationToken);

        // Step 2: If not found, return false (idempotent - already released or never existed)
        if (reserva == null)
        {
            notifier.NotifyWarning(
                "Reserva de estoque não encontrada para liberar. {PedidoId}",
                request.PedidoId);

            return false;
        }

        // Step 3: If already released, return true (idempotent)
        if (reserva.Status == StatusReserva.Liberada)
        {
            notifier.NotifyInformation(
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

        // Step 5: Persist to MongoDB
        await reservaEstoqueRepository.SaveOrUpdateAsync(
            reserva,
            r => r.Id == reserva.Id,
            cancellationToken);

        // Step 6: Add structured logging via INotifier (2+ log entries)
        notifier.NotifyInformation(
            "Estoque liberado com sucesso. {PedidoId} {ReservaId} {Status} {TotalItens}",
            request.PedidoId,
            reserva.Id,
            reserva.Status.ToString(),
            reserva.Itens?.Count ?? 0);

        // Step 7: Return true on success
        return true;
    }
}

