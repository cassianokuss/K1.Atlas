# K1.Atlas

Biblioteca .NET para construção de sistemas distribuídos com suporte a mensageria, telemetria e persistência.

## 📁 Estrutura do Projeto

- **`/src/`** - Bibliotecas principais do K1.Atlas
  - `K1.Atlas.Domain` - Entidades e interfaces de domínio
  - `K1.Atlas.MongoDB` - Provider MongoDB
  - `K1.Atlas.PubSub` - Abstrações para mensageria
  - `K1.Atlas.PubSub.RabbitMQ` - Implementação RabbitMQ
  - `K1.Atlas.Telemetry` - OpenTelemetry integration
  - `K1.Atlas.Host` - Hosting utilities

- **`/samples/`** - Exemplos de uso completos
  - `ecommerce/` - Sistema de e-commerce distribuído

- **`/tests/`** - Testes automatizados
  - `K1.Atlas.UnitTest` - Testes unitários
  - `K1.Atlas.IntegrationTest` - Testes de integração

## 🚀 Quick Start

Veja os exemplos completos em [`/samples/`](./samples/README.md).

## 🧪 Testes

```bash
# Rodar todos os testes
dotnet test

# Apenas testes unitários
dotnet test tests/K1.Atlas.UnitTest/

# Apenas testes de integração
dotnet test tests/K1.Atlas.IntegrationTest/
```

## 📦 Pacotes NuGet

(A ser publicado)

## 📄 Licença

(Definir licença)