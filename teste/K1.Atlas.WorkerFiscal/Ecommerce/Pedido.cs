namespace K1.Atlas.WorkerFiscal.Ecommerce;

public class Pedido
{
    public string Id { get; set; }
    
    public string NumeroPedido { get; set; } = string.Empty;
    public string ClienteId { get; set; } = string.Empty;
    
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

public class ItemPedido
{
    public string ProdutoId { get; set; } = string.Empty;
    public string CodigoProduto { get; set; } = string.Empty;
    public string DescricaoProduto { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal ValorUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public enum StatusPedido
{
    Pendente,
    Aprovado,
    Rejeitado,
    EstoqueReservado,
    Concluido,
    Cancelado
}
