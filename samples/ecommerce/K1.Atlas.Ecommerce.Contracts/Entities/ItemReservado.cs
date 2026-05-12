namespace K1.Atlas.Ecommerce.Contracts.Entities;

public class ItemReservado
{
    public string ProdutoId { get; set; } = default!;
    public int Quantidade { get; set; }
    public int QuantidadeReservada { get; set; }
}
