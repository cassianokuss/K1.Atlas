# K1.Atlas - Samples

Este diretório contém exemplos de uso completos das bibliotecas K1.Atlas.

## 📁 Estrutura

### Ecommerce Sample

Exemplo completo de um sistema de e-commerce distribuído demonstrando:
- **API REST** - Criação de pedidos
- **Worker de Validação** - Validação de crédito
- **Worker de Estoque** - Reserva e liberação de estoque
- **Worker Fiscal** - Emissão de notas fiscais

#### Arquitetura

```
┌─────────────┐
│ Ecommerce   │
│ API         │──┐
└─────────────┘  │
                 │
                 v
         ┌───────────────┐
         │   RabbitMQ    │
         │  (Message     │
         │   Broker)     │
         └───────────────┘
                 │
        ┌────────┼────────┐
        │        │        │
        v        v        v
   ┌────────┐ ┌────────┐ ┌────────┐
   │Worker  │ │Worker  │ │Worker  │
   │Validação│ │Estoque│ │Fiscal │
   └────────┘ └────────┘ └────────┘
        │        │        │
        └────────┼────────┘
                 v
         ┌───────────────┐
         │   MongoDB     │
         └───────────────┘
```

#### Tecnologias Utilizadas

- **.NET 10.0**
- **MongoDB** - Banco de dados
- **RabbitMQ** - Message broker
- **OpenTelemetry** - Observabilidade (logs, traces, métricas)
- **MediatR** - CQRS pattern

#### Pré-requisitos

- Docker e Docker Compose
- .NET 10.0 SDK (para desenvolvimento local)

#### Como Executar

1. **Via Docker Compose (Recomendado)**
   ```bash
   # Na raiz do projeto
   docker-compose up -d
   ```

2. **Verificar se os serviços estão rodando**
   ```bash
   docker-compose ps
   ```

3. **Acessar a API**
   - API: http://localhost:5000
   - Swagger: http://localhost:5000/swagger
   - RabbitMQ Management: http://localhost:15672 (admin/admin123)

4. **Testar o fluxo completo**
   ```bash
   # Criar um pedido
   curl -X POST http://localhost:5000/pedidos \
     -H "Content-Type: application/json" \
     -d '{
       "clienteId": "123",
       "itens": [
         {"produtoId": "prod-1", "quantidade": 2}
       ]
     }'
   ```

5. **Acompanhar os logs**
   ```bash
   docker-compose logs -f worker-validacao
   docker-compose logs -f worker-estoque
   docker-compose logs -f worker-fiscal
   ```

#### Fluxo de Processamento

1. **Cliente cria um pedido** via API REST
2. API publica mensagem `PedidoCriado` no RabbitMQ
3. **Worker Validação** consome a mensagem:
   - Valida crédito do cliente
   - Publica `PedidoAprovado` ou `PedidoRejeitado`
4. **Worker Estoque** consome `PedidoAprovado`:
   - Reserva estoque dos produtos
   - Publica `EstoqueReservado`
5. **Worker Fiscal** consome `EstoqueReservado`:
   - Emite nota fiscal
   - Publica `NotaFiscalEmitida`

#### Projetos

- **K1.Atlas.Ecommerce.Api** - API REST para criação de pedidos
- **K1.Atlas.Ecommerce.WorkerValidacao** - Worker para validação de crédito
- **K1.Atlas.Ecommerce.WorkerEstoque** - Worker para gestão de estoque
- **K1.Atlas.Ecommerce.WorkerFiscal** - Worker para emissão de notas fiscais

#### Observabilidade

Todos os serviços estão instrumentados com OpenTelemetry enviando:
- **Logs estruturados** - JSON format
- **Traces distribuídos** - Propagação de contexto entre serviços
- **Métricas** - Contadores de pedidos, notas fiscais, etc.

Configurado para exportar para Grafana Cloud (veja docker-compose.yml).

## 🛠️ Desenvolvimento Local

Para desenvolver localmente sem Docker:

1. **Iniciar infraestrutura** (MongoDB + RabbitMQ):
   ```bash
   docker-compose up mongodb rabbitmq -d
   ```

2. **Executar cada projeto**:
   ```bash
   cd samples/ecommerce/K1.Atlas.Ecommerce.Api
   dotnet run
   
   # Em outro terminal
   cd samples/ecommerce/K1.Atlas.Ecommerce.WorkerValidacao
   dotnet run
   
   # E assim por diante...
   ```

## 📚 Próximos Passos

- Explore o código-fonte de cada projeto
- Verifique os testes em `/tests/`
- Leia a documentação das bibliotecas em `/src/`
