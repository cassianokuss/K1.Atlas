using K1.Atlas.PubSub.Consumer;
using K1.Atlas.PubSub.HostedServices;
using MediatR;
using K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce.Commands;
using K1.Atlas.Telemetry.Logging;
using System.Diagnostics;

namespace K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce;

public class PedidoCriadoSubscription : IBackgroundConsumer<Pedido>
{
    private readonly ISender _sender;
    private readonly INotifier _notifier;

    public PedidoCriadoSubscription(ISender sender, INotifier notifier)
    {
        _sender = sender;
        _notifier = notifier;
    }

    public async Task ConsumeAsync(Pedido obj, IMessageContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Structured logging - entry 1: Start processing
            _notifier.NotifyInformation(
                "Iniciando validação de crédito. PedidoId: {PedidoId} NumeroPedido: {NumeroPedido} ClienteId: {ClienteId} ValorTotal: {ValorTotal}",
                obj.Id, obj.NumeroPedido, obj.ClienteId, obj.ValorTotal);

            // Send command via MediatR
            var command = new ValidarCredito
            {
                Pedido = obj
            };

            await _sender.SendAsync(command, cancellationToken);

            // Structured logging - entry 2: Completion
            _notifier.NotifyInformation(
                "Validação de crédito concluída. PedidoId: {PedidoId} NumeroPedido: {NumeroPedido}",
                obj.Id, obj.NumeroPedido);
        }
        catch (Exception ex)
        {
            // Log error but acknowledge to avoid infinite requeue loops
            _notifier.NotifyError(
                "Erro ao processar validação de crédito. PedidoId: {PedidoId} NumeroPedido: {NumeroPedido} Erro: {Erro}",
                obj.Id, obj.NumeroPedido, ex.Message);

            Activity.Current?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }

        // Always acknowledge to prevent requeue loops
        await context.AckAsync(cancellationToken);
    }
}
