using System.Diagnostics.Metrics;

namespace K1.Atlas.Ecommerce.Api.Ecommerce;

public class PedidoMetrics
{
    private static readonly Meter _meter = new("K1.Atlas.Ecommerce.Api.Pedidos", "1.0.0");
    private readonly Counter<long> _pedidosCriados;

    public PedidoMetrics()
    {
        _pedidosCriados = _meter.CreateCounter<long>(
            "ecommerce.pedidos.criados",
            unit: "pedidos",
            description: "Total de pedidos criados");
    }

    public void IncrementPedidoCriado(string servico, string clienteId)
    {
        _pedidosCriados.Add(1,
            new KeyValuePair<string, object?>("servico", servico),
            new KeyValuePair<string, object?>("cliente_id", clienteId));
    }
}
