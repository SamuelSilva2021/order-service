# Decisões técnicas

## Arquitetura

Quatro camadas, com dependências apontando para o centro:

```
Api ──► Infrastructure ──► Application ──► Domain
 └────────────────────────────►┘
```

- **Domain** não depende de nada. Concentra invariantes e transições de estado.
- **Application** define os casos de uso e as portas (`IOrderRepository`, `IOrderQueries`, `IUnitOfWork`, ...). Não conhece EF Core.
- **Infrastructure** implementa as portas: EF Core, JWT, usuários.
- **Api** só traduz HTTP para casos de uso e erros para `ProblemDetails`.

A complexidade foi mantida baixa de propósito: **um caso de uso por classe** chamado direto pelo controller, sem MediatR, sem pipeline de behaviors e sem AutoMapper. Quem lê encontra o fluxo seguindo o código.

## Modelo de domínio

- `Order` é o aggregate root. Itens só entram na criação (`Order.Place`), então não há como violar o total depois. O total é calculado uma vez e persistido, para evitar somar itens em toda listagem.
- `OrderItem` guarda o preço unitário **no momento da compra**: mudar o preço do produto depois não altera pedidos antigos.
- `Currency` é um value object (código ISO de 3 letras, normalizado). Evita strings soltas e centraliza a validação.
- `Draft` existe no enum conforme o enunciado, mas o pedido **nasce `Placed`**: não há fluxo de edição de rascunho, então um estado `Draft` navegável seria código sem uso.
- Um produto só é vendido na sua própria moeda. Pedido em moeda diferente retorna 422.

### Estoque

`Product.AvailableQuantity` é o estoque disponível. O ciclo é:

| Momento | Efeito no estoque |
|---|---|
| Criar pedido | Apenas valida (`quantidade <= disponível`); **não reserva** |
| Confirmar | Baixa (`Reserve`) a quantidade dos itens |
| Cancelar pedido `Confirmed` | Devolve (`Release`) |
| Cancelar pedido `Placed` | Nada a devolver |

Reservar só na confirmação segue o enunciado ("confirmação deve reservar/baixar estoque") e evita que pedidos abandonados em `Placed` prendam estoque. A contrapartida é que a confirmação pode falhar se o estoque acabou depois da criação: nesse caso a API devolve 409 e o pedido continua `Placed`.

`Order.HoldsStock` (verdadeiro só em `Confirmed`) deixa explícito para o caso de uso de cancelamento quando há estoque a devolver.

## Idempotência

Resolvida pelo **próprio estado do agregado**, sem `Idempotency-Key`:

- `Order.Confirm()` e `Order.Cancel()` retornam `false` quando o pedido já está no estado alvo.
- O caso de uso só mexe no estoque e salva quando houve mudança real.
- Chamar duas vezes devolve 200 com o mesmo pedido, e o estoque é ajustado uma única vez.

Uma `Idempotency-Key` seria necessária para operações não naturalmente idempotentes (como `POST /orders`), que está fora do escopo do enunciado.

## Concorrência

Duas confirmações simultâneas disputando o mesmo estoque não podem vender a mais. Usei **concorrência otimista** com a coluna de sistema `xmin` do PostgreSQL como token em `orders` e `products`:

- Sem coluna extra para manter e sem lock pessimista.
- O segundo `SaveChanges` falha com `DbUpdateConcurrencyException`, convertida em `ConcurrencyConflictException` (HTTP 409). O cliente repete a chamada.
- Há um teste de integração que dispara duas confirmações concorrentes e garante que só uma tem sucesso.
- Uma `CHECK (AvailableQuantity >= 0)` no banco é a última barreira.

**Nota sobre migrations:** o provider Npgsql gera `xmin` como coluna em `CREATE TABLE`, mas `xmin` é coluna de sistema e o PostgreSQL rejeita. A migration inicial foi ajustada removendo essas linhas. O mapeamento no modelo (`HasColumnType("xid")`) continua correto.

## Persistência e performance

- **Escrita** (`IOrderRepository`, `IProductRepository`): carrega o agregado rastreado.
- **Leitura** (`IOrderQueries`, `IProductQueries`): `AsNoTracking` com projeção direta para DTO, sem materializar entidades. Separação simples de leitura e escrita, sem CQRS completo.
- Itens do pedido são *owned entities* em `order_items` (chave `OrderId + ProductId`), carregadas junto do pedido.
- A listagem de pedidos usa paginação por `Skip/Take` ordenada por `CreatedAt desc, Id`, com `pageSize` limitado a 100.
- Índices de `orders`: `(CustomerId, CreatedAt)`, `(Status, CreatedAt)` e `(CreatedAt)`, cobrindo os filtros do enunciado.
- Valores monetários em `numeric(18,2)`.
- A confirmação e o cancelamento carregam todos os produtos dos itens em **uma** query (`WHERE Id IN (...)`).
- Paginação por offset é suficiente aqui; para volumes muito grandes, a evolução natural é *keyset pagination*.

