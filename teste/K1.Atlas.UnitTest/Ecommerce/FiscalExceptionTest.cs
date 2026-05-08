using Xunit;

namespace K1.Atlas.UnitTest.Ecommerce;

public class FiscalExceptionTest
{
    [Fact]
    public void SefazException_Constructor_Should_Set_Properties()
    {
        // Arrange
        var chaveAcesso = "35210812345678901234567890123456789012345678";
        var motivoRejeicao = "Falha na comunicação com SEFAZ";
        var tentativasRealizadas = 3;

        // Act
        var exception = new K1.Atlas.WorkerFiscal.Ecommerce.Exceptions.SefazException(
            chaveAcesso, 
            motivoRejeicao, 
            tentativasRealizadas);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(chaveAcesso, exception.ChaveAcesso);
        Assert.Equal(motivoRejeicao, exception.MotivoRejeicao);
        Assert.Equal(tentativasRealizadas, exception.TentativasRealizadas);
        Assert.Contains(chaveAcesso, exception.Message);
        Assert.Contains(motivoRejeicao, exception.Message);
        Assert.Contains(tentativasRealizadas.ToString(), exception.Message);
    }

    [Fact]
    public void SefazException_Should_Derive_From_Exception()
    {
        // Arrange
        var exception = new K1.Atlas.WorkerFiscal.Ecommerce.Exceptions.SefazException(
            "35210812345678901234567890123456789012345678", 
            "Timeout", 
            3);

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void SefazException_Constructor_With_InnerException_Should_Set_All_Properties()
    {
        // Arrange
        var chaveAcesso = "35210812345678901234567890123456789012345678";
        var motivoRejeicao = "Serviço indisponível";
        var tentativasRealizadas = 5;
        var innerException = new InvalidOperationException("Network error");

        // Act
        var exception = new K1.Atlas.WorkerFiscal.Ecommerce.Exceptions.SefazException(
            chaveAcesso, 
            motivoRejeicao, 
            tentativasRealizadas, 
            innerException);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(chaveAcesso, exception.ChaveAcesso);
        Assert.Equal(motivoRejeicao, exception.MotivoRejeicao);
        Assert.Equal(tentativasRealizadas, exception.TentativasRealizadas);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void EmissaoNotaFiscalException_Constructor_Should_Set_Properties()
    {
        // Arrange
        var pedidoId = "507f1f77bcf86cd799439011";
        var message = "Erro ao emitir nota fiscal: dados do cliente inválidos";

        // Act
        var exception = new K1.Atlas.WorkerFiscal.Ecommerce.Exceptions.EmissaoNotaFiscalException(
            pedidoId, 
            message);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(pedidoId, exception.PedidoId);
        Assert.Contains(pedidoId, exception.Message);
        Assert.Contains(message, exception.Message);
    }

    [Fact]
    public void EmissaoNotaFiscalException_Should_Derive_From_Exception()
    {
        // Arrange
        var exception = new K1.Atlas.WorkerFiscal.Ecommerce.Exceptions.EmissaoNotaFiscalException(
            "507f1f77bcf86cd799439011", 
            "Erro no cálculo de impostos");

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void EmissaoNotaFiscalException_Constructor_With_InnerException_Should_Set_All_Properties()
    {
        // Arrange
        var pedidoId = "507f1f77bcf86cd799439011";
        var message = "Falha ao calcular ICMS";
        var innerException = new ArgumentException("Alíquota inválida");

        // Act
        var exception = new K1.Atlas.WorkerFiscal.Ecommerce.Exceptions.EmissaoNotaFiscalException(
            pedidoId, 
            message, 
            innerException);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(pedidoId, exception.PedidoId);
        Assert.Contains(pedidoId, exception.Message);
        Assert.Contains(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void EmissaoNotaFiscalException_Constructor_With_Only_PedidoId_Should_Use_Default_Message()
    {
        // Arrange
        var pedidoId = "507f1f77bcf86cd799439011";

        // Act
        var exception = new K1.Atlas.WorkerFiscal.Ecommerce.Exceptions.EmissaoNotaFiscalException(pedidoId);

        // Assert
        Assert.NotNull(exception);
        Assert.Equal(pedidoId, exception.PedidoId);
        Assert.Contains(pedidoId, exception.Message);
        Assert.Contains("emissão", exception.Message.ToLower());
    }
}
