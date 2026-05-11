namespace K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Events;

public class NotaFiscalEmitida
{
    public string PedidoId { get; set; } = string.Empty;
    public string NotaFiscalId { get; set; } = string.Empty;
    public string NumeroNotaFiscal { get; set; } = string.Empty;
    public string ChaveAcesso { get; set; } = string.Empty;
    public string ProtocoloAutorizacao { get; set; } = string.Empty;
    public DateTime DataEmissao { get; set; }
    public decimal ValorTotal { get; set; }
}
