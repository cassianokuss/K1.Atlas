namespace K1.Atlas.Ecommerce.Contracts.Entities;

public class ItemPedido
{
    public string ProdutoId { get; set; } = default!;
    public string CodigoProduto { get; set; } = default!;
    public string DescricaoProduto { get; set; } = default!;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }

    public void CalcularSubtotal()
    {
        Subtotal = Quantidade * ValorUnitario;
    }
}
