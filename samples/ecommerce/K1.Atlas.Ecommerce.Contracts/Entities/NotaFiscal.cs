namespace K1.Atlas.Ecommerce.Contracts.Entities;

public class NotaFiscal
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string Numero { get; set; } = string.Empty;
    public string Serie { get; set; } = string.Empty;
    public string PedidoId { get; set; } = string.Empty;
    public string ClienteId { get; set; } = string.Empty;
    public string ChaveAcesso { get; set; } = string.Empty;
    
    public Cliente? Cliente { get; set; }
    
    public List<ItemNotaFiscal> Itens { get; set; } = new();
    public decimal ValorProdutos { get; set; }
    public decimal ValorICMS { get; set; }
    public decimal ValorPIS { get; set; }
    public decimal ValorCOFINS { get; set; }
    public decimal ValorIPI { get; set; }
    public decimal ValorTotal { get; set; }
    public DateTime DataEmissao { get; set; }
    public string? ProtocoloAutorizacao { get; set; }
    public StatusNotaFiscal Status { get; set; }
    public int TentativasEnvio { get; set; }
}
