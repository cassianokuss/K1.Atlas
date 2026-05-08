using System.Diagnostics;
using MediatR;
using K1.Atlas.Telemetry;
using K1.Atlas.Telemetry.Logging;
using K1.Atlas.WorkerFiscal.Ecommerce.Commands;
using K1.Atlas.WorkerFiscal.Ecommerce.Services;
using K1.Atlas.Domain.Repositories;
using K1.Atlas.PubSub.Producer;

namespace K1.Atlas.WorkerFiscal.Ecommerce.Handlers;

public class EmitirNotaFiscalHandler(
    IRepository<Pedido> pedidoRepository,
    IRepository<Cliente> clienteRepository,
    IRepository<NotaFiscal> notaFiscalRepository,
    IMessageProducer messageProducer,
    INotifier notifier,
    ISefazService sefazService)
    : IRequestHandler<EmitirNotaFiscal, NotaFiscal>
{
    private const int MaxRetryAttempts = 3;
    private const decimal AliquotaICMS = 0.18m; // 18%
    private const decimal AliquotaPIS = 0.0165m; // 1.65%
    private const decimal AliquotaCOFINS = 0.076m; // 7.6%
    private const decimal AliquotaIPI = 0.10m; // 10%
    private const string ServiceName = "worker-fiscal";

    public async Task<NotaFiscal> HandleAsync(EmitirNotaFiscal request, CancellationToken cancellationToken = default)
    {
        notifier.NotifyInformation("Iniciando emissão de nota fiscal. {PedidoId} {ReservaId}", 
            request.PedidoId, request.ReservaId);

        // Step 1: Load Pedido
        var pedido = await pedidoRepository.FirstOrDefaultAsync(
            q => q.Where(p => p.Id == request.PedidoId),
            cancellationToken);

        if (pedido == null)
        {
            notifier.NotifyError("Pedido não encontrado. {PedidoId}", request.PedidoId);
            throw new InvalidOperationException($"Pedido {request.PedidoId} não encontrado");
        }

        // Step 2: Load Cliente
        var cliente = await clienteRepository.FirstOrDefaultAsync(
            q => q.Where(c => c.Nome == pedido.ClienteId || true), // Simplified query
            cancellationToken);

        if (cliente == null)
        {
            notifier.NotifyError("Cliente não encontrado. {ClienteId}", pedido.ClienteId);
            throw new InvalidOperationException($"Cliente {pedido.ClienteId} não encontrado");
        }

        // Step 3: Calculate taxes
        var valorBase = pedido.ValorProdutos;
        var valorICMS = Math.Round(valorBase * AliquotaICMS, 2);
        var valorPIS = Math.Round(valorBase * AliquotaPIS, 2);
        var valorCOFINS = Math.Round(valorBase * AliquotaCOFINS, 2);
        var valorIPI = Math.Round(valorBase * AliquotaIPI, 2);
        var valorTotal = valorBase + valorICMS + valorPIS + valorCOFINS + valorIPI;

        notifier.NotifyInformation("Impostos calculados. {PedidoId} ICMS:{ICMS} PIS:{PIS} COFINS:{COFINS} IPI:{IPI}", 
            pedido.NumeroPedido, valorICMS, valorPIS, valorCOFINS, valorIPI);

        // Step 4: Generate ChaveAcesso (44-char SEFAZ key - simulated)
        var chaveAcesso = GerarChaveAcesso();

        // Step 5: Create NotaFiscal entity with status "Processando"
        var notaFiscal = new NotaFiscal
        {
            PedidoId = pedido.Id,
            ClienteId = pedido.ClienteId,
            Cliente = cliente,
            ChaveAcesso = chaveAcesso,
            Numero = GerarNumeroNota(),
            Serie = "1",
            DataEmissao = DateTime.UtcNow,
            Status = StatusNotaFiscal.Processando,
            ValorProdutos = valorBase,
            ValorICMS = valorICMS,
            ValorPIS = valorPIS,
            ValorCOFINS = valorCOFINS,
            ValorIPI = valorIPI,
            ValorTotal = valorTotal,
            Itens = pedido.Itens.Select(i => new ItemNotaFiscal
            {
                ProdutoId = i.ProdutoId,
                CodigoProduto = i.CodigoProduto,
                DescricaoProduto = i.DescricaoProduto,
                Quantidade = i.Quantidade,
                ValorUnitario = i.ValorUnitario,
                ValorTotal = i.Subtotal,
                AliquotaICMS = AliquotaICMS,
                ValorICMS = Math.Round(i.Subtotal * AliquotaICMS, 2),
                ValorPIS = Math.Round(i.Subtotal * AliquotaPIS, 2),
                ValorCOFINS = Math.Round(i.Subtotal * AliquotaCOFINS, 2),
                ValorIPI = Math.Round(i.Subtotal * AliquotaIPI, 2)
            }).ToList()
        };

        // Step 6: Save initial NotaFiscal
        await notaFiscalRepository.SaveOrUpdateAsync(
            notaFiscal,
            nf => nf.Id == notaFiscal.Id,
            cancellationToken);

        // Step 7: Implement SEFAZ retry logic with exponential backoff
        bool sucesso = false;
        string? protocolo = null;
        int tentativa = 0;

        while (tentativa < MaxRetryAttempts && !sucesso)
        {
            tentativa++;
            notaFiscal.TentativasEnvio = tentativa;

            try
            {
                protocolo = await sefazService.EnviarNotaAsync(notaFiscal, cancellationToken);
                sucesso = true;

                notifier.NotifyInformation("SEFAZ respondeu com sucesso na tentativa {Tentativa}. {NumeroNF} {Protocolo}", 
                    tentativa, notaFiscal.Numero, protocolo);
            }
            catch (HttpRequestException ex)
            {
                notifier.NotifyWarning("Tentativa {Tentativa} de envio SEFAZ falhou. {NumeroNF} {Erro}", 
                    tentativa, notaFiscal.Numero, ex.Message);

                Activity.Current?.AddEvent(new ActivityEvent("SefazRetry", 
                    tags: new ActivityTagsCollection 
                    {
                        { "attempt.number", tentativa },
                        { "attempt.reason", ex.Message },
                        { "backoff.seconds", Math.Pow(2, tentativa) }
                    }));

                if (tentativa < MaxRetryAttempts)
                {
                    var delayMs = (int)(2000 * Math.Pow(2, tentativa - 1)); // 2s, 4s, 8s
                    await Task.Delay(delayMs, cancellationToken);
                }
                else
                {
                    notifier.NotifyError("Falha definitiva ao enviar nota fiscal após {Tentativas} tentativas. {NumeroNF}", 
                        MaxRetryAttempts, notaFiscal.Numero);
                }
            }
        }

        // Step 8: Update NotaFiscal status
        if (sucesso)
        {
            notaFiscal.Status = StatusNotaFiscal.Autorizada;
            notaFiscal.ProtocoloAutorizacao = protocolo;
        }
        else
        {
            notaFiscal.Status = StatusNotaFiscal.Rejeitada;
        }

        // Step 9: Persist updated NotaFiscal
        await notaFiscalRepository.SaveOrUpdateAsync(
            notaFiscal,
            nf => nf.Id == notaFiscal.Id,
            cancellationToken);

        // Step 11: Publish message to RabbitMQ (only if authorized)
        if (sucesso)
        {
            await messageProducer.Publish(notaFiscal, new PublishOptions
            {
                RoutingKey = "NotaFiscalEmitida"
            });

            notifier.NotifyInformation("Nota fiscal emitida com sucesso. {NumeroNF} {Serie} {Protocolo} {ValorTotal} {Tentativas}", 
                notaFiscal.Numero, notaFiscal.Serie, protocolo, notaFiscal.ValorTotal, tentativa);

            EcommerceMetrics.IncrementNotaFiscalGerada(ServiceName, notaFiscal.Numero, notaFiscal.ChaveAcesso);
        }

        return notaFiscal;
    }

    private string GerarChaveAcesso()
    {
        // Generate 44-digit SEFAZ access key (simplified simulation)
        // Real format: UF + AAMM + CNPJ + Modelo + Serie + NumeroNF + Forma Emissao + Codigo + DV
        var random = new Random();
        return string.Concat(Enumerable.Range(0, 44).Select(_ => random.Next(0, 10)));
    }

    private string GerarNumeroNota()
    {
        // Simple sequential number generation (in production, would use database sequence)
        return DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
    }
}
