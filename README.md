# Order Service

API REST para gestão de pedidos com itens e controle de estoque. .NET 8, ASP.NET Core, EF Core (migrations) e PostgreSQL.

As decisões técnicas e seus motivos estão em [docs/decisions.md](docs/decisions.md).

## Subir com Docker

```bash
docker compose up --build
```

- API: http://localhost:8080
- Swagger (com botão **Authorize** para o JWT): http://localhost:8080/swagger
- PostgreSQL: `localhost:5432` (`postgres` / `postgres`, banco `orderservice`)

As migrations e o seed de produtos são aplicados automaticamente na inicialização da API.

## Rodar localmente

Requisitos: SDK .NET 8+ e um PostgreSQL acessível.

```bash
docker compose up -d db
dotnet run --project src/OrderService.Api
```

A API sobe em http://localhost:5080/swagger. A connection string padrão (`appsettings.json`) aponta para `localhost:5432`.

## Testes

```bash
dotnet test
```

| Projeto | O que cobre |
|---|---|
| `OrderService.Domain.Tests` | Invariantes e transições de `Order`, `OrderItem`, `Product`, `Currency` e `User` |
| `OrderService.Application.Tests` | Casos de uso (pedidos e login) e regra de posse por cliente, com dependências substituídas |
| `OrderService.Api.IntegrationTests` | API completa contra um PostgreSQL real (Testcontainers), incluindo JWT, posse por cliente, idempotência, concorrência e o hasher de senha |

Os testes de integração precisam do **Docker em execução**: eles sobem um container PostgreSQL descartável.

## Usando a API

Usuários de demonstração (tabela `users`, criados pela migration com a senha em hash PBKDF2):

| Usuário | Senha | Perfil | Permissões |
|---|---|---|---|
| `admin` | `admin123` | Admin | Tudo, inclusive cadastrar produtos |
| `customer` | `customer123` | Customer | Pedidos do cliente `customer-001` e consulta de produtos |
| `customer2` | `customer123` | Customer | Pedidos do cliente `customer-002` e consulta de produtos |

O token traz o perfil e, para clientes, a claim `customer_id`. Um **Customer** só cria pedidos para o seu próprio `customerId` (403 caso contrário), só enxerga, confirma e cancela os seus pedidos (404 nos de outros) e a listagem é sempre restrita aos seus pedidos. O **Admin** age em nome de qualquer cliente.

Produtos do seed:

| Id | Produto | Preço (BRL) | Estoque |
|---|---|---|---|
| `11111111-1111-1111-1111-111111111111` | Mechanical Keyboard | 349,90 | 50 |
| `22222222-2222-2222-2222-222222222222` | Wireless Mouse | 129,90 | 100 |
| `33333333-3333-3333-3333-333333333333` | 27-inch Monitor | 1599,00 | 15 |
| `44444444-4444-4444-4444-444444444444` | Noise Cancelling Headset | 799,50 | 30 |
| `55555555-5555-5555-5555-555555555555` | Full HD Webcam | 289,00 | 0 |

### Endpoints

| Método | Rota | Acesso | Descrição |
|---|---|---|---|
| POST | `/auth/token` | Público | Emite o JWT (usuário e senha) |
| POST | `/products` | Admin | Cadastra produto |
| GET | `/products?page=&pageSize=` | Autenticado | Lista produtos paginados |
| POST | `/orders` | Autenticado | Cria pedido (nasce `Placed`) |
| POST | `/orders/{id}/confirm` | Autenticado | Confirma e reserva estoque (idempotente) |
| POST | `/orders/{id}/cancel` | Autenticado | Cancela e libera estoque reservado (idempotente) |
| GET | `/orders/{id}` | Autenticado | Pedido com itens |
| GET | `/orders?customerId=&status=&from=&to=&page=&pageSize=` | Autenticado | Lista paginada com filtros |

### Exemplo com curl

```bash
TOKEN=$(curl -s -X POST http://localhost:8080/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' | sed -E 's/.*"accessToken":"([^"]+)".*/\1/')

curl -s -X POST http://localhost:8080/orders \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"customerId":"c-1","currency":"BRL","items":[{"productId":"11111111-1111-1111-1111-111111111111","quantity":2}]}'
```

### Códigos de resposta

| Código | Quando |
|---|---|
| 400 | Payload inválido, produto inexistente, filtros ou paginação inválidos |
| 401 / 403 | Sem token ou credenciais inválidas / sem permissão (inclui Customer operando em nome de outro cliente) |
| 404 | Pedido inexistente ou pertencente a outro cliente |
| 409 | Estoque insuficiente, transição de estado inválida ou conflito de concorrência (repetir a chamada) |
| 422 | Demais regras de negócio violadas (ex.: moeda diferente da do produto) |

Erros seguem o formato `application/problem+json`, com títulos e mensagens em português (ver [decisions.md](docs/decisions.md#idioma-das-respostas)).

## Estrutura

```
src/
  OrderService.Domain          Entidades, value object e regras de negócio (sem dependências)
  OrderService.Application     Casos de uso, DTOs, validações e contratos (portas)
  OrderService.Infrastructure  EF Core, migrations, repositórios, queries e JWT
  OrderService.Api             Controllers, autenticação, tratamento de erros e Swagger
tests/
  OrderService.Domain.Tests
  OrderService.Application.Tests
  OrderService.Api.IntegrationTests
```

## Migrations

O `dotnet-ef` está fixado em `dotnet-tools.json`:

```bash
dotnet tool restore
dotnet tool run dotnet-ef migrations add <Nome> -p src/OrderService.Infrastructure -s src/OrderService.Api -o Persistence/Migrations
```

Atenção: ao gerar uma migration que **crie** tabelas, remova da migration as colunas `xmin` (ver [decisions.md](docs/decisions.md#concorrência)).
