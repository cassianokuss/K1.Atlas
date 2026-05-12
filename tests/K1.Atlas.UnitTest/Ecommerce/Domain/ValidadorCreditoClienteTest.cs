using K1.Atlas.Ecommerce.WorkerValidacao.Features.ValidarCredito.Domain;
using K1.Atlas.Ecommerce.Contracts.Entities;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Domain;

public class ValidadorCreditoClienteTest
{
    [Fact]
    public void ValidarLimiteCredito_ComCreditoSuficiente_DeveRetornarTrue()
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = "CLI123",
            Nome = "Cliente Teste",
            LimiteCredito = 1000m,
            CreditoUtilizado = 200m
        };
        decimal valorPedido = 500m;

        // Act
        var resultado = ValidadorCreditoCliente.TemLimite(cliente, valorPedido);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void ValidarLimiteCredito_ComCreditoExato_DeveRetornarTrue()
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = "CLI123",
            Nome = "Cliente Teste",
            LimiteCredito = 1000m,
            CreditoUtilizado = 500m
        };
        decimal valorPedido = 500m;

        // Act
        var resultado = ValidadorCreditoCliente.TemLimite(cliente, valorPedido);

        // Assert
        Assert.True(resultado);
    }

    [Fact]
    public void ValidarLimiteCredito_ComCreditoInsuficiente_DeveRetornarFalse()
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = "CLI123",
            Nome = "Cliente Teste",
            LimiteCredito = 1000m,
            CreditoUtilizado = 700m
        };
        decimal valorPedido = 400m;

        // Act
        var resultado = ValidadorCreditoCliente.TemLimite(cliente, valorPedido);

        // Assert
        Assert.False(resultado);
    }

    [Fact]
    public void ValidarLimiteCredito_ComLimiteZero_DeveRetornarFalse()
    {
        // Arrange
        var cliente = new Cliente
        {
            Id = "CLI123",
            Nome = "Cliente Teste",
            LimiteCredito = 0m,
            CreditoUtilizado = 0m
        };
        decimal valorPedido = 100m;

        // Act
        var resultado = ValidadorCreditoCliente.TemLimite(cliente, valorPedido);

        // Assert
        Assert.False(resultado);
    }
}
