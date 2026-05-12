using System.Diagnostics.Metrics;

namespace K1.Atlas.Ecommerce.WorkerValidacao;

public class PedidoMetrics
{
    private static readonly Meter _meter = new("K1.Atlas.Ecommerce.WorkerValidacao.Pedidos", "1.0.0");
    private readonly Counter<long> _pedidosCriados;
    private readonly Counter<long> _pedidosAprovados;
    private readonly Counter<long> _pedidosRejeitados;

    public PedidoMetrics()
    {
        _pedidosCriados = _meter.CreateCounter<long>(
            "ecommerce.pedidos.criados",
            unit: "pedidos",
            description: "Total de pedidos criados");

        _pedidosAprovados = _meter.CreateCounter<long>(
            "ecommerce.pedidos.aprovados",
            unit: "pedidos",
            description: "Total de pedidos aprovados");

        _pedidosRejeitados = _meter.CreateCounter<long>(
            "ecommerce.pedidos.rejeitados",
            unit: "pedidos",
            description: "Total de pedidos rejeitados");
    }

    public void IncrementPedidoCriado(string servico, string clienteId)
    {
        _pedidosCriados.Add(1,
            new KeyValuePair<string, object?>("servico", servico),
            new KeyValuePair<string, object?>("cliente_id", clienteId));
    }

    public void IncrementPedidoAprovado(string servico, string numeroPedido)
    {
        _pedidosAprovados.Add(1,
            new KeyValuePair<string, object?>("servico", servico),
            new KeyValuePair<string, object?>("numero_pedido", numeroPedido));
    }

    public void IncrementPedidoRejeitado(string servico, string numeroPedido, string motivo)
    {
        _pedidosRejeitados.Add(1,
            new KeyValuePair<string, object?>("servico", servico),
            new KeyValuePair<string, object?>("numero_pedido", numeroPedido),
            new KeyValuePair<string, object?>("motivo", motivo));
    }
}
