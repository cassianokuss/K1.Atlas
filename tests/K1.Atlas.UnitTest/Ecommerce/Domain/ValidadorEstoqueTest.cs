using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Features.ReservarEstoque.Domain;
using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Exceptions;
using K1.Atlas.Ecommerce.Contracts.Entities;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Domain;

public class ValidadorEstoqueTest
{
    [Fact]
    public void ValidarDisponibilidade_ComEstoqueSuficiente_DevePassar()
    {
        // Arrange
        var produto = new Produto
        {
            Id = "PROD1",
            Codigo = "COD1",
            Descricao = "Produto Teste",
            ValorUnitario = 100m,
            EstoqueDisponivel = 10
        };
        int quantidadeSolicitada = 5;

        // Act & Assert - should not throw
        ValidadorEstoque.ValidarDisponibilidade(produto, quantidadeSolicitada);
    }

    [Fact]
    public void ValidarDisponibilidade_ComEstoqueExato_DevePassar()
    {
        // Arrange
        var produto = new Produto
        {
            Id = "PROD1",
            Codigo = "COD1",
            Descricao = "Produto Teste",
            ValorUnitario = 100m,
            EstoqueDisponivel = 5
        };
        int quantidadeSolicitada = 5;

        // Act & Assert - should not throw
        ValidadorEstoque.ValidarDisponibilidade(produto, quantidadeSolicitada);
    }

    [Fact]
    public void ValidarDisponibilidade_ComEstoqueInsuficiente_DeveLancarExcecao()
    {
        // Arrange
        var produto = new Produto
        {
            Id = "PROD1",
            Codigo = "COD1",
            Descricao = "Produto Teste",
            ValorUnitario = 100m,
            EstoqueDisponivel = 3
        };
        int quantidadeSolicitada = 5;

        // Act & Assert
        var exception = Assert.Throws<EstoqueInsuficienteException>(() =>
            ValidadorEstoque.ValidarDisponibilidade(produto, quantidadeSolicitada));
        
        Assert.Equal("COD1", exception.ProdutoCodigo);
        Assert.Equal(5, exception.QuantidadeRequerida);
        Assert.Equal(3, exception.QuantidadeDisponivel);
    }

    [Fact]
    public void ValidarDisponibilidade_ComEstoqueZero_DeveLancarExcecao()
    {
        // Arrange
        var produto = new Produto
        {
            Id = "PROD1",
            Codigo = "COD1",
            Descricao = "Produto Teste",
            ValorUnitario = 100m,
            EstoqueDisponivel = 0
        };
        int quantidadeSolicitada = 1;

        // Act & Assert
        var exception = Assert.Throws<EstoqueInsuficienteException>(() =>
            ValidadorEstoque.ValidarDisponibilidade(produto, quantidadeSolicitada));
        
        Assert.Equal("COD1", exception.ProdutoCodigo);
        Assert.Equal(1, exception.QuantidadeRequerida);
        Assert.Equal(0, exception.QuantidadeDisponivel);
    }

    [Fact]
    public void ValidarProdutoExiste_ComProdutoValido_DevePassar()
    {
        // Arrange
        var produto = new Produto
        {
            Id = "PROD1",
            Codigo = "COD1",
            Descricao = "Produto Teste"
        };

        // Act & Assert - should not throw
        ValidadorEstoque.ValidarProdutoExiste(produto, "PROD1");
    }

    [Fact]
    public void ValidarProdutoExiste_ComProdutoNulo_DeveLancarExcecao()
    {
        // Arrange
        Produto? produto = null;
        string produtoId = "PROD1";

        // Act & Assert
        var exception = Assert.Throws<ProdutoNaoEncontradoException>(() =>
            ValidadorEstoque.ValidarProdutoExiste(produto, produtoId));
        
        Assert.Equal("PROD1", exception.ProdutoId);
    }
}
