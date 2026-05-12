namespace K1.Atlas.Ecommerce.Contracts.Entities;

public class Cliente
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Nome { get; set; } = default!;
    public string CpfCnpj { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Endereco { get; set; } = default!;
    public string Cidade { get; set; } = default!;
    public string Estado { get; set; } = default!;
    public string Cep { get; set; } = default!;
    public decimal LimiteCredito { get; set; }
    public decimal CreditoUtilizado { get; set; }
    public DateTime DataCadastro { get; set; }
    public bool Ativo { get; set; }

    public decimal CreditoDisponivel => LimiteCredito - CreditoUtilizado;
}
