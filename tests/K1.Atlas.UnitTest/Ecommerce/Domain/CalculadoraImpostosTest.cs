using K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Features.EmitirNotaFiscal.Domain;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Domain;

public class CalculadoraImpostosTest
{
    [Fact]
    public void Calcular_ComValorPositivo_DeveRetornarImpostosCalculados()
    {
        // Arrange
        decimal valorBase = 1000m;

        // Act
        var result = CalculadoraImpostos.Calcular(valorBase);

        // Assert
        Assert.Equal(180m, result.ValorICMS); // 18%
        Assert.Equal(16.5m, result.ValorPIS); // 1.65%
        Assert.Equal(76m, result.ValorCOFINS); // 7.6%
        Assert.Equal(100m, result.ValorIPI); // 10%
        Assert.Equal(1372.5m, result.ValorTotal); // Base + impostos
    }

    [Fact]
    public void Calcular_ComValorZero_DeveRetornarImpostosZerados()
    {
        // Arrange
        decimal valorBase = 0m;

        // Act
        var result = CalculadoraImpostos.Calcular(valorBase);

        // Assert
        Assert.Equal(0m, result.ValorICMS);
        Assert.Equal(0m, result.ValorPIS);
        Assert.Equal(0m, result.ValorCOFINS);
        Assert.Equal(0m, result.ValorIPI);
        Assert.Equal(0m, result.ValorTotal);
    }

    [Fact]
    public void CalcularPorItem_ComValorPositivo_DeveRetornarImpostosItem()
    {
        // Arrange
        decimal valorItem = 500m;

        // Act
        var result = CalculadoraImpostos.CalcularPorItem(valorItem);

        // Assert
        Assert.Equal(90m, result.ValorICMS); // 18%
        Assert.Equal(8.25m, result.ValorPIS); // 1.65%
        Assert.Equal(38m, result.ValorCOFINS); // 7.6%
        Assert.Equal(50m, result.ValorIPI); // 10%
    }

    [Fact]
    public void CalcularPorItem_ComValorZero_DeveRetornarImpostosZerados()
    {
        // Arrange
        decimal valorItem = 0m;

        // Act
        var result = CalculadoraImpostos.CalcularPorItem(valorItem);

        // Assert
        Assert.Equal(0m, result.ValorICMS);
        Assert.Equal(0m, result.ValorPIS);
        Assert.Equal(0m, result.ValorCOFINS);
        Assert.Equal(0m, result.ValorIPI);
    }

    [Fact]
    public void Calcular_DeveArredondarParaDuasCasasDecimais()
    {
        // Arrange
        decimal valorBase = 123.45m;

        // Act
        var result = CalculadoraImpostos.Calcular(valorBase);

        // Assert - 123.45 * percentual arredondado para 2 decimais
        Assert.Equal(22.22m, result.ValorICMS); // 123.45 * 0.18
        Assert.Equal(2.04m, result.ValorPIS); // 123.45 * 0.0165
        Assert.Equal(9.38m, result.ValorCOFINS); // 123.45 * 0.076
        Assert.Equal(12.34m, result.ValorIPI); // 123.45 * 0.10 = 12.345 arredondado para 12.34
    }

    [Fact]
    public void ObterAliquotaICMS_DeveRetornar18Porcento()
    {
        // Act
        var aliquota = CalculadoraImpostos.ObterAliquotaICMS();

        // Assert
        Assert.Equal(0.18m, aliquota);
    }
}
