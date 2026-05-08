using K1.Atlas.PubSub.Consumer;
using K1.Atlas.PubSub.HostedServices;
using MediatR;
using K1.Atlas.WorkerEstoque.Ecommerce.Commands;
using K1.Atlas.Telemetry.Logging;
using System.Diagnostics;

namespace K1.Atlas.WorkerEstoque.Ecommerce;

public class ReservarEstoqueSubscription : IBackgroundConsumer<Pedido>
{
    private readonly ISender _sender;
    private readonly INotifier _notifier;

    public ReservarEstoqueSubscription(ISender sender, INotifier notifier)
    {
        _sender = sender;
        _notifier = notifier;
    }

    public async Task ConsumeAsync(Pedido obj, IMessageContext context, CancellationToken cancellationToken)
    {
        try
        {
            // Add OpenTelemetry tags
            var totalItens = obj.Itens?.Sum(i => i.Quantidade) ?? 0;

            _notifier.NotifyInformation(
                "Iniciando reserva de estoque. PedidoId: {PedidoId} NumeroPedido: {NumeroPedido} TotalItens: {TotalItens}",
                obj.Id, obj.NumeroPedido, totalItens);

            // Send command via MediatR
            var command = new ReservarEstoque
            {
                Pedido = obj,
                PedidoId = obj.Id
            };

            await _sender.SendAsync(command, cancellationToken);

            _notifier.NotifyInformation(
                "Reserva de estoque concluída. PedidoId: {PedidoId} NumeroPedido: {NumeroPedido}",
                obj.Id, obj.NumeroPedido);
        }
        catch (Exception ex)
        {
            // Log error but acknowledge to avoid infinite requeue loops
            _notifier.NotifyError(
                "Erro ao processar reserva de estoque. PedidoId: {PedidoId} NumeroPedido: {NumeroPedido} Erro: {Erro}",
                obj.Id, obj.NumeroPedido, ex.Message);

            Activity.Current?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }

        // Always acknowledge to prevent requeue loops
        await context.AckAsync(cancellationToken);
    }
}
