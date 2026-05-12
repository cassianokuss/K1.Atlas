using K1.Atlas.Ecommerce.Api.Ecommerce;
using K1.Atlas.Ecommerce.Contracts.Entities;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce;

public class ClienteTest
{
    [Fact]
    public void Constructor_Should_Initialize_Properties()
    {
        // Arrange & Act
        var cliente = new Cliente
        {
            Nome = "João Silva",
            CpfCnpj = "12345678901",
            Email = "joao@example.com",
            Endereco = "Rua A, 100",
            Cidade = "São Paulo",
            Estado = "SP",
            Cep = "01310-100",
            LimiteCredito = 10000m,
            CreditoUtilizado = 2000m,
            DataCadastro = DateTime.Now,
            Ativo = true
        };

        // Assert
        Assert.Equal("João Silva", cliente.Nome);
        Assert.Equal("12345678901", cliente.CpfCnpj);
        Assert.Equal("joao@example.com", cliente.Email);
        Assert.Equal(10000m, cliente.LimiteCredito);
        Assert.Equal(2000m, cliente.CreditoUtilizado);
        Assert.True(cliente.Ativo);
    }

    [Fact]
    public void CreditoDisponivel_Should_Calculate_Correctly()
    {
        // Arrange
        var cliente = new Cliente
        {
            LimiteCredito = 10000m,
            CreditoUtilizado = 3000m
        };

        // Act
        var creditoDisponivel = cliente.CreditoDisponivel;

        // Assert
        Assert.Equal(7000m, creditoDisponivel);
    }
}
