namespace K1.Atlas.Ecommerce.Contracts.ValueObjects;

public class ResultadoValidacao
{
    public bool Aprovado { get; set; }
    public string MotivoRejeicao { get; set; } = string.Empty;
    public int ScoreBureau { get; set; }
    public decimal LimiteDisponivel { get; set; }
}
