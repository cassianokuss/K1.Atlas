using K1.Atlas.PubSub.Consumer;
using K1.Atlas.PubSub.HostedServices;
using K1.Atlas.Domain.Repositories;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Ecommerce.Contracts.Events;
using K1.Atlas.Ecommerce.Contracts.Entities;
using System.Diagnostics;

namespace K1.Atlas.Ecommerce.Api.Ecommerce.Subscriptions;

public class NotaFiscalEmitidaSubscription : IBackgroundConsumer<NotaFiscalEmitida>
{
    private readonly IRepository<Pedido> _pedidoRepository;
    private readonly INotifier _notifier;

    public NotaFiscalEmitidaSubscription(IRepository<Pedido> pedidoRepository, INotifier notifier)
    {
        _pedidoRepository = pedidoRepository;
        _notifier = notifier;
    }

    public async Task ConsumeAsync(NotaFiscalEmitida obj, IMessageContext context, CancellationToken cancellationToken)
    {
        try
        {
            _notifier.NotifyInformation(
                "Recebida notificação de nota fiscal emitida. PedidoId: {PedidoId} NotaFiscalId: {NotaFiscalId} Numero: {Numero}",
                obj.PedidoId, obj.NotaFiscalId, obj.NumeroNotaFiscal);

            // Load the Pedido
            var pedido = await _pedidoRepository.FirstOrDefaultAsync(
                q => q.Where(p => p.Id == obj.PedidoId),
                cancellationToken);

            if (pedido == null)
            {
                _notifier.NotifyWarning(
                    "Pedido não encontrado para atualização de status. PedidoId: {PedidoId}",
                    obj.PedidoId);
                
                await context.AckAsync(cancellationToken);
                return;
            }

            // Update Pedido status to Concluído
            pedido.Status = StatusPedido.Concluido;
            pedido.DataConclusao = DateTime.UtcNow;

            // Save updated Pedido
            await _pedidoRepository.SaveOrUpdateAsync(
                pedido,
                p => p.Id == pedido.Id,
                cancellationToken);

            _notifier.NotifyInformation(
                "Status do pedido atualizado para Concluído. PedidoId: {PedidoId} NumeroPedido: {NumeroPedido} NotaFiscal: {NotaFiscal}",
                pedido.Id, pedido.NumeroPedido, obj.NumeroNotaFiscal);
        }
        catch (Exception ex)
        {
            _notifier.NotifyError(
                "Erro ao processar nota fiscal emitida. PedidoId: {PedidoId} NotaFiscalId: {NotaFiscalId} Erro: {Erro}",
                obj.PedidoId, obj.NotaFiscalId, ex.Message);

            Activity.Current?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }

        // Always acknowledge to prevent requeue loops
        await context.AckAsync(cancellationToken);
    }
}
