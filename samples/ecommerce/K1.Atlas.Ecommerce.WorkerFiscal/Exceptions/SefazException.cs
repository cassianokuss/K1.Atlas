namespace K1.Atlas.Ecommerce.WorkerFiscal.Exceptions;

/// <summary>
/// Exception thrown when there is a SEFAZ communication or rejection error
/// </summary>
public class SefazException : Exception
{
    public string ChaveAcesso { get; }
    public string MotivoRejeicao { get; }
    public int TentativasRealizadas { get; }

    public SefazException(
        string chaveAcesso, 
        string motivoRejeicao, 
        int tentativasRealizadas)
        : base($"Erro SEFAZ - Chave: {chaveAcesso}, Motivo: {motivoRejeicao}, Tentativas: {tentativasRealizadas}")
    {
        ChaveAcesso = chaveAcesso;
        MotivoRejeicao = motivoRejeicao;
        TentativasRealizadas = tentativasRealizadas;
    }

    public SefazException(
        string chaveAcesso, 
        string motivoRejeicao, 
        int tentativasRealizadas, 
        Exception innerException)
        : base($"Erro SEFAZ - Chave: {chaveAcesso}, Motivo: {motivoRejeicao}, Tentativas: {tentativasRealizadas}", innerException)
    {
        ChaveAcesso = chaveAcesso;
        MotivoRejeicao = motivoRejeicao;
        TentativasRealizadas = tentativasRealizadas;
    }
}
