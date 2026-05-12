using System.Diagnostics;

namespace K1.Atlas.Ecommerce.WorkerValidacao.Services;

public class BureauCreditoSimulator : IBureauCreditoService
{
    public async Task<int> SimularConsultaAsync(string cpfCnpj, CancellationToken cancellationToken = default)
    {
        // Simulate HTTP call with 200ms delay
        await Task.Delay(200, cancellationToken);
        
        // Return random score between 300 and 850
        var score = Random.Shared.Next(300, 851);
        
        return score;
    }
}
