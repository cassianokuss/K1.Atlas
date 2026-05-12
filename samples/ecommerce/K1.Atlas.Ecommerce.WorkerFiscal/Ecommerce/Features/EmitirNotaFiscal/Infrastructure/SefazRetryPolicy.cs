using System.Diagnostics;
using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Services;
using K1.Atlas.Telemetry.Logging;

namespace K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Features.EmitirNotaFiscal.Infrastructure;

public interface ISefazRetryPolicy
{
    Task<(bool Sucesso, string? Protocolo, int Tentativas)> ExecutarComRetryAsync(
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

    public async Task<(bool Sucesso, string? Protocolo, int Tentativas)> ExecutarComRetryAsync(
        NotaFiscal notaFiscal,
        CancellationToken cancellationToken)
    {
        bool sucesso = false;
        string? protocolo = null;
        int tentativa = 0;

        while (tentativa < MaxRetryAttempts && !sucesso)
        {
            tentativa++;
            notaFiscal.TentativasEnvio = tentativa;

            try
            {
                protocolo = await _sefazService.EnviarNotaAsync(notaFiscal, cancellationToken);
                sucesso = true;

                _notifier.NotifyInformation(
                    "SEFAZ respondeu com sucesso na tentativa {Tentativa}. {NumeroNF} {Protocolo}",
                    tentativa, notaFiscal.Numero, protocolo);
            }
            catch (HttpRequestException ex)
            {
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
                else
                {
                    _notifier.NotifyError(
                        "Falha definitiva ao enviar nota fiscal após {Tentativas} tentativas. {NumeroNF}",
                        MaxRetryAttempts, notaFiscal.Numero);
                }
            }
        }

        return (sucesso, protocolo, tentativa);
    }
}
