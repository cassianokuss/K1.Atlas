using System.Diagnostics.Metrics;

namespace K1.Atlas.Telemetry;

public static class EcommerceMetrics
{
    public static readonly Counter<long> PedidosCriados = MetricsRegistry.Meter.CreateCounter<long>(
        "ecommerce.pedidos.criados",
        unit: "pedidos",
        description: "Total de pedidos criados");

    public static readonly Counter<long> PedidosAprovados = MetricsRegistry.Meter.CreateCounter<long>(
        "ecommerce.pedidos.aprovados",
        unit: "pedidos",
        description: "Total de pedidos aprovados");

    public static readonly Counter<long> PedidosRejeitados = MetricsRegistry.Meter.CreateCounter<long>(
        "ecommerce.pedidos.rejeitados",
        unit: "pedidos",
        description: "Total de pedidos rejeitados");

    public static readonly Counter<long> NotasFiscaisGeradas = MetricsRegistry.Meter.CreateCounter<long>(
        "ecommerce.notas_fiscais.geradas",
        unit: "notas",
        description: "Total de notas fiscais geradas");

    public static void IncrementPedidoCriado(string servico, string clienteId)
    {
        PedidosCriados.Add(1, new KeyValuePair<string, object?>("servico", servico),
            new KeyValuePair<string, object?>("cliente_id", clienteId));
    }

    public static void IncrementPedidoAprovado(string servico, string numeroPedido)
    {
        PedidosAprovados.Add(1, new KeyValuePair<string, object?>("servico", servico),
            new KeyValuePair<string, object?>("numero_pedido", numeroPedido));
    }

    public static void IncrementPedidoRejeitado(string servico, string numeroPedido, string motivo)
    {
        PedidosRejeitados.Add(1, new KeyValuePair<string, object?>("servico", servico),
            new KeyValuePair<string, object?>("numero_pedido", numeroPedido),
            new KeyValuePair<string, object?>("motivo", motivo));
    }

    public static void IncrementNotaFiscalGerada(string servico, string numeroNota, string chaveAcesso)
    {
        NotasFiscaisGeradas.Add(1, new KeyValuePair<string, object?>("servico", servico),
            new KeyValuePair<string, object?>("numero_nota", numeroNota),
            new KeyValuePair<string, object?>("chave_acesso", chaveAcesso));
    }
}

public static class MetricsRegistry
{
    public static readonly Meter Meter = new("EcommerceMetrics", "1.0.0");
}