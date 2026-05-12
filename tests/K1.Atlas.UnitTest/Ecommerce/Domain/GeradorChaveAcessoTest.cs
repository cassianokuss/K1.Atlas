using K1.Atlas.Ecommerce.WorkerFiscal.Features.EmitirNotaFiscal.Domain;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Domain;

public class GeradorChaveAcessoTest
{
    [Fact]
    public void Gerar_DeveRetornar44Caracteres()
    {
        // Act
        var chave = GeradorChaveAcesso.Gerar();

        // Assert
        Assert.Equal(44, chave.Length);
    }

    [Fact]
    public void Gerar_DeveRetornarApenasNumeros()
    {
        // Act
        var chave = GeradorChaveAcesso.Gerar();

        // Assert
        Assert.All(chave, c => Assert.True(char.IsDigit(c)));
    }

    [Fact]
    public void Gerar_DevemGerarChavesDiferentes()
    {
        // Act
        var chave1 = GeradorChaveAcesso.Gerar();
        var chave2 = GeradorChaveAcesso.Gerar();

        // Assert - Com alta probabilidade, chaves devem ser diferentes
        // (tecnicamente poderiam ser iguais, mas probabilidade é extremamente baixa)
        Assert.NotEqual(chave1, chave2);
    }

    [Fact]
    public void Gerar_DeveGerarChaveComFormatoValido()
    {
        // Act
        var chave = GeradorChaveAcesso.Gerar();

        // Assert
        Assert.Matches("^[0-9]{44}$", chave);
    }
}
