using K1.Atlas.Domain.Repositories;
using K1.Atlas.PubSub.Producer;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce.Services;
using MediatR;

namespace K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce.Commands;

public class ValidarCredito : IRequest<ResultadoValidacao>
{
    public Pedido Pedido { get; set; } = null!;
}

public class ValidarCreditoHandler(
    IRepository<Cliente> clienteRepository,
    IRepository<Pedido> pedidoRepository,
    IMessageProducer messageProducer,
    INotifier notifier,
    IBureauCreditoService bureauService,
    PedidoMetrics pedidoMetrics)
    : IRequestHandler<ValidarCredito, ResultadoValidacao>
{
    private const string ServiceName = "worker-validacao";

    public async Task<ResultadoValidacao> HandleAsync(ValidarCredito request, CancellationToken cancellationToken)
    {
        var pedido = request.Pedido;
        
        // Log start of validation
        notifier.NotifyInformation(
            "Iniciando validação de crédito. {NumeroPedido} {ClienteId} {ValorTotal}",
            pedido.NumeroPedido, pedido.ClienteId, pedido.ValorTotal);

        // 1. Load Cliente entity from MongoDB
        var cliente = await clienteRepository.FirstOrDefaultAsync(
            query => query.Where(c => c.CpfCnpj == pedido.Cliente.CpfCnpj || c.Nome.Contains(pedido.ClienteId)),
            cancellationToken);

        if (cliente == null)
        {
            throw new InvalidOperationException($"Cliente não encontrado: {pedido.ClienteId}");
        }

        // 2. Simulate credit bureau HTTP call
        var scoreBureau = await bureauService.SimularConsultaAsync(cliente.CpfCnpj, cancellationToken);
        
        notifier.NotifyInformation(
            "Consulta ao bureau de crédito concluída. {CpfCnpj} {Score}",
            cliente.CpfCnpj, scoreBureau);

        // 3. Check credit limit
        var limiteDisponivel = cliente.LimiteCredito - cliente.CreditoUtilizado;
        var aprovado = limiteDisponivel >= pedido.ValorTotal;

        // 4. Create ResultadoValidacao
        var resultado = new ResultadoValidacao
        {
            Aprovado = aprovado,
            ScoreBureau = scoreBureau,
            LimiteDisponivel = limiteDisponivel,
            MotivoRejeicao = string.Empty
        };

        string routingKey;
        
        if (aprovado)
        {
            // 5. Update Pedido status to "Aprovado"
            pedido.Status = StatusPedido.Aprovado;
            pedido.DataAprovacao = DateTime.UtcNow;
            routingKey = "PedidoAprovado";
            
            // Log approval
            notifier.NotifyInformation(
                "Crédito aprovado. {NumeroPedido} {ClienteId} {LimiteDisponivel} {Score}",
                pedido.NumeroPedido, cliente.CpfCnpj, limiteDisponivel, scoreBureau);

            pedidoMetrics.IncrementPedidoAprovado(ServiceName, pedido.NumeroPedido);
        }
        else
        {
            // 5. Update Pedido status to "Rejeitado"
            pedido.Status = StatusPedido.Rejeitado;
            pedido.MotivoRejeicao = $"Limite de crédito insuficiente. Disponível: {limiteDisponivel:C}, Necessário: {pedido.ValorTotal:C}";
            resultado.MotivoRejeicao = pedido.MotivoRejeicao;
            routingKey = "PedidoRejeitado";
            
            // Log rejection
            notifier.NotifyWarning(
                "Pedido rejeitado. {NumeroPedido} {Motivo} {LimiteNecessario} {LimiteDisponivel}",
                pedido.NumeroPedido, pedido.MotivoRejeicao, pedido.ValorTotal, limiteDisponivel);

            pedidoMetrics.IncrementPedidoRejeitado(ServiceName, pedido.NumeroPedido, "credito_insuficiente");
        }

        // 6. Save updated Pedido to MongoDB
        await pedidoRepository.SaveOrUpdateAsync(
            pedido, 
            p => p.Id == pedido.Id, 
            cancellationToken);

        // 7. Publish appropriate message to RabbitMQ
        await messageProducer.Publish(
            pedido,
            PublishOptions.RoutingTo(routingKey).ToExchange("Pedidos"));

        notifier.NotifyInformation(
            "Validação de crédito concluída. {NumeroPedido} {Status} {RoutingKey}",
            pedido.NumeroPedido, pedido.Status, routingKey);

        return resultado;
    }
}
