using K1.Atlas.Ecommerce.Api.Ecommerce;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce;

public class ProdutoTest
{
    [Fact]
    public void Constructor_Should_Initialize_Properties()
    {
        var produto = new Produto
        {
            Codigo = "PROD001",
            Descricao = "Notebook Dell",
            ValorUnitario = 3500.00m,
            EstoqueDisponivel = 10,
            AliquotaICMS = 18m,
            CalculaIPI = false,
            Ativo = true
        };

        Assert.Equal("PROD001", produto.Codigo);
        Assert.Equal("Notebook Dell", produto.Descricao);
        Assert.Equal(3500.00m, produto.ValorUnitario);
        Assert.Equal(10, produto.EstoqueDisponivel);
        Assert.Equal(18m, produto.AliquotaICMS);
        Assert.False(produto.CalculaIPI);
        Assert.True(produto.Ativo);
    }

    [Fact]
    public void TemEstoque_Should_Return_True_When_Stock_Available()
    {
        var produto = new Produto { EstoqueDisponivel = 10 };
        Assert.True(produto.TemEstoque(5));
    }

    [Fact]
    public void TemEstoque_Should_Return_False_When_Stock_Insufficient()
    {
        var produto = new Produto { EstoqueDisponivel = 3 };
        Assert.False(produto.TemEstoque(5));
    }
}
