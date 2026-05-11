namespace K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce;

public class Produto
{
    public string Id { get; set; }
    
    public string Codigo { get; set; } = default!;
    public string Descricao { get; set; } = default!;
    public decimal ValorUnitario { get; set; }
    public int EstoqueDisponivel { get; set; }
    public decimal AliquotaICMS { get; set; }
    public bool CalculaIPI { get; set; }
    public bool Ativo { get; set; }

    public bool TemEstoque(int quantidade)
    {
        return EstoqueDisponivel >= quantidade;
    }
}
