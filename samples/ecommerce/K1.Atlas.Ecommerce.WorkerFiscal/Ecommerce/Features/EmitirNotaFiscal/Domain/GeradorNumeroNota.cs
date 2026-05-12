namespace K1.Atlas.Ecommerce.WorkerFiscal.Ecommerce.Features.EmitirNotaFiscal.Domain;

public static class GeradorNumeroNota
{
    public static string Gerar()
    {
        // Simple sequential number generation (in production, would use database sequence)
        return DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
    }
}
