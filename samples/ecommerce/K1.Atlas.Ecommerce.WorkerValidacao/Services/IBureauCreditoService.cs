namespace K1.Atlas.Ecommerce.WorkerValidacao.Services;

public interface IBureauCreditoService
{
    Task<int> SimularConsultaAsync(string cpfCnpj, CancellationToken cancellationToken = default);
}
