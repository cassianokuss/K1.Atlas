using K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce.Features.ValidarCredito.Domain;
using K1.Atlas.Ecommerce.Contracts.ValueObjects;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Domain;

public class DecisaoCreditoTest
{
    [Fact]
    public void Aprovar_ComScoreValido_DeveCriarResultadoAprovado()
    {
        // Arrange
        int scoreBureau = 750;
        decimal limiteDisponivel = 5000m;

        // Act
        var resultado = DecisaoCredito.Aprovar(scoreBureau, limiteDisponivel);

        // Assert
        Assert.True(resultado.Aprovado);
        Assert.Equal(scoreBureau, resultado.ScoreBureau);
        Assert.Equal(limiteDisponivel, resultado.LimiteDisponivel);
        Assert.Equal(string.Empty, resultado.MotivoRejeicao);
    }

    [Fact]
    public void Rejeitar_ComCreditoInsuficiente_DeveCriarResultadoReprovado()
    {
        // Arrange
        decimal limiteDisponivel = 1000m;
        decimal valorRequerido = 2000m;

        // Act
        var resultado = DecisaoCredito.Rejeitar(limiteDisponivel, valorRequerido);

        // Assert
        Assert.False(resultado.Aprovado);
        Assert.Equal(0, resultado.ScoreBureau);
        Assert.Equal(limiteDisponivel, resultado.LimiteDisponivel);
        Assert.Contains("insuficiente", resultado.MotivoRejeicao.ToLower());
        Assert.Contains(limiteDisponivel.ToString("C"), resultado.MotivoRejeicao); // Disponível
        Assert.Contains(valorRequerido.ToString("C"), resultado.MotivoRejeicao); // Necessário
    }

    [Fact]
    public void Rejeitar_DeveCriarMotivoComValoresFormatados()
    {
        // Arrange
        decimal limiteDisponivel = 500.50m;
        decimal valorRequerido = 1500.75m;

        // Act
        var resultado = DecisaoCredito.Rejeitar(limiteDisponivel, valorRequerido);

        // Assert
        Assert.NotNull(resultado.MotivoRejeicao);
        Assert.NotEmpty(resultado.MotivoRejeicao);
        Assert.Contains("Limite de crédito", resultado.MotivoRejeicao);
    }

    [Fact]
    public void Aprovar_ComScoreZero_DeveAindaAprovar()
    {
        // Arrange
        int scoreBureau = 0;
        decimal limiteDisponivel = 1000m;

        // Act
        var resultado = DecisaoCredito.Aprovar(scoreBureau, limiteDisponivel);

        // Assert
        Assert.True(resultado.Aprovado);
        Assert.Equal(0, resultado.ScoreBureau);
    }
}
