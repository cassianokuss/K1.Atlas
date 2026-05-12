using K1.Atlas.Ecommerce.WorkerFiscal.Features.EmitirNotaFiscal.Domain;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Domain;

public class GeradorNumeroNotaTest
{
    [Fact]
    public void Gerar_DeveRetornarStringDe17Caracteres()
    {
        // Arrange & Act
        var numero = GeradorNumeroNota.Gerar();

        // Assert
        Assert.Equal(17, numero.Length);
    }

    [Fact]
    public void Gerar_DeveRetornarApenasNumeros()
    {
        // Arrange & Act
        var numero = GeradorNumeroNota.Gerar();

        // Assert
        Assert.All(numero, c => Assert.True(char.IsDigit(c)));
    }

    [Fact]
    public void Gerar_DevemGerarNumerosDiferentes()
    {
        // Arrange & Act
        var numero1 = GeradorNumeroNota.Gerar();
        System.Threading.Thread.Sleep(1); // Ensure different milliseconds
        var numero2 = GeradorNumeroNota.Gerar();

        // Assert
        Assert.NotEqual(numero1, numero2);
    }

    [Fact]
    public void Gerar_DeveGerarNumeroComFormatoTimestamp()
    {
        // Arrange & Act
        var numero = GeradorNumeroNota.Gerar();

        // Assert - Deve começar com ano atual (20XX)
        Assert.StartsWith(DateTime.UtcNow.Year.ToString(), numero);
    }
}
