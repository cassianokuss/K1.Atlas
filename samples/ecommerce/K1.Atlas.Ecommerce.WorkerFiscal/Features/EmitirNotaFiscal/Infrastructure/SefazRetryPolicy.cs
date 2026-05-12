using System.Diagnostics;
using K1.Atlas.Domain.ResultPattern;
using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Ecommerce.WorkerFiscal.Services;
using K1.Atlas.Telemetry.Logging;

namespace K1.Atlas.Ecommerce.WorkerFiscal.Features.EmitirNotaFiscal.Infrastructure;

public record SefazResult(string Protocolo, int Tentativas);

public interface ISefazRetryPolicy
{
    Task<ResultT<SefazResult>> ExecutarComRetryAsync(
        NotaFiscal notaFiscal,
        CancellationToken cancellationToken);
}

public class SefazRetryPolicy : ISefazRetryPolicy
{
    private const int MaxRetryAttempts = 3;
    private readonly ISefazService _sefazService;
    private readonly INotifier _notifier;

    public SefazRetryPolicy(ISefazService sefazService, INotifier notifier)
    {
        _sefazService = sefazService;
        _notifier = notifier;
    }

    public async Task<ResultT<SefazResult>> ExecutarComRetryAsync(
        NotaFiscal notaFiscal,
        CancellationToken cancellationToken)
    {
        string? protocolo = null;
        int tentativa = 0;
        Exception? lastException = null;

        while (tentativa < MaxRetryAttempts)
        {
            tentativa++;
            notaFiscal.TentativasEnvio = tentativa;

            try
            {
                protocolo = await _sefazService.EnviarNotaAsync(notaFiscal, cancellationToken);

                _notifier.NotifyInformation(
                    "SEFAZ respondeu com sucesso na tentativa {Tentativa}. {NumeroNF} {Protocolo}",
                    tentativa, notaFiscal.Numero, protocolo);

                return new SefazResult(protocolo, tentativa);
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                
                _notifier.NotifyWarning(
                    "Tentativa {Tentativa} de envio SEFAZ falhou. {NumeroNF} {Erro}",
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
            }
        }

        _notifier.NotifyError(
            "Falha definitiva ao enviar nota fiscal após {Tentativas} tentativas. {NumeroNF}",
            MaxRetryAttempts, notaFiscal.Numero);

        return Error.Failure(
            "SEFAZ.RETRY_FAILED",
            $"Falha ao enviar nota fiscal após {MaxRetryAttempts} tentativas: {lastException?.Message}");
    }
}
