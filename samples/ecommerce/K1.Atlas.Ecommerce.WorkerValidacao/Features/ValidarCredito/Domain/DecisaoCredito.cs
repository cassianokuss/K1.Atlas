using K1.Atlas.Ecommerce.Contracts.ValueObjects;

namespace K1.Atlas.Ecommerce.WorkerValidacao.Features.ValidarCredito.Domain;

public static class DecisaoCredito
{
    public static ResultadoValidacao Aprovar(int scoreBureau, decimal limiteDisponivel)
    {
        return new ResultadoValidacao
        {
            Aprovado = true,
            ScoreBureau = scoreBureau,
            LimiteDisponivel = limiteDisponivel,
            MotivoRejeicao = string.Empty
        };
    }

    public static ResultadoValidacao Rejeitar(decimal limiteDisponivel, decimal valorRequerido, int scoreBureau = 0)
    {
        return new ResultadoValidacao
        {
            Aprovado = false,
            ScoreBureau = scoreBureau,
            LimiteDisponivel = limiteDisponivel,
            MotivoRejeicao = $"Limite de crédito insuficiente. Disponível: {limiteDisponivel:C}, Necessário: {valorRequerido:C}"
        };
    }
}
