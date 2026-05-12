namespace K1.Atlas.Ecommerce.WorkerEstoque.Exceptions;

/// <summary>
/// Exception thrown when there is insufficient stock for a product
/// </summary>
public class EstoqueInsuficienteException : Exception
{
    public string ProdutoCodigo { get; }
    public int QuantidadeRequerida { get; }
    public int QuantidadeDisponivel { get; }

    public EstoqueInsuficienteException(
        string produtoCodigo, 
        int quantidadeRequerida, 
        int quantidadeDisponivel)
        : base($"Estoque insuficiente para o produto {produtoCodigo}. Requerido: {quantidadeRequerida}, Disponível: {quantidadeDisponivel}")
    {
        ProdutoCodigo = produtoCodigo;
        QuantidadeRequerida = quantidadeRequerida;
        QuantidadeDisponivel = quantidadeDisponivel;
    }

    public EstoqueInsuficienteException(
        string produtoCodigo, 
        int quantidadeRequerida, 
        int quantidadeDisponivel, 
        Exception innerException)
        : base($"Estoque insuficiente para o produto {produtoCodigo}. Requerido: {quantidadeRequerida}, Disponível: {quantidadeDisponivel}", innerException)
    {
        ProdutoCodigo = produtoCodigo;
        QuantidadeRequerida = quantidadeRequerida;
        QuantidadeDisponivel = quantidadeDisponivel;
    }
}
