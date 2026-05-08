using K1.Atlas.Telemetry;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.WorkerValidacao.Ecommerce.Commands;
using K1.Atlas.WorkerValidacao.Ecommerce.Services;
using K1.Atlas.Domain.Repositories;
using K1.Atlas.PubSub.Producer;
using MediatR;

namespace K1.Atlas.WorkerValidacao.Ecommerce.Handlers;

public class ValidarCreditoHandler : IRequestHandler<ValidarCredito, ResultadoValidacao>
{
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Pedido> _pedidoRepository;
    private readonly IMessageProducer _messageProducer;
    private readonly INotifier _notifier;
    private readonly IBureauCreditoService _bureauService;
    private const string ServiceName = "worker-validacao";

    public ValidarCreditoHandler(
        IRepository<Cliente> clienteRepository,
        IRepository<Pedido> pedidoRepository,
        IMessageProducer messageProducer,
        INotifier notifier,
        IBureauCreditoService bureauService)
    {
        _clienteRepository = clienteRepository;
        _pedidoRepository = pedidoRepository;
        _messageProducer = messageProducer;
        _notifier = notifier;
        _bureauService = bureauService;
    }

    public async Task<ResultadoValidacao> HandleAsync(ValidarCredito request, CancellationToken cancellationToken)
    {
        var pedido = request.Pedido;
        
        // Log start of validation
        _notifier.NotifyInformation(
            "Iniciando validação de crédito. {NumeroPedido} {ClienteId} {ValorTotal}",
            pedido.NumeroPedido, pedido.ClienteId, pedido.ValorTotal);

        // 1. Load Cliente entity from MongoDB
        var cliente = await _clienteRepository.FirstOrDefaultAsync(
            query => query.Where(c => c.CpfCnpj == pedido.Cliente.CpfCnpj || c.Nome.Contains(pedido.ClienteId)),
            cancellationToken);

        if (cliente == null)
        {
            throw new InvalidOperationException($"Cliente não encontrado: {pedido.ClienteId}");
        }

        // 2. Simulate credit bureau HTTP call
        var scoreBureau = await _bureauService.SimularConsultaAsync(cliente.CpfCnpj, cancellationToken);
        
        _notifier.NotifyInformation(
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
            _notifier.NotifyInformation(
                "Crédito aprovado. {NumeroPedido} {ClienteId} {LimiteDisponivel} {Score}",
                pedido.NumeroPedido, cliente.CpfCnpj, limiteDisponivel, scoreBureau);

            EcommerceMetrics.IncrementPedidoAprovado(ServiceName, pedido.NumeroPedido);
        }
        else
        {
            // 5. Update Pedido status to "Rejeitado"
            pedido.Status = StatusPedido.Rejeitado;
            pedido.MotivoRejeicao = $"Limite de crédito insuficiente. Disponível: {limiteDisponivel:C}, Necessário: {pedido.ValorTotal:C}";
            resultado.MotivoRejeicao = pedido.MotivoRejeicao;
            routingKey = "PedidoRejeitado";
            
            // Log rejection
            _notifier.NotifyWarning(
                "Pedido rejeitado. {NumeroPedido} {Motivo} {LimiteNecessario} {LimiteDisponivel}",
                pedido.NumeroPedido, pedido.MotivoRejeicao, pedido.ValorTotal, limiteDisponivel);

            EcommerceMetrics.IncrementPedidoRejeitado(ServiceName, pedido.NumeroPedido, "credito_insuficiente");
        }

        // 6. Save updated Pedido to MongoDB
        await _pedidoRepository.SaveOrUpdateAsync(
            pedido, 
            p => p.Id == pedido.Id, 
            cancellationToken);

        // 7. Publish appropriate message to RabbitMQ
        await _messageProducer.Publish(
            pedido,
            PublishOptions.RoutingTo(routingKey).ToExchange("Pedidos"));

        _notifier.NotifyInformation(
            "Validação de crédito concluída. {NumeroPedido} {Status} {RoutingKey}",
            pedido.NumeroPedido, pedido.Status, routingKey);

        return resultado;
    }
}
