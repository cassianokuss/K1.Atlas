using K1.Atlas.Domain.Repositories;
using K1.Atlas.PubSub.Producer;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Features.EmitirNotaFiscal.Domain;
using K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Features.EmitirNotaFiscal.Infrastructure;
using K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Features.EmitirNotaFiscal.Mappers;
using MediatR;

namespace K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Features.EmitirNotaFiscal;

public class EmitirNotaFiscal : IRequest<NotaFiscal>
{
    public string PedidoId { get; set; } = string.Empty;
    public string ReservaId { get; set; } = string.Empty;
}

public class EmitirNotaFiscalHandler : IRequestHandler<EmitirNotaFiscal, NotaFiscal>
{
    private readonly IRepository<Pedido> _pedidoRepository;
    private readonly IRepository<Cliente> _clienteRepository;
    private readonly IRepository<NotaFiscal> _notaFiscalRepository;
    private readonly IMessageProducer _messageProducer;
    private readonly ISefazRetryPolicy _sefazRetryPolicy;
    private readonly INotifier _notifier;
    private readonly NotaFiscalMetrics _notaFiscalMetrics;
    private const string ServiceName = "worker-fiscal";

    public EmitirNotaFiscalHandler(
        IRepository<Pedido> pedidoRepository,
        IRepository<Cliente> clienteRepository,
        IRepository<NotaFiscal> notaFiscalRepository,
        IMessageProducer messageProducer,
        ISefazRetryPolicy sefazRetryPolicy,
        INotifier notifier,
        NotaFiscalMetrics notaFiscalMetrics)
    {
        _pedidoRepository = pedidoRepository;
        _clienteRepository = clienteRepository;
        _notaFiscalRepository = notaFiscalRepository;
        _messageProducer = messageProducer;
        _sefazRetryPolicy = sefazRetryPolicy;
        _notifier = notifier;
        _notaFiscalMetrics = notaFiscalMetrics;
    }

    public async Task<NotaFiscal> HandleAsync(EmitirNotaFiscal request, CancellationToken cancellationToken = default)
    {
        _notifier.NotifyInformation("Iniciando emissão de nota fiscal. {PedidoId} {ReservaId}",
            request.PedidoId, request.ReservaId);

        var pedido = await CarregarPedidoAsync(request.PedidoId, cancellationToken);
        var cliente = await CarregarClienteAsync(pedido.ClienteId, cancellationToken);

        var impostos = CalculadoraImpostos.Calcular(pedido.ValorProdutos);
        
        _notifier.NotifyInformation("Impostos calculados. {PedidoId} ICMS:{ICMS} PIS:{PIS} COFINS:{COFINS} IPI:{IPI}",
            pedido.NumeroPedido, impostos.ValorICMS, impostos.ValorPIS, impostos.ValorCOFINS, impostos.ValorIPI);

        var chaveAcesso = GeradorChaveAcesso.Gerar();
        var numeroNota = GeradorNumeroNota.Gerar();
        
        var notaFiscal = NotaFiscalMapper.CriarNotaFiscal(pedido, cliente, impostos, chaveAcesso, numeroNota);

        await _notaFiscalRepository.SaveOrUpdateAsync(notaFiscal, nf => nf.Id == notaFiscal.Id, cancellationToken);

        var (sucesso, protocolo, tentativas) = await _sefazRetryPolicy.ExecutarComRetryAsync(notaFiscal, cancellationToken);

        notaFiscal.Status = sucesso ? StatusNotaFiscal.Autorizada : StatusNotaFiscal.Rejeitada;
        notaFiscal.ProtocoloAutorizacao = protocolo;
        notaFiscal.TentativasEnvio = tentativas;

        await _notaFiscalRepository.SaveOrUpdateAsync(notaFiscal, nf => nf.Id == notaFiscal.Id, cancellationToken);

        if (sucesso)
        {
            await PublicarEventosAsync(notaFiscal, protocolo!);
            
            _notifier.NotifyInformation("Nota fiscal emitida com sucesso. {NumeroNF} {Serie} {Protocolo} {ValorTotal} {Tentativas}",
                notaFiscal.Numero, notaFiscal.Serie, protocolo, notaFiscal.ValorTotal, tentativas);

            _notaFiscalMetrics.IncrementNotaFiscalGerada(ServiceName, notaFiscal.Numero, notaFiscal.ChaveAcesso);
        }

        return notaFiscal;
    }

    private async Task<Pedido> CarregarPedidoAsync(string pedidoId, CancellationToken cancellationToken)
    {
        var pedido = await _pedidoRepository.FirstOrDefaultAsync(
            q => q.Where(p => p.Id == pedidoId),
            cancellationToken);

        if (pedido == null)
        {
            _notifier.NotifyError("Pedido não encontrado. {PedidoId}", pedidoId);
            throw new InvalidOperationException($"Pedido {pedidoId} não encontrado");
        }

        return pedido;
    }

    private async Task<Cliente> CarregarClienteAsync(string clienteId, CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.FirstOrDefaultAsync(
            q => q.Where(c => c.Nome == clienteId || true),
            cancellationToken);

        if (cliente == null)
        {
            _notifier.NotifyError("Cliente não encontrado. {ClienteId}", clienteId);
            throw new InvalidOperationException($"Cliente {clienteId} não encontrado");
        }

        return cliente;
    }

    private async Task PublicarEventosAsync(NotaFiscal notaFiscal, string protocolo)
    {
        await _messageProducer.Publish(notaFiscal, new PublishOptions
        {
            RoutingKey = "NotaFiscalEmitida"
        });

        var evento = NotaFiscalMapper.ParaEvento(notaFiscal, protocolo);
        await _messageProducer.Publish(evento, PublishOptions.RoutingTo("NotaFiscalEmitida").ToExchange("Pedidos"));
    }
}
