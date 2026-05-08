namespace K1.Atlas.WorkerFiscal.Ecommerce;

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
