namespace K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce.Services;

public interface IBureauCreditoService
{
    Task<int> SimularConsultaAsync(string cpfCnpj, CancellationToken cancellationToken = default);
}
