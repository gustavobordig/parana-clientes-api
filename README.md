# Clientes API

Microsserviço de **cadastro de clientes** do fluxo de concessão de crédito. Recebe o cadastro via REST, valida os dados e publica o evento `ClienteCadastrado`, que dispara a geração da proposta de crédito. Também oferece uma listagem de clientes protegida por autenticação JWT.

| | |
|---|---|
| **Swagger (produção)** | https://parana-clientes-api-production.up.railway.app/swagger |
| **Front da solução** | https://parana-credito-web.vercel.app |
| **Stack** | .NET 10 · ASP.NET Core · EF Core + SQL Server · MassTransit 8 + RabbitMQ · xUnit |

## Sumário

- [A solução completa](#a-solução-completa)
- [Endpoints](#endpoints)
  - [Regras de validação](#regras-de-validação)
  - [Evento publicado: ClienteCadastrado](#evento-publicado-clientecadastrado)
- [Autenticação](#autenticação)
- [Arquitetura](#arquitetura)
- [Decisões técnicas](#decisões-técnicas)
- [Como executar localmente](#como-executar-localmente)
  - [Variáveis de ambiente](#variáveis-de-ambiente)
- [Testes](#testes)
- [Deploy](#deploy)
- [Próximos passos](#próximos-passos)

## A solução completa

O fluxo é dividido em três microsserviços independentes, um por repositório, que se comunicam apenas por eventos:

| Serviço | Responsabilidade | Repositório |
|---|---|---|
| **Clientes** | Cadastra o cliente e publica `ClienteCadastrado` | este |
| **Propostas** | Calcula o score, decide o crédito e publica `PropostaGerada` | [parana-propostas-api](https://github.com/gustavobordig/parana-propostas-api) |
| **Cartões** | Emite os cartões da proposta aprovada | [parana-cartoes-api](https://github.com/gustavobordig/parana-cartoes-api) |
| Front | Cadastro e acompanhamento do fluxo em tempo real | [parana-credito-web](https://github.com/gustavobordig/parana-credito-web) |

![Fluxograma da solução](docs/fluxograma.svg)

Versão editável: [`docs/fluxograma.drawio`](docs/fluxograma.drawio), que abre no [draw.io](https://app.diagrams.net).

## Endpoints

| Método | Rota | Respostas |
|---|---|---|
| `POST` | `/api/v1/clientes` | `201` com `Location` · `400` dados inválidos · `409` CPF já cadastrado |
| `GET` | `/api/v1/clientes/{id}` | `200` (CPF mascarado) · `404` |
| `GET` | `/api/v1/clientes?pagina=1&tamanho=20` | 🔒 `200` lista paginada · `400` paginação inválida · `401` sem token |
| `POST` | `/api/v1/auth/login` | `200` com o token · `401` credenciais inválidas · `429` excesso de tentativas |
| `GET` | `/health` | `Healthy` quando SQL Server e RabbitMQ estão acessíveis |

```bash
curl -X POST https://parana-clientes-api-production.up.railway.app/api/v1/clientes \
  -H "Content-Type: application/json" \
  -d '{"nome":"Maria Silva","cpf":"529.982.247-25","email":"maria@exemplo.com","dataNascimento":"1990-05-10","rendaMensal":7500}'
```

Erros seguem o padrão **ProblemDetails** (RFC 9457) e trazem todos os problemas de uma vez:

```json
{
  "title": "Dados inválidos.",
  "status": 400,
  "erros": ["CPF inválido.", "Cliente deve ter pelo menos 18 anos."]
}
```

### Regras de validação
- Nome, CPF, e-mail, data de nascimento e renda são obrigatórios.
- CPF validado pelos dígitos verificadores; sequências repetidas (`111.111.111-11`) são rejeitadas; aceita com ou sem máscara.
- CPF único: checagem prévia + índice único no banco (cobre cadastros simultâneos).
- E-mail em formato válido, normalizado para minúsculas.
- Cliente com 18 anos ou mais; renda maior ou igual a zero.

### Evento publicado: `ClienteCadastrado`

```json
{ "clienteId": "guid", "nome": "Maria Silva", "rendaMensal": 7500.00, "ocorridoEm": "2026-10-29T10:00:00Z", "versao": 1 }
```

## Autenticação

A listagem de clientes expõe dados de todos os cadastros, por isso exige um **token JWT de moderador**. Cadastro e consulta por id continuam públicos: o fluxo da tela depende deles, e o id é um GUID que não dá para adivinhar.

Há **um único usuário moderador**, configurado por variáveis de ambiente. As credenciais de produção não ficam no repositório e são informadas à parte.

**Pelo Swagger:**
1. Chame `POST /api/v1/auth/login` com usuário e senha.
2. Copie o `accessToken` da resposta.
3. Clique em **Authorize** e cole o token.
4. Chame `GET /api/v1/clientes`.

**Por linha de comando:**

```bash
curl -X POST https://parana-clientes-api-production.up.railway.app/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"usuario":"<usuario>","senha":"<senha>"}'
```

```json
{ "accessToken": "eyJhbGciOi...", "tokenType": "Bearer", "expiraEm": "2026-10-29T15:00:00+00:00" }
```

```bash
curl "https://parana-clientes-api-production.up.railway.app/api/v1/clientes?pagina=1&tamanho=20" \
  -H "Authorization: Bearer <accessToken>"
```

```json
{
  "itens": [{ "id": "guid", "nome": "Maria Silva", "cpf": "***.982.247-**", "email": "maria@exemplo.com", "dataNascimento": "1990-05-10", "rendaMensal": 7500.00, "criadoEm": "2026-10-29T10:00:00+00:00" }],
  "pagina": 1, "tamanho": 20, "total": 1, "totalPaginas": 1
}
```

| Proteção | Como funciona |
|---|---|
| **JWT assinado** (HS256) | Emissor, audiência, assinatura e validade conferidos em toda requisição. O token vale 1 hora e traz o papel `moderador`. |
| **Autorização por papel** | A listagem exige `[Authorize(Roles = "moderador")]`. |
| **Comparação em tempo constante** | Usuário e senha são comparados pelo hash, com `CryptographicOperations.FixedTimeEquals`, para não vazar informação pelo tempo de resposta. |
| **Rate limit no login** | 5 tentativas por minuto por IP; depois disso a API responde `429`. |
| **Validação na inicialização** | A API não sobe sem usuário, senha e uma chave de assinatura de pelo menos 32 bytes. |
| **Paginação** | No máximo 100 itens por página, ordenados do cadastro mais recente para o mais antigo, com índice em `CriadoEm`. |

## Arquitetura

Clean Architecture em quatro camadas, mais um projeto só com os contratos de evento:

```
src/
├── Clientes.Api             Controllers, autenticação JWT, rate limit, ProblemDetails, Swagger, versionamento
├── Clientes.Application     Casos de uso (cadastrar, consultar, listar, autenticar) e portas
├── Clientes.Domain          Entidade Cliente e value objects Cpf e Email, sem dependências externas
├── Clientes.Infrastructure  EF Core, migrations, MassTransit, outbox, emissão e verificação de credenciais
└── Clientes.Contracts       Records dos eventos de integração
tests/
└── Clientes.Tests.Unit      xUnit + NSubstitute
```

Regra de dependência: `Api → Application → Domain` e `Infrastructure → Application`. O domínio não conhece banco, mensageria nem HTTP.

## Decisões técnicas

| Decisão | Motivo |
|---|---|
| **Transactional Outbox** (MassTransit + EF Core) | Cliente e evento são gravados na mesma transação; um processo de fundo entrega o evento ao RabbitMQ. Se o broker estiver fora, nenhum cadastro fica sem evento. |
| **MassTransit 8** | Abstrai topologia, retry e dead-letter; trocar RabbitMQ por Azure Service Bus é configuração. Fixado na v8 por licença (a v9 é comercial). |
| **Exchanges com nome explícito** (`cliente-cadastrado`) | A topologia não depende do namespace .NET; os contratos são duplicados em cada repositório com o mesmo namespace. |
| **Value objects** `Cpf` e `Email` | Validação e normalização num só lugar; o EF Core persiste via `ValueConverter`. |
| **Fluent API** (`IEntityTypeConfiguration`) | Mapeamento fora do domínio; índice único de CPF, `char(11)`, `decimal(18,2)`. |
| **LGPD** | CPF não trafega no evento (os consumidores não precisam dele) e sai mascarado na API e nos logs (`***.982.247-**`). |
| **Correlation ID** | O header `X-Correlation-Id` entra nos logs e é propagado nos eventos: um cadastro é rastreável pelos três serviços. |
| **Serilog** | Logs estruturados em JSON em produção, legíveis em desenvolvimento. |
| **JWT com um moderador** | Protege o único endpoint que expõe dados em massa, sem a complexidade de um cadastro de usuários. |
| **Versionamento na URL** (`/api/v1`) | Permite evoluir para `v2` sem quebrar clientes. |
| **Pacotes centralizados** (`Directory.Packages.props`) | Uma versão por pacote; `System.Security.Cryptography.Pkcs` fixado para corrigir vulnerabilidades transitivas do SqlClient. |

## Como executar localmente

**Pré-requisitos:** .NET 10 SDK e Docker.

Este repositório sobe a infraestrutura compartilhada do fluxo (RabbitMQ e SQL Server) e cria a rede Docker `parana-credito`, que os outros serviços usam. **Suba este primeiro.**

```bash
docker compose up -d --build
```

| | |
|---|---|
| Swagger | http://localhost:5101/swagger |
| RabbitMQ (painel) | http://localhost:15672 (usuário e senha `guest`) |
| SQL Server | `localhost,1433` (usuário `sa`, senha `Parana@2026`) |

Para rodar a API fora do Docker, suba só a infraestrutura e use o `dotnet run`. As migrations são aplicadas na inicialização.

```bash
docker compose up -d rabbitmq sqlserver
```

```bash
dotnet run --project src/Clientes.Api
```

### Variáveis de ambiente

| Variável | Descrição | Padrão local |
|---|---|---|
| `ConnectionStrings__SqlServer` | Conexão com o SQL Server | `Server=localhost,1433;Database=clientes_db;...` |
| `RabbitMq__Url` | URL AMQP do RabbitMQ | `amqp://guest:guest@localhost:5672/` |
| `Cors__Origens__0` | Origem liberada para o front | `http://localhost:5173` |
| `Database__AplicarMigrations` | Aplica as migrations na inicialização | `true` |
| `PORT` | Porta HTTP (usada pelo Railway) | `8080` no container |
| `Autenticacao__Usuario` | Usuário do moderador | `moderador` |
| `Autenticacao__Senha` | Senha do moderador | `moderador-local` |
| `Autenticacao__ChaveAssinatura` | Chave HS256 dos tokens, com pelo menos 32 bytes | chave de desenvolvimento |
| `Autenticacao__ExpiracaoMinutos` | Validade do token | `60` |

Os valores locais de autenticação estão em `appsettings.Development.json` e no `docker-compose.yml`, e servem apenas para desenvolvimento. Em produção, os três primeiros são obrigatórios.

## Testes

```bash
dotnet test
```

71 testes de unidade com xUnit, no padrão AAA e com nomes `Metodo_Cenario_ResultadoEsperado`:

- **Cpf:** válido com e sem máscara, dígitos verificadores errados, sequências repetidas, tamanhos inválidos, máscara LGPD.
- **Email:** normalização e formatos inválidos.
- **Cliente:** criação válida, idade-limite de 18 anos (inclusive aniversário em 29/02), renda negativa, vários erros reportados juntos.
- **Cadastro:** salva e publica o evento; publica antes de salvar (para entrar no outbox); CPF duplicado não salva nem publica.
- **Consulta:** cliente encontrado com CPF mascarado e cliente inexistente.
- **Listagem:** página com totais e CPF mascarado, página vazia, paginação inválida sem consultar o banco.
- **Autenticação:** login válido gera o token; credenciais erradas ou ausentes são recusadas sem gerar token.
- **Credenciais:** usuário sem diferença de maiúsculas, senha com diferença, valores vazios.
- **JWT:** expiração configurada, emissor, audiência e papel corretos, rejeição com outra chave, identificador único por token.

O CI (GitHub Actions) roda build e testes a cada push e pull request.

## Deploy

As três APIs rodam no **Railway**, cada uma a partir do próprio `Dockerfile`, junto com SQL Server, MongoDB e RabbitMQ no mesmo projeto. O front está na Vercel.

![Projeto no Railway: as três APIs, SQL Server, MongoDB e RabbitMQ](docs/deploy-railway.webp)

As setas mostram as dependências de cada serviço: Clientes e Propostas usam o SQL Server (`server`), Cartões usa o MongoDB, e as três se conectam ao RabbitMQ.

## Próximos passos

- Testes de integração com Testcontainers (SQL Server e RabbitMQ reais).
- Usuários e papéis persistidos, com senha em hash, no lugar do moderador único.
- Provisionar a topologia do RabbitMQ no deploy, para não depender de os consumidores já terem subido.
- Rastreamento distribuído com OpenTelemetry.
