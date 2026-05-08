using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce;

public class EstoqueExceptionTest
{
    [Fact]
    public void EstoqueInsuficienteException_Constructor_Should_Set_Properties()
    {
        // Arrange
        var produtoCodigo = "PROD-001";
        var quantidadeRequerida = 10;
        var quantidadeDisponivel = 5;

        // Act
        var exception = new K1.Atlas.WorkerEstoque.Ecommerce.Exceptions.EstoqueInsuficienteException(
            produtoCodigo, 
            quantidadeRequerida, 
            quantidadeDisponivel);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(produtoCodigo, exception.ProdutoCodigo);
        Assert.Equal(quantidadeRequerida, exception.QuantidadeRequerida);
        Assert.Equal(quantidadeDisponivel, exception.QuantidadeDisponivel);
        Assert.Contains(produtoCodigo, exception.Message);
        Assert.Contains(quantidadeRequerida.ToString(), exception.Message);
        Assert.Contains(quantidadeDisponivel.ToString(), exception.Message);
    }

    [Fact]
    public void EstoqueInsuficienteException_Should_Derive_From_Exception()
    {
        // Arrange
        var exception = new K1.Atlas.WorkerEstoque.Ecommerce.Exceptions.EstoqueInsuficienteException(
            "PROD-001", 
            10, 
            5);

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void ProdutoNaoEncontradoException_Constructor_Should_Set_Properties()
    {
        // Arrange
        var produtoId = "507f1f77bcf86cd799439011";
        var customMessage = "Produto não encontrado no estoque";

        // Act
        var exception = new K1.Atlas.WorkerEstoque.Ecommerce.Exceptions.ProdutoNaoEncontradoException(
            produtoId, 
            customMessage);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(produtoId, exception.ProdutoId);
        Assert.Contains(produtoId, exception.Message);
        Assert.Contains(customMessage, exception.Message);
    }

    [Fact]
    public void ProdutoNaoEncontradoException_Should_Derive_From_Exception()
    {
        // Arrange
        var exception = new K1.Atlas.WorkerEstoque.Ecommerce.Exceptions.ProdutoNaoEncontradoException(
            "507f1f77bcf86cd799439011", 
            "Produto não encontrado");

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void ProdutoNaoEncontradoException_Constructor_With_Only_ProdutoId_Should_Use_Default_Message()
    {
        // Arrange
        var produtoId = "507f1f77bcf86cd799439011";

        // Act
        var exception = new K1.Atlas.WorkerEstoque.Ecommerce.Exceptions.ProdutoNaoEncontradoException(produtoId);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(produtoId, exception.ProdutoId);
        Assert.Contains(produtoId, exception.Message);
        Assert.Contains("não encontrado", exception.Message.ToLower());
    }
}
