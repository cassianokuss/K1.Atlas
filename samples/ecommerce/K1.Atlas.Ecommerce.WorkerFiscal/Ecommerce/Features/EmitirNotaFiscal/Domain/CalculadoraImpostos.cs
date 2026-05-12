using K1.Atlas.Ecommerce.Contracts.Entities;

namespace K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Features.EmitirNotaFiscal.Domain;

public static class CalculadoraImpostos
{
    private const decimal AliquotaICMS = 0.18m; // 18%
    private const decimal AliquotaPIS = 0.0165m; // 1.65%
    private const decimal AliquotaCOFINS = 0.076m; // 7.6%
    private const decimal AliquotaIPI = 0.10m; // 10%

    public static ImpostosCalculados Calcular(decimal valorBase)
    {
        var valorICMS = Math.Round(valorBase * AliquotaICMS, 2);
        var valorPIS = Math.Round(valorBase * AliquotaPIS, 2);
        var valorCOFINS = Math.Round(valorBase * AliquotaCOFINS, 2);
        var valorIPI = Math.Round(valorBase * AliquotaIPI, 2);
        var valorTotal = valorBase + valorICMS + valorPIS + valorCOFINS + valorIPI;

        return new ImpostosCalculados(valorICMS, valorPIS, valorCOFINS, valorIPI, valorTotal);
    }

    public static ImpostosItem CalcularPorItem(decimal valorItem)
    {
        return new ImpostosItem(
            ValorICMS: Math.Round(valorItem * AliquotaICMS, 2),
            ValorPIS: Math.Round(valorItem * AliquotaPIS, 2),
            ValorCOFINS: Math.Round(valorItem * AliquotaCOFINS, 2),
            ValorIPI: Math.Round(valorItem * AliquotaIPI, 2)
        );
    }

    public static decimal ObterAliquotaICMS() => AliquotaICMS;
}

public record ImpostosCalculados(
    decimal ValorICMS,
    decimal ValorPIS,
    decimal ValorCOFINS,
    decimal ValorIPI,
    decimal ValorTotal);

public record ImpostosItem(
    decimal ValorICMS,
    decimal ValorPIS,
    decimal ValorCOFINS,
    decimal ValorIPI);
