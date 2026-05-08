using MediatR;

namespace K1.Atlas.WorkerFiscal.Ecommerce.Commands;

public class EmitirNotaFiscal : IRequest<NotaFiscal>
{
    public string PedidoId { get; set; } = string.Empty;
    public string ReservaId { get; set; } = string.Empty;
}
