using K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce;
using K1.Atlas.Ecommerce.Contracts.Entities;
using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce;

public class NotaFiscalTest
{
    [Fact]
    public void Constructor_Should_Initialize_Default_Values()
    {
        // Arrange & Act
        var notaFiscal = new NotaFiscal();

        // Assert
        Assert.NotNull(notaFiscal.Id);
        Assert.NotEmpty(notaFiscal.Id);
        Assert.Empty(notaFiscal.Numero);
        Assert.Empty(notaFiscal.Serie);
        Assert.Empty(notaFiscal.PedidoId);
        Assert.Empty(notaFiscal.ClienteId);
        Assert.NotNull(notaFiscal.Itens);
        Assert.Empty(notaFiscal.Itens);
        Assert.Equal(0m, notaFiscal.ValorProdutos);
        Assert.Equal(0m, notaFiscal.ValorTotal);
        Assert.Equal(StatusNotaFiscal.Processando, notaFiscal.Status);
        Assert.Equal(0, notaFiscal.TentativasEnvio);
        Assert.NotNull(notaFiscal.ChaveAcesso);
        Assert.Empty(notaFiscal.ChaveAcesso);
    }

    [Fact]
    public void Constructor_Should_Set_All_Properties()
    {
        // Arrange & Act
        var notaFiscal = new NotaFiscal
        {
            Numero = "000001",
            Serie = "1",
            PedidoId = "507f1f77bcf86cd799439011",
            ClienteId = "507f1f77bcf86cd799439012",
            ChaveAcesso = "12345678901234567890123456789012345678901234",
            ValorProdutos = 1000m,
            ValorTotal = 1200m,
            DataEmissao = new DateTime(2026, 5, 7),
            Status = StatusNotaFiscal.Autorizada,
            TentativasEnvio = 1
        };

        // Assert
        Assert.Equal("000001", notaFiscal.Numero);
        Assert.Equal("1", notaFiscal.Serie);
        Assert.Equal("507f1f77bcf86cd799439011", notaFiscal.PedidoId);
        Assert.Equal("507f1f77bcf86cd799439012", notaFiscal.ClienteId);
        Assert.Equal("12345678901234567890123456789012345678901234", notaFiscal.ChaveAcesso);
        Assert.Equal(1000m, notaFiscal.ValorProdutos);
        Assert.Equal(1200m, notaFiscal.ValorTotal);
        Assert.Equal(new DateTime(2026, 5, 7), notaFiscal.DataEmissao);
        Assert.Equal(StatusNotaFiscal.Autorizada, notaFiscal.Status);
        Assert.Equal(1, notaFiscal.TentativasEnvio);
    }

    [Fact]
    public void ItemNotaFiscal_Should_Set_Properties()
    {
        // Arrange & Act
        var item = new ItemNotaFiscal
        {
            ProdutoId = "507f1f77bcf86cd799439013",
            CodigoProduto = "PROD001",
            DescricaoProduto = "Produto Teste",
            Quantidade = 2,
            ValorUnitario = 100m,
            ValorTotal = 200m,
            AliquotaICMS = 18m,
            ValorICMS = 36m,
            ValorPIS = 3.30m,
            ValorCOFINS = 15.20m,
            ValorIPI = 0m
        };

        // Assert
        Assert.Equal("507f1f77bcf86cd799439013", item.ProdutoId);
        Assert.Equal("PROD001", item.CodigoProduto);
        Assert.Equal("Produto Teste", item.DescricaoProduto);
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(100m, item.ValorUnitario);
        Assert.Equal(200m, item.ValorTotal);
        Assert.Equal(18m, item.AliquotaICMS);
        Assert.Equal(36m, item.ValorICMS);
        Assert.Equal(3.30m, item.ValorPIS);
        Assert.Equal(15.20m, item.ValorCOFINS);
        Assert.Equal(0m, item.ValorIPI);
    }

    [Fact]
    public void StatusNotaFiscal_Should_Have_Correct_Values()
    {
        // Arrange & Act & Assert
        Assert.Equal(0, (int)StatusNotaFiscal.Processando);
        Assert.Equal(1, (int)StatusNotaFiscal.Autorizada);
        Assert.Equal(2, (int)StatusNotaFiscal.Rejeitada);
    }

    [Fact]
    public void ChaveAcesso_Should_Accept_44_Characters()
    {
        // Arrange
        var chaveValida = "12345678901234567890123456789012345678901234"; // 44 chars

        // Act
        var notaFiscal = new NotaFiscal
        {
            ChaveAcesso = chaveValida
        };

        // Assert
        Assert.Equal(44, notaFiscal.ChaveAcesso.Length);
        Assert.Equal(chaveValida, notaFiscal.ChaveAcesso);
    }

    [Fact]
    public void NotaFiscal_Should_Store_Impostos_Values()
    {
        // Arrange & Act
        var notaFiscal = new NotaFiscal
        {
            ValorICMS = 180m,
            ValorPIS = 16.50m,
            ValorCOFINS = 76m,
            ValorIPI = 50m
        };

        // Assert
        Assert.Equal(180m, notaFiscal.ValorICMS);
        Assert.Equal(16.50m, notaFiscal.ValorPIS);
        Assert.Equal(76m, notaFiscal.ValorCOFINS);
        Assert.Equal(50m, notaFiscal.ValorIPI);
    }

    [Fact]
    public void NotaFiscal_Should_Track_TentativasEnvio()
    {
        // Arrange & Act
        var notaFiscal = new NotaFiscal
        {
            TentativasEnvio = 0
        };

        // Assert
        Assert.Equal(0, notaFiscal.TentativasEnvio);

        // Act - increment attempts
        notaFiscal.TentativasEnvio++;
        Assert.Equal(1, notaFiscal.TentativasEnvio);

        notaFiscal.TentativasEnvio++;
        Assert.Equal(2, notaFiscal.TentativasEnvio);
    }
}
