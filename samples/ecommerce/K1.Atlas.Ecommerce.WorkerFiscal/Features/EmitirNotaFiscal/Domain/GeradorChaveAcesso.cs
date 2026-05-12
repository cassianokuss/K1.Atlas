namespace K1.Atlas.Ecommerce.WorkerFiscal.Features.EmitirNotaFiscal.Domain;

public static class GeradorChaveAcesso
{
    public static string Gerar()
    {
        // Generate 44-digit SEFAZ access key (simplified simulation)
        // Real format: UF + AAMM + CNPJ + Modelo + Serie + NumeroNF + Forma Emissao + Codigo + DV
        var random = new Random();
        return string.Concat(Enumerable.Range(0, 44).Select(_ => random.Next(0, 10)));
    }
}
