namespace K1.Atlas.Ecommerce.Contracts.Entities;

public class ReservaEstoque
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    public string PedidoId { get; set; } = default!;
    public string ClienteId { get; set; } = default!;
    public List<ItemReservado> Itens { get; set; } = new();
    public StatusReserva Status { get; set; } = StatusReserva.Ativa;
    public DateTime DataReserva { get; set; } = DateTime.UtcNow;
    public DateTime DataExpiracao { get; set; } = DateTime.UtcNow.AddHours(24);
}
