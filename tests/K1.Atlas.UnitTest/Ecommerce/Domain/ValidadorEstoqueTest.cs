using K1.Atlas.Ecommerce.WorkerEstoque.Features.ReservarEstoque.Domain;
using K1.Atlas.Ecommerce.WorkerEstoque.Exceptions;
using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Domain.ResultPattern;
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

        // Act
        var result = ValidadorEstoque.ValidarDisponibilidade(produto, quantidadeSolicitada);
        
        // Assert
        Assert.True(result.IsSuccess);
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

        // Act
        var result = ValidadorEstoque.ValidarDisponibilidade(produto, quantidadeSolicitada);
        
        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void ValidarDisponibilidade_ComEstoqueInsuficiente_DeveRetornarErro()
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

        // Act
        var result = ValidadorEstoque.ValidarDisponibilidade(produto, quantidadeSolicitada);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.ErrorType);
        Assert.Equal("ESTOQUE.INSUFICIENTE", result.Error.Code);
        Assert.Contains("COD1", result.Error.Description);
        Assert.Contains("5", result.Error.Description);
        Assert.Contains("3", result.Error.Description);
    }

    [Fact]
    public void ValidarDisponibilidade_ComEstoqueZero_DeveRetornarErro()
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

        // Act
        var result = ValidadorEstoque.ValidarDisponibilidade(produto, quantidadeSolicitada);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.ErrorType);
        Assert.Equal("ESTOQUE.INSUFICIENTE", result.Error.Code);
        Assert.Contains("COD1", result.Error.Description);
        Assert.Contains("1", result.Error.Description);
        Assert.Contains("0", result.Error.Description);
    }

    [Fact]
    public void ValidarProdutoExiste_ComProdutoValido_DeveRetornarProduto()
    {
        // Arrange
        var produto = new Produto
        {
            Id = "PROD1",
            Codigo = "COD1",
            Descricao = "Produto Teste"
        };

        // Act
        var result = ValidadorEstoque.ValidarProdutoExiste(produto, "PROD1");
        
        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("PROD1", result.Value.Id);
    }

    [Fact]
    public void ValidarProdutoExiste_ComProdutoNulo_DeveRetornarErro()
    {
        // Arrange
        Produto? produto = null;
        string produtoId = "PROD1";

        // Act
        var result = ValidadorEstoque.ValidarProdutoExiste(produto, produtoId);
        
        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.ErrorType);
        Assert.Equal("PRODUTO.NOT_FOUND", result.Error.Code);
        Assert.Contains("PROD1", result.Error.Description);
    }
}
