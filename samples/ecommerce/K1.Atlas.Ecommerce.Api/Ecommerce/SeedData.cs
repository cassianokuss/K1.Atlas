using MongoDB.Bson;

namespace K1.Atlas.Ecommerce.Api.Ecommerce;

public static class SeedData
{
    private static readonly string[] ClienteIds = { "cliente-bom-pagador-001", "cliente-limite-baixo-002", "cliente-inadimplente-003" };
    private static readonly string[] ProdutoIds = { "produto-notebook-001", "produto-mouse-002", "produto-teclado-003", "produto-monitor-004", "produto-webcam-005" };

    public static List<Cliente> GetClientes()
    {
        return new List<Cliente>
        {
            new Cliente
            {
                Id = ClienteIds[0],
                Nome = "João Silva - Bom Pagador",
                CpfCnpj = "12345678901",
                Email = "joao.silva@example.com",
                Endereco = "Rua A, 100",
                Cidade = "São Paulo",
                Estado = "SP",
                Cep = "01310-100",
                LimiteCredito = 10000m,
                CreditoUtilizado = 2000m,
                DataCadastro = DateTime.Now.AddYears(-2),
                Ativo = true
            },
            new Cliente
            {
                Id = ClienteIds[1],
                Nome = "Maria Santos - Limite Baixo",
                CpfCnpj = "98765432109",
                Email = "maria.santos@example.com",
                Endereco = "Av B, 200",
                Cidade = "Rio de Janeiro",
                Estado = "RJ",
                Cep = "20040-020",
                LimiteCredito = 5000m,
                CreditoUtilizado = 4800m,
                DataCadastro = DateTime.Now.AddMonths(-6),
                Ativo = true
            },
            new Cliente
            {
                Id = ClienteIds[2],
                Nome = "Pedro Costa - Inadimplente",
                CpfCnpj = "11122233344",
                Email = "pedro.costa@example.com",
                Endereco = "Praça C, 300",
                Cidade = "Belo Horizonte",
                Estado = "MG",
                Cep = "30110-000",
                LimiteCredito = 3000m,
                CreditoUtilizado = 3500m,
                DataCadastro = DateTime.Now.AddYears(-1),
                Ativo = true
            }
        };
    }

    public static List<Produto> GetProdutos()
    {
        return new List<Produto>
        {
            new Produto
            {
                Id = ProdutoIds[0],
                Codigo = "PROD001",
                Descricao = "Notebook Dell Inspiron 15",
                ValorUnitario = 3500.00m,
                EstoqueDisponivel = 10,
                AliquotaICMS = 18m,
                CalculaIPI = false,
                Ativo = true
            },
            new Produto
            {
                Id = ProdutoIds[1],
                Codigo = "PROD002",
                Descricao = "Mouse Logitech MX Master",
                ValorUnitario = 450.00m,
                EstoqueDisponivel = 25,
                AliquotaICMS = 18m,
                CalculaIPI = false,
                Ativo = true
            },
            new Produto
            {
                Id = ProdutoIds[2],
                Codigo = "PROD003",
                Descricao = "Teclado Mecânico Keychron K8",
                ValorUnitario = 650.00m,
                EstoqueDisponivel = 15,
                AliquotaICMS = 18m,
                CalculaIPI = false,
                Ativo = true
            },
            new Produto
            {
                Id = ProdutoIds[3],
                Codigo = "PROD004",
                Descricao = "Monitor LG UltraWide 34'",
                ValorUnitario = 2800.00m,
                EstoqueDisponivel = 3,
                AliquotaICMS = 18m,
                CalculaIPI = false,
                Ativo = true
            },
            new Produto
            {
                Id = ProdutoIds[4],
                Codigo = "PROD005",
                Descricao = "Webcam Logitech C920",
                ValorUnitario = 550.00m,
                EstoqueDisponivel = 20,
                AliquotaICMS = 18m,
                CalculaIPI = false,
                Ativo = true
            }
        };
    }
}
