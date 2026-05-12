namespace K1.Atlas.Ecommerce.Contracts.Entities;

public class Pedido
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string NumeroPedido { get; set; } = default!;
    public string ClienteId { get; set; } = default!;
    
    public Cliente? Cliente { get; set; }
    
    public List<ItemPedido> Itens { get; set; } = new();
    public decimal ValorProdutos { get; set; }
    public decimal ValorFrete { get; set; }
    public decimal ValorTotal { get; set; }
    public StatusPedido Status { get; set; }
    public string? MotivoRejeicao { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAprovacao { get; set; }
    public DateTime? DataConclusao { get; set; }

    public void CalcularTotais()
    {
        ValorProdutos = Itens.Sum(i => i.Subtotal);
        ValorTotal = ValorProdutos + ValorFrete;
    }
}
