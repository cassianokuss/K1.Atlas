using System.Diagnostics.Metrics;

namespace K1.Atlas.Telemetry;

public static class MetricsRegistry
{
    public static readonly Meter WorkerValidacaoPedidos = new("K1.Atlas.WorkerValidacao.Pedidos", "1.0.0");
    public static readonly Meter WorkerFiscalNotas = new("K1.Atlas.WorkerFiscal.NotasFiscais", "1.0.0");
    public static readonly Meter TesteApiPedidos = new("K1.Atlas.TesteApi.Pedidos", "1.0.0");
}
