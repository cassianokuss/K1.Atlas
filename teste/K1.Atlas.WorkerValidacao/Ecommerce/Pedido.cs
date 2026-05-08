namespace K1.Atlas.WorkerValidacao.Ecommerce;

public class Pedido
{
    public string Id { get; set; }
    
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

public enum StatusPedido
{
    Pendente,
    Aprovado,
    Rejeitado,
    EstoqueReservado,
    Concluido,
    Cancelado
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

public class Cliente
{
    public string Nome { get; set; } = string.Empty;
    public string CpfCnpj { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Endereco { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string Cep { get; set; } = string.Empty;
    public decimal LimiteCredito { get; set; }
    public decimal CreditoUtilizado { get; set; }
    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; }
}
