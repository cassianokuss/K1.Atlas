using System.Diagnostics.Metrics;
using K1.Atlas.Telemetry;

namespace K1.Atlas.Ecommerce.Api.Ecommerce;

public class PedidoMetrics
{
    private readonly Counter<long> _pedidosCriados;

    public PedidoMetrics()
    {
        _pedidosCriados = MetricsRegistry.TesteApiPedidos.CreateCounter<long>(
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
