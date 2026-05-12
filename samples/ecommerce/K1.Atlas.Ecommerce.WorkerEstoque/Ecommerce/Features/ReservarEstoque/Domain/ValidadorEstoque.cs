using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Exceptions;

namespace K1.Atlas.Ecommerce.WorkerEstoque.Ecommerce.Features.ReservarEstoque.Domain;

public static class ValidadorEstoque
{
    public static void ValidarDisponibilidade(Produto produto, int quantidadeRequerida)
    {
        if (!produto.TemEstoque(quantidadeRequerida))
        {
            throw new EstoqueInsuficienteException(
                produto.Codigo,
                quantidadeRequerida,
                produto.EstoqueDisponivel);
        }
    }

    public static void ValidarProdutoExiste(Produto? produto, string produtoId)
    {
        if (produto == null)
        {
            throw new ProdutoNaoEncontradoException(produtoId);
        }
    }
}
