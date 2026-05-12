using K1.Atlas.Ecommerce.Contracts.Entities;

namespace K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Services;

/// <summary>
/// SEFAZ service simulator for testing and development
/// Simulates 30% failure rate on first attempt
/// </summary>
public class SefazServiceSimulator : ISefazService
{
    private readonly Random _random = new();
    private readonly Dictionary<string, int> _attemptCounter = new();

    public async Task<string> EnviarNotaAsync(NotaFiscal notaFiscal, CancellationToken cancellationToken = default)
    {
        // Simulate network delay
        await Task.Delay(100, cancellationToken);

        // Track attempts per nota fiscal
        var key = notaFiscal.Id;
        if (!_attemptCounter.ContainsKey(key))
        {
            _attemptCounter[key] = 0;
        }
        _attemptCounter[key]++;

        var attemptNumber = _attemptCounter[key];

        // 30% failure rate on first attempt only
        if (attemptNumber == 1 && _random.Next(100) < 30)
        {
            throw new HttpRequestException("Timeout simulado - SEFAZ indisponível");
        }

        // Generate protocol number
        var protocolo = $"PROT{DateTime.UtcNow:yyyyMMddHHmmssfff}{_random.Next(1000, 9999)}";
        
        return protocolo;
    }
}
