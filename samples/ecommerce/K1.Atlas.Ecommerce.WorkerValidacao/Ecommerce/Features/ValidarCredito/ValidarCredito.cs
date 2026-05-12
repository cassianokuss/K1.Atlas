using K1.Atlas.Domain.Repositories;
using K1.Atlas.Domain.ResultPattern;
using K1.Atlas.PubSub.Producer;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Ecommerce.Contracts.ValueObjects;
using K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce.Services;
using K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce.Features.ValidarCredito.Domain;
using MediatR;

namespace K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce.Features.ValidarCredito;

public class ValidarCredito : IRequest<ResultT<ResultadoValidacao>>
{
    public Pedido Pedido { get; set; } = null!;
}

public class ValidarCreditoHandler : IRequestHandler<ValidarCredito, ResultT<ResultadoValidacao>>
{
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<Pedido> _pedidoRepository;
    private readonly IMessageProducer _messageProducer;
    private readonly INotifier _notifier;
    private readonly IBureauCreditoService _bureauService;
    private readonly PedidoMetrics _pedidoMetrics;
    private const string ServiceName = "worker-validacao";

    public ValidarCreditoHandler(
        IRepository<Cliente> clienteRepository,
        IRepository<Pedido> pedidoRepository,
        IMessageProducer messageProducer,
        INotifier notifier,
        IBureauCreditoService bureauService,
        PedidoMetrics pedidoMetrics)
    {
        _clienteRepository = clienteRepository;
        _pedidoRepository = pedidoRepository;
        _messageProducer = messageProducer;
        _notifier = notifier;
        _bureauService = bureauService;
        _pedidoMetrics = pedidoMetrics;
    }

    public async Task<ResultT<ResultadoValidacao>> HandleAsync(ValidarCredito request, CancellationToken cancellationToken)
    {
        var pedido = request.Pedido;

        _notifier.NotifyInformation(
            "Iniciando validação de crédito. {NumeroPedido} {ClienteId} {ValorTotal}",
            pedido.NumeroPedido, pedido.ClienteId, pedido.ValorTotal);

        var clienteResult = await CarregarClienteAsync(pedido, cancellationToken);
        if (!clienteResult.IsSuccess)
            return clienteResult.Error!;

        var cliente = clienteResult.Value;
        var scoreBureau = await _bureauService.SimularConsultaAsync(cliente.CpfCnpj, cancellationToken);

        _notifier.NotifyInformation(
            "Consulta ao bureau de crédito concluída. {CpfCnpj} {Score}",
            cliente.CpfCnpj, scoreBureau);

        var limiteDisponivel = ValidadorCreditoCliente.CalcularLimiteDisponivel(cliente);
        var aprovado = ValidadorCreditoCliente.TemLimite(cliente, pedido.ValorTotal);

        var resultado = aprovado
            ? DecisaoCredito.Aprovar(scoreBureau, limiteDisponivel)
            : DecisaoCredito.Rejeitar(limiteDisponivel, pedido.ValorTotal, scoreBureau);

        await AtualizarPedidoAsync(pedido, resultado, cancellationToken);
        await PublicarEventoAsync(pedido, resultado, cancellationToken);
        RegistrarMetricas(pedido, aprovado);

        _notifier.NotifyInformation(
            "Validação de crédito concluída. {NumeroPedido} {Status}",
            pedido.NumeroPedido, pedido.Status);

        return resultado;
    }

    private async Task<ResultT<Cliente>> CarregarClienteAsync(Pedido pedido, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.FirstOrDefaultAsync(
            query => query.Where(c => c.CpfCnpj == pedido.Cliente.CpfCnpj || c.Nome.Contains(pedido.ClienteId)),
            cancellationToken);

        if (cliente == null)
        {
            return Error.NotFound("CLIENTE.NOT_FOUND", $"Cliente não encontrado: {pedido.ClienteId}");
        }

        return cliente;
    }

    private async Task AtualizarPedidoAsync(Pedido pedido, ResultadoValidacao resultado, CancellationToken cancellationToken)
    {
        if (resultado.Aprovado)
        {
            pedido.Status = StatusPedido.Aprovado;
            pedido.DataAprovacao = DateTime.UtcNow;
        }
        else
        {
            pedido.Status = StatusPedido.Rejeitado;
            pedido.MotivoRejeicao = resultado.MotivoRejeicao;
        }

        await _pedidoRepository.SaveOrUpdateAsync(pedido, p => p.Id == pedido.Id, cancellationToken);
    }

    private async Task PublicarEventoAsync(Pedido pedido, ResultadoValidacao resultado, CancellationToken cancellationToken)
    {
        var routingKey = resultado.Aprovado ? "PedidoAprovado" : "PedidoRejeitado";
        await _messageProducer.Publish(pedido, PublishOptions.RoutingTo(routingKey).ToExchange("Pedidos"));
    }

    private void RegistrarMetricas(Pedido pedido, bool aprovado)
    {
        if (aprovado)
        {
            _notifier.NotifyInformation(
                "Crédito aprovado. {NumeroPedido} {ClienteId}",
                pedido.NumeroPedido, pedido.ClienteId);
            _pedidoMetrics.IncrementPedidoAprovado(ServiceName, pedido.NumeroPedido);
        }
        else
        {
            _notifier.NotifyWarning(
                "Pedido rejeitado. {NumeroPedido} {Motivo}",
                pedido.NumeroPedido, pedido.MotivoRejeicao);
            _pedidoMetrics.IncrementPedidoRejeitado(ServiceName, pedido.NumeroPedido, "credito_insuficiente");
        }
    }
}
