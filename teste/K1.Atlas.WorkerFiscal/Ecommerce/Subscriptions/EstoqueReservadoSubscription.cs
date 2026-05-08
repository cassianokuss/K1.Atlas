using K1.Atlas.PubSub.Consumer;
using K1.Atlas.PubSub.HostedServices;
using MediatR;
using K1.Atlas.WorkerFiscal.Ecommerce.Commands;
using K1.Atlas.Telemetry.Logging;
using System.Diagnostics;

namespace K1.Atlas.WorkerFiscal.Ecommerce;

public class EstoqueReservadoSubscription : IBackgroundConsumer<ReservaEstoque>
{
    private readonly ISender _sender;
    private readonly INotifier _notifier;

    public EstoqueReservadoSubscription(ISender sender, INotifier notifier)
    {
        _sender = sender;
        _notifier = notifier;
    }

    public async Task ConsumeAsync(ReservaEstoque obj, IMessageContext context, CancellationToken cancellationToken)
    {
        try
        {
            _notifier.NotifyInformation(
                "Iniciando emissão de nota fiscal após reserva de estoque. PedidoId: {PedidoId} ReservaId: {ReservaId}",
                obj.PedidoId, obj.Id);

            // Send command via MediatR
            var command = new EmitirNotaFiscal
            {
                PedidoId = obj.PedidoId,
                ReservaId = obj.Id
            };
            await _sender.SendAsync(command, cancellationToken);

            _notifier.NotifyInformation(
                "Emissão de nota fiscal concluída. PedidoId: {PedidoId} ReservaId: {ReservaId}",
                obj.PedidoId, obj.Id);
        }
        catch (Exception ex)
        {
            // Log error but acknowledge to avoid infinite requeue loops
            _notifier.NotifyError(
                "Erro ao emitir nota fiscal. PedidoId: {PedidoId} ReservaId: {ReservaId} Erro: {Erro}",
                obj.PedidoId, obj.Id, ex.Message);

            Activity.Current?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }

        // Always acknowledge to prevent requeue loops
        await context.AckAsync(cancellationToken);
    }
}
