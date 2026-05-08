namespace K1.Atlas.WorkerFiscal.Ecommerce.Exceptions;

/// <summary>
/// Exception thrown when there is an error during invoice emission
/// </summary>
public class EmissaoNotaFiscalException : Exception
{
    public string PedidoId { get; }

    public EmissaoNotaFiscalException(string pedidoId)
        : base($"Erro na emissão da nota fiscal para o pedido {pedidoId}")
    {
        PedidoId = pedidoId;
    }

    public EmissaoNotaFiscalException(string pedidoId, string message)
        : base($"Erro na emissão da nota fiscal para o pedido {pedidoId}: {message}")
    {
        PedidoId = pedidoId;
    }

    public EmissaoNotaFiscalException(
        string pedidoId, 
        string message, 
        Exception innerException)
        : base($"Erro na emissão da nota fiscal para o pedido {pedidoId}: {message}", innerException)
    {
        PedidoId = pedidoId;
    }
}
