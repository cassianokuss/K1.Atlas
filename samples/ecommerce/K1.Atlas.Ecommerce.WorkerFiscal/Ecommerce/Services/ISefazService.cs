namespace K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Services;

/// <summary>
/// Interface for SEFAZ (Sistema de Emissão de Nota Fiscal Eletrônica) integration
/// </summary>
public interface ISefazService
{
    /// <summary>
    /// Sends NotaFiscal to SEFAZ for authorization
    /// </summary>
    /// <param name="notaFiscal">NotaFiscal to be authorized</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Protocol number if authorized</returns>
    /// <exception cref="HttpRequestException">Thrown when SEFAZ communication fails</exception>
    Task<string> EnviarNotaAsync(NotaFiscal notaFiscal, CancellationToken cancellationToken = default);
}
