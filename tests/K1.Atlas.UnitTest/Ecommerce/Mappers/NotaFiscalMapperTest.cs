using K1.Atlas.Ecommerce.WorkerFiscal.Features.EmitirNotaFiscal.Mappers;
using K1.Atlas.Ecommerce.WorkerFiscal.Features.EmitirNotaFiscal.Domain;
using K1.Atlas.Ecommerce.Contracts.Entities;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce.Mappers;

public class NotaFiscalMapperTest
{
    [Fact]
    public void CriarNotaFiscal_ComPedidoValido_DeveCriarNotaFiscalCompleta()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "PED123",
            ClienteId = "CLI456",
            Itens = new List<ItemPedido>
            {
                new ItemPedido 
                { 
                    ProdutoId = "PROD1", 
                    Quantidade = 2, 
                    ValorUnitario = 100m,
                    CodigoProduto = "COD1",
                    DescricaoProduto = "Produto 1"
                }
            },
            ValorProdutos = 200m,
            ValorFrete = 20m,
            DataCriacao = DateTime.UtcNow
        };
        var cliente = new Cliente
        {
            Id = "CLI456",
            Nome = "Cliente Teste",
            CpfCnpj = "12345678901"
        };
        var impostos = new ImpostosCalculados(36m, 3.3m, 15.2m, 20m, 274.5m);
        string numeroNota = "000123";
        string chaveAcesso = "12345678901234567890123456789012345678901234";

        // Act
        var notaFiscal = NotaFiscalMapper.CriarNotaFiscal(
            pedido, cliente, impostos, chaveAcesso, numeroNota);

        // Assert
        Assert.Equal(pedido.Id, notaFiscal.PedidoId);
        Assert.Equal(numeroNota, notaFiscal.Numero);
        Assert.Equal(chaveAcesso, notaFiscal.ChaveAcesso);
        Assert.Equal(274.5m, notaFiscal.ValorTotal);
        Assert.Equal(impostos.ValorICMS, notaFiscal.ValorICMS);
        Assert.Equal(impostos.ValorIPI, notaFiscal.ValorIPI);
        Assert.Equal(impostos.ValorPIS, notaFiscal.ValorPIS);
        Assert.Equal(impostos.ValorCOFINS, notaFiscal.ValorCOFINS);
        Assert.Equal(StatusNotaFiscal.Processando, notaFiscal.Status);
    }

    [Fact]
    public void CriarNotaFiscal_DeveMapearTodosItens()
    {
        // Arrange
        var pedido = new Pedido
        {
            Id = "PED123",
            ClienteId = "CLI456",
            Itens = new List<ItemPedido>
            {
                new ItemPedido 
                { 
                    ProdutoId = "PROD1", 
                    Quantidade = 2, 
                    ValorUnitario = 100m,
                    CodigoProduto = "COD1",
                    DescricaoProduto = "Produto 1"
                },
                new ItemPedido 
                { 
                    ProdutoId = "PROD2", 
                    Quantidade = 1, 
                    ValorUnitario = 50m,
                    CodigoProduto = "COD2",
                    DescricaoProduto = "Produto 2"
                }
            },
            ValorProdutos = 250m,
            ValorFrete = 20m
        };
        var cliente = new Cliente { Id = "CLI456", Nome = "Cliente Teste", CpfCnpj = "123" };
        var impostos = new ImpostosCalculados(0, 0, 0, 0, 270m);

        // Act
        var notaFiscal = NotaFiscalMapper.CriarNotaFiscal(
            pedido, cliente, impostos, "12345678901234567890123456789012345678901234", "000123");

        // Assert
        Assert.Equal(2, notaFiscal.Itens.Count);
        Assert.Equal("PROD1", notaFiscal.Itens[0].ProdutoId);
        Assert.Equal(2, notaFiscal.Itens[0].Quantidade);
        Assert.Equal(100m, notaFiscal.Itens[0].ValorUnitario);
        Assert.Equal("PROD2", notaFiscal.Itens[1].ProdutoId);
    }

    [Fact]
    public void ParaEvento_ComNotaFiscalValida_DeveCriarEventoNotaFiscalEmitida()
    {
        // Arrange
        var notaFiscal = new NotaFiscal
        {
            Id = "NF123",
            PedidoId = "PED123",
            Numero = "000123",
            ChaveAcesso = "12345678901234567890123456789012345678901234",
            DataEmissao = DateTime.UtcNow,
            ValorTotal = 1000m
        };
        string protocolo = "PROT789";

        // Act
        var evento = NotaFiscalMapper.ParaEvento(notaFiscal, protocolo);

        // Assert
        Assert.Equal(notaFiscal.PedidoId, evento.PedidoId);
        Assert.Equal(notaFiscal.Id, evento.NotaFiscalId);
        Assert.Equal(notaFiscal.Numero, evento.NumeroNotaFiscal);
        Assert.Equal(notaFiscal.ChaveAcesso, evento.ChaveAcesso);
        Assert.Equal(protocolo, evento.ProtocoloAutorizacao);
        Assert.Equal(notaFiscal.ValorTotal, evento.ValorTotal);
    }
}
