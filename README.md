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

A API sobe em http://localhost:5080/swagger. A connection string padrão (`appsettings.json`) aponta para `localhost:5432` com o usuário e a senha do `docker-compose.yml`.

Para usar outro PostgreSQL (por exemplo, um instalado na sua máquina) sem alterar arquivos versionados, guarde a connection string nos *user secrets*:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=meu_banco;Username=postgres;Password=sua_senha" --project src/OrderService.Api
```

## Solução de problemas

**`port is already allocated` ou `address already in use` ao rodar `docker compose up`.** Alguma porta do host já está ocupada. A mais comum é a **5432**, quando há um PostgreSQL instalado na máquina. Edite o `docker-compose.yml` e troque só o número da esquerda do mapeamento:

| Serviço | Mapeamento original | Exemplo de troca | Acesso depois da troca |
|---|---|---|---|
| Banco | `"5432:5432"` | `"5433:5432"` | `localhost:5433` (pgAdmin, `dotnet run` local) |
| API | `"8080:8080"` | `"8081:8080"` | http://localhost:8081/swagger |

A API que roda dentro do compose continua falando com o banco pela rede interna (`db:5432`), então nada mais precisa mudar. Se você rodar a API fora do Docker com o banco na nova porta, informe-a pelos *user secrets*:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5433;Database=orderservice;Username=postgres;Password=postgres" --project src/OrderService.Api
```

**`Docker is either not running or misconfigured` ao rodar `dotnet test`.** Os testes de integração sobem um PostgreSQL descartável via Docker. Inicie o Docker (Docker Desktop) e rode `dotnet test` de novo.

**A API do Docker não reflete uma mudança no código.** O `docker compose up` reaproveita a imagem antiga. Use `docker compose up --build`.

## Testes

```bash
dotnet test
```

| Projeto | O que cobre |
|---|---|
| `OrderService.Domain.Tests` | Invariantes e transições de `Order`, `OrderItem`, `Product`, `Currency` e `User` |
| `OrderService.Application.Tests` | Handlers (pedidos e login) e regra de posse por cliente, com dependências substituídas |
| `OrderService.Api.IntegrationTests` | API completa contra um PostgreSQL real (Testcontainers), incluindo JWT, posse por cliente, idempotência, concorrência e o hasher de senha |

Os testes de integração precisam do **Docker em execução**: eles sobem um container PostgreSQL descartável.

## Usando a API

Usuários de demonstração (tabela `users`, criados pela migration com a senha em hash PBKDF2):

| Usuário | Senha | Perfil | Permissões |
|---|---|---|---|
| `admin` | `admin123` | Admin | Tudo, inclusive cadastrar produtos |
| `customer` | `customer123` | Customer | Pedidos do cliente `customer-001` e consulta de produtos |
| `customer2` | `customer123` | Customer | Pedidos do cliente `customer-002` e consulta de produtos |

A resposta do login já informa quem você é, inclusive o `customerId` que deve ser usado ao criar pedidos:

```json
{ "accessToken": "...", "expiresAt": "...",
  "user": { "username": "customer", "role": "Customer", "customerId": "customer-001" } }
```

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

### Exemplos com curl

**Como cliente** (usuário `customer`, que opera o `customerId` `customer-001`): criar, confirmar e consultar um pedido.

```bash
TOKEN=$(curl -s -X POST http://localhost:8080/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"customer","password":"customer123"}' | sed -E 's/.*"accessToken":"([^"]+)".*/\1/')

curl -s -X POST http://localhost:8080/orders \
  -H "Authorization: Bearer $TOKEN" -H "Content-Type: application/json" \
  -d '{"customerId":"customer-001","currency":"BRL","items":[{"productId":"11111111-1111-1111-1111-111111111111","quantity":2}]}'
```

Use o `id` da resposta para confirmar e consultar (troque `<id>`):

```bash
curl -s -X POST http://localhost:8080/orders/<id>/confirm -H "Authorization: Bearer $TOKEN"
curl -s http://localhost:8080/orders/<id> -H "Authorization: Bearer $TOKEN"
```

**Como administrador** (usuário `admin`): cadastrar um produto, operação que o `customer` não pode fazer (403).

```bash
ADMIN_TOKEN=$(curl -s -X POST http://localhost:8080/auth/token \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' | sed -E 's/.*"accessToken":"([^"]+)".*/\1/')

curl -s -X POST http://localhost:8080/products \
  -H "Authorization: Bearer $ADMIN_TOKEN" -H "Content-Type: application/json" \
  -d '{"name":"Teclado Gamer","unitPrice":199.90,"currency":"BRL","availableQuantity":30}'
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
  OrderService.Application     Comandos, consultas e handlers por feature, validações e contratos (portas)
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
