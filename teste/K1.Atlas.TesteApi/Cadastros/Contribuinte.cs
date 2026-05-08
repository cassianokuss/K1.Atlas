namespace K1.Atlas.TesteApi.Cadastros;

public class Contribuinte
{
    public string NumDocReceita { get; set; } = null!;
    public string Nome { get; set; } = null!;
    public string Endereco { get; set; } = null!;
    public DateTime DataCadastro { get; set; } = default!;
    public decimal Valor { get; set; } = 0;
}
