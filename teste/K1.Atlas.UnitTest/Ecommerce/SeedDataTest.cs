using K1.Atlas.TesteApi.Ecommerce;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce;

public class SeedDataTest
{
    [Fact]
    public void GetClientes_Should_Return_Three_Clients()
    {
        var clientes = SeedData.GetClientes();

        Assert.Equal(3, clientes.Count);
        Assert.Contains(clientes, c => c.Nome.Contains("Bom Pagador"));
        Assert.Contains(clientes, c => c.Nome.Contains("Limite Baixo"));
        Assert.Contains(clientes, c => c.Nome.Contains("Inadimplente"));
    }

    [Fact]
    public void GetProdutos_Should_Return_Five_Products()
    {
        var produtos = SeedData.GetProdutos();

        Assert.Equal(5, produtos.Count);
        Assert.All(produtos, p => Assert.NotNull(p.Codigo));
        Assert.All(produtos, p => Assert.True(p.ValorUnitario > 0));
        Assert.All(produtos, p => Assert.True(p.EstoqueDisponivel >= 0));
    }

    [Fact]
    public void GetClientes_Should_Have_Different_Credit_Profiles()
    {
        var clientes = SeedData.GetClientes();

        var bomPagador = clientes.First(c => c.Nome.Contains("Bom Pagador"));
        var limiteBaixo = clientes.First(c => c.Nome.Contains("Limite Baixo"));

        Assert.True(bomPagador.CreditoDisponivel > limiteBaixo.CreditoDisponivel);
    }
}
