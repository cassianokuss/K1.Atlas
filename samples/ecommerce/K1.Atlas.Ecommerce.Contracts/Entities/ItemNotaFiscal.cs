namespace K1.Atlas.Ecommerce.Contracts.Entities;

public class ItemNotaFiscal
{
    public string ProdutoId { get; set; } = string.Empty;
    public string CodigoProduto { get; set; } = string.Empty;
    public string DescricaoProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal ValorTotal { get; set; }
    public decimal AliquotaICMS { get; set; }
    public decimal ValorICMS { get; set; }
    public decimal ValorPIS { get; set; }
    public decimal ValorCOFINS { get; set; }
    public decimal ValorIPI { get; set; }
}
