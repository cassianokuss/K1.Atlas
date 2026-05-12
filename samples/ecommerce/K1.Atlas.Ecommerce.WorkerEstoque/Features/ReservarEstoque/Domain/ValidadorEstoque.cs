using K1.Atlas.Domain.ResultPattern;
using K1.Atlas.Ecommerce.Contracts.Entities;

namespace K1.Atlas.Ecommerce.WorkerEstoque.Features.ReservarEstoque.Domain;

public static class ValidadorEstoque
{
    public static Result ValidarDisponibilidade(Produto produto, int quantidadeRequerida)
    {
        if (!produto.TemEstoque(quantidadeRequerida))
        {
            return Error.Validation(
                "ESTOQUE.INSUFICIENTE",
                $"Estoque insuficiente para produto {produto.Codigo}. Requerido: {quantidadeRequerida}, Disponível: {produto.EstoqueDisponivel}");
        }

        return Result.Success();
    }

    public static ResultT<Produto> ValidarProdutoExiste(Produto? produto, string produtoId)
    {
        if (produto == null)
        {
            return Error.NotFound(
                "PRODUTO.NOT_FOUND",
                $"Produto {produtoId} não encontrado");
        }

        return produto;
    }
}
