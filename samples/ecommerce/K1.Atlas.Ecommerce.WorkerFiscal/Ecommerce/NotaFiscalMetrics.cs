using System.Diagnostics.Metrics;

namespace K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce;

public class NotaFiscalMetrics
{
    private static readonly Meter _meter = new("K1.Atlas.Ecommerce.WorkerFiscal.NotasFiscais", "1.0.0");
    private readonly Counter<long> _notasFiscaisGeradas;

    public NotaFiscalMetrics()
    {
        _notasFiscaisGeradas = _meter.CreateCounter<long>(
            "ecommerce.notas_fiscais.geradas",
            unit: "notas",
            description: "Total de notas fiscais geradas");
    }

    public void IncrementNotaFiscalGerada(string servico, string numeroNota, string chaveAcesso)
    {
        _notasFiscaisGeradas.Add(1,
            new KeyValuePair<string, object?>("servico", servico),
            new KeyValuePair<string, object?>("numero_nota", numeroNota),
            new KeyValuePair<string, object?>("chave_acesso", chaveAcesso));
    }
}
