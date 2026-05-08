using MongoDB.Bson;

namespace K1.Atlas.WorkerEstoque.Ecommerce;

public class ReservaEstoque
{
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    public string PedidoId { get; set; } = default!;
    public string ClienteId { get; set; } = default!;
    public List<ItemReservado> Itens { get; set; } = new();
    public StatusReserva Status { get; set; } = StatusReserva.Ativa;
    public DateTime DataReserva { get; set; } = DateTime.UtcNow;
    public DateTime DataExpiracao { get; set; } = DateTime.UtcNow.AddHours(24);
}

public class ItemReservado
{
    public string ProdutoId { get; set; } = default!;
    public int Quantidade { get; set; }
    public int QuantidadeReservada { get; set; }
}

public enum StatusReserva
{
    Ativa,
    Liberada,
    Expirada
}
