using K1.Atlas.TesteApi.Cadastros;

namespace K1.Atlas.TesteApi.NotasFiscais;

public class NFSe
{
    public int Numero { get; set; }
    public Contribuinte Contribuinte { get; set; } = default!;
    public decimal Valor { get; set; }
}