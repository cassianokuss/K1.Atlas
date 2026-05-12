namespace K1.Atlas.Ecommerce.WorkerEstoque.Exceptions;

/// <summary>
/// Exception thrown when a product is not found
/// </summary>
public class ProdutoNaoEncontradoException : Exception
{
    public string ProdutoId { get; }

    public ProdutoNaoEncontradoException(string produtoId)
        : base($"Produto não encontrado: {produtoId}")
    {
        ProdutoId = produtoId;
    }

    public ProdutoNaoEncontradoException(string produtoId, string message)
        : base($"{message}: {produtoId}")
    {
        ProdutoId = produtoId;
    }

    public ProdutoNaoEncontradoException(string produtoId, string message, Exception innerException)
        : base($"{message}: {produtoId}", innerException)
    {
        ProdutoId = produtoId;
    }
}