## Segurança

- JWT Bearer (HMAC-SHA256) emitido em `POST /auth/token`. Chave, emissor e audiência vêm de configuração e são validados na subida (`ValidateOnStart`).
- Usuários e papéis existem só para viabilizar autenticação; gestão de identidade está fora do escopo por se tratar de um projeto teste.
- Tabela `users` (`Username` único, `PasswordHash`, `Role`, `CustomerId`), populada pelo seed da migration. `Role` é um enum (`Admin`, `Customer`) gravado como texto; não há tabela de papéis nem ASP.NET Identity, que trariam muito mais do que o enunciado pede.
- Senhas guardadas com PBKDF2-HMAC-SHA256, salt aleatório por usuário e 600 mil iterações (recomendação OWASP), no formato `v1.<iterações>.<salt>.<hash>`, que permite aumentar o custo no futuro. A comparação é em tempo constante. Quando o usuário não existe, o login executa o mesmo trabalho de derivação, para que o tempo de resposta não revele quais usernames existem.
- O seed usa hashes pré-calculados com salt fixo, para a migration ser determinística. Um teste confirma que eles correspondem às senhas documentadas no README. Em produção, a chave JWT viria de um cofre de segredos e os usuários de um provedor de identidade.
- Perfis: `Admin` cadastra produtos e age em nome de qualquer cliente; `Customer` opera apenas sobre o seu `customerId`.
- O `customerId` do usuário vai para o token na claim `customer_id`. A regra de posse fica em `OrderAccessPolicy`, na camada Application, e depende apenas da abstração `ICurrentUser` (implementada na Api a partir das claims):
  - criar pedido para outro cliente: **403**;
  - consultar, confirmar ou cancelar pedido de outro cliente: **404**, para não revelar a existência do pedido;
  - listar: sempre restrito ao próprio cliente; pedir o `customerId` de outro retorna 403.

## Tratamento de erros

Um único `IExceptionHandler` converte exceções em `application/problem+json`. Os controllers ficam sem `try/catch`.

| Exceção | HTTP |
|---|---|
| `ValidationException` (FluentValidation) | 400 |
| `NotFoundException` | 404 |
| `InvalidCredentialsException` | 401 |
| `ForbiddenException` | 403 |
| `InsufficientStockException`, `InvalidOrderTransitionException`, `ConcurrencyConflictException` | 409 |
| Demais `DomainException` | 422 |
| Qualquer outra | 500 (detalhes só no log) |

Validação em duas camadas, cada uma no seu papel: FluentValidation valida **o formato da requisição** (campos obrigatórios, ranges, duplicidade); o domínio protege **as invariantes** mesmo que alguém o use sem passar pela API.

Produto inexistente no payload retorna 400 (e não 404), porque o recurso inválido está no corpo da requisição, não na rota.

## Operação

- `docker compose up` sobe PostgreSQL (com healthcheck) e a API, que só inicia depois do banco saudável.
- As migrations são aplicadas na subida (`Database.MigrateAsync`). Para uma única instância isso é simples e suficiente; com várias réplicas, o ideal seria um job de migração separado.
- O seed de produtos faz parte da migration (`HasData`), com ids fixos e documentados no README.
- Swagger fica habilitado em qualquer ambiente para facilitar a avaliação.
- Dockerfile multi-stage, com restore em camada separada (cache) e execução como usuário não-root.

## Testes

- **Domain**: invariantes, transições, idempotência do agregado, estoque.
- **Application**: fluxos dos casos de uso com repositórios substituídos (NSubstitute), incluindo "chamar duas vezes só ajusta o estoque uma vez".
- **Integração**: API real + PostgreSQL real (Testcontainers) para validar o que mocks não cobrem: tradução das queries, migrations, JWT, `xmin` e concorrência.

## Fora de escopo (conscientemente)

Refresh token, cadastro e gestão de usuários e papéis, `Idempotency-Key` em `POST /orders`, edição de pedido, outbox/eventos, observabilidade (métricas/tracing) e cache.
