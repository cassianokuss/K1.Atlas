using K1.Atlas.Ecommerce.Contracts.Entities;

namespace K1.Atlas.Ecommerce.WorkerValidacao.Ecommerce.Features.ValidarCredito.Domain;

public static class ValidadorCreditoCliente
{
    public static decimal CalcularLimiteDisponivel(Cliente cliente)
    {
        return cliente.LimiteCredito - cliente.CreditoUtilizado;
    }

    public static bool TemLimite(Cliente cliente, decimal valorRequerido)
    {
        var limiteDisponivel = CalcularLimiteDisponivel(cliente);
        return limiteDisponivel >= valorRequerido;
    }
}
