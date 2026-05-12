using K1.Atlas.PubSub.Consumer;
using K1.Atlas.PubSub.HostedServices;
using MediatR;
using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Features.LiberarEstoque;
using K1.Atlas.Telemetry.Logging;
using System.Diagnostics;

namespace K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce;

public class PedidoRejeitadoSubscription : IBackgroundConsumer<Pedido>
{
    private readonly ISender _sender;
    private readonly INotifier _notifier;

    public PedidoRejeitadoSubscription(ISender sender, INotifier notifier)
    {
        _sender = sender;
        _notifier = notifier;
    }

    public async Task ConsumeAsync(Pedido obj, IMessageContext context, CancellationToken cancellationToken)
    {
        try
        {
            var totalItens = obj.Itens?.Sum(i => i.Quantidade) ?? 0;

            _notifier.NotifyInformation(
                "Pedido rejeitado - iniciando liberação de estoque. PedidoId: {PedidoId} NumeroPedido: {NumeroPedido} Motivo: {Motivo}",
                obj.Id, obj.NumeroPedido, obj.MotivoRejeicao ?? "Crédito insuficiente");

            var command = new LiberarEstoque
            {
                PedidoId = obj.Id
            };

            var resultado = await _sender.SendAsync(command, cancellationToken);

            if (resultado)
            {
                _notifier.NotifyInformation(
                    "Estoque liberado com sucesso após rejeição. PedidoId: {PedidoId} NumeroPedido: {NumeroPedido}",
                    obj.Id, obj.NumeroPedido);

            }
            else
            {
                _notifier.NotifyWarning(
                    "Reserva de estoque não encontrada para liberar. PedidoId: {PedidoId} NumeroPedido: {NumeroPedido}",
                    obj.Id, obj.NumeroPedido);
            }
        }
        catch (Exception ex)
        {
            _notifier.NotifyError(
                "Erro ao processar liberação de estoque. PedidoId: {PedidoId} NumeroPedido: {NumeroPedido} Erro: {Erro}",
                obj.Id, obj.NumeroPedido, ex.Message);

            Activity.Current?.SetStatus(ActivityStatusCode.Error, ex.Message);
        }

        await context.AckAsync(cancellationToken);
    }
}