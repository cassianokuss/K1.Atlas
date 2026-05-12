using K1.Atlas.Ecommerce.Contracts.Entities;
using K1.Atlas.Ecommerce.Contracts.Events;
using K1.Atlas.Ecommerce.WorkerFiscal.Features.EmitirNotaFiscal.Domain;

namespace K1.Atlas.Ecommerce.WorkerFiscal.Features.EmitirNotaFiscal.Mappers;

public static class NotaFiscalMapper
{
    public static NotaFiscal CriarNotaFiscal(
        Pedido pedido,
        Cliente cliente,
        ImpostosCalculados impostos,
        string chaveAcesso,
        string numeroNota)
    {
        return new NotaFiscal
        {
            PedidoId = pedido.Id,
            ClienteId = pedido.ClienteId,
            Cliente = cliente,
            ChaveAcesso = chaveAcesso,
            Numero = numeroNota,
            Serie = "1",
            DataEmissao = DateTime.UtcNow,
            Status = StatusNotaFiscal.Processando,
            ValorProdutos = pedido.ValorProdutos,
            ValorICMS = impostos.ValorICMS,
            ValorPIS = impostos.ValorPIS,
            ValorCOFINS = impostos.ValorCOFINS,
            ValorIPI = impostos.ValorIPI,
            ValorTotal = impostos.ValorTotal,
            Itens = pedido.Itens.Select(MapearItem).ToList()
        };
    }

    private static ItemNotaFiscal MapearItem(ItemPedido item)
    {
        var impostosItem = CalculadoraImpostos.CalcularPorItem(item.Subtotal);
        
        return new ItemNotaFiscal
        {
            ProdutoId = item.ProdutoId,
            CodigoProduto = item.CodigoProduto,
            DescricaoProduto = item.DescricaoProduto,
            Quantidade = item.Quantidade,
            ValorUnitario = item.ValorUnitario,
            ValorTotal = item.Subtotal,
            AliquotaICMS = CalculadoraImpostos.ObterAliquotaICMS(),
            ValorICMS = impostosItem.ValorICMS,
            ValorPIS = impostosItem.ValorPIS,
            ValorCOFINS = impostosItem.ValorCOFINS,
            ValorIPI = impostosItem.ValorIPI
        };
    }

    public static NotaFiscalEmitida ParaEvento(NotaFiscal notaFiscal, string protocolo)
    {
        return new NotaFiscalEmitida
        {
            PedidoId = notaFiscal.PedidoId,
            NotaFiscalId = notaFiscal.Id,
            NumeroNotaFiscal = notaFiscal.Numero,
            ChaveAcesso = notaFiscal.ChaveAcesso,
            ProtocoloAutorizacao = protocolo,
            DataEmissao = notaFiscal.DataEmissao,
            ValorTotal = notaFiscal.ValorTotal
        };
    }
}
